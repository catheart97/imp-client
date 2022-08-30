using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


namespace IMP
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(ObjectManipulator))]
    [RequireComponent(typeof(NearInteractionGrabbable))]
    public class IMPMovableObject : IMPObject
    {

        /////////
        // nested
        /////////
        enum MotionPlanningState
        {
            Rapair,
            RequestExploration,
            Exploration
        };

        enum SmoothState
        {
            Active,
            Sleep
        };

        /////////
        // data
        /////////

        // the object manipulator (for path tracking) 
        //// todo: Disable ability to move during calculation phases etc
        ObjectManipulator _object_manipulator = null;

        // visualization helper

        public static List<GameObject> Companions = new List<GameObject>();
        GameObject _companion_object = null;
        GameObject _path_object = null;
        GameObject _user_path_object = null;

        Material _original_material = null;
        Material _ghost_material = null;
        bool _dirty_path = false;

        int _animation_index = 0;
        Configuration _animation_start;
        Configuration _animation_end;
        float _lerp_value = 0.0f;
        float _lerp_step = 0.0f;
        bool _load_animation_data = true;

        // motion planning 
        bool _manipulation = false;
        Thread _planning_thread = null;
        Thread _smoothing_thread = null;
        ConfigurationHistory _user_history = null;
        ConfigurationHistory _planned_history = null;
        bool _planned_once = false;

        bool _blink_ghost = false;
        int _update_counter = 0;

        MotionPlanningState _planning_state = MotionPlanningState.Rapair;

        /////////
        // unity properties
        /////////

        [Header("General Settings and Configuration")]
        [Tooltip("Determines whether to visualize the planned path.")]
        public bool VisualizePlannedPath = true;

        [Tooltip("Determines whether to visualize the user path.")]
        public bool VisualizeUserPath = true;

        [Tooltip("Enables direct path test before sampling state.")]
        public bool EnableInitialPathTest = true;

        [Tooltip("Enables path smoothing.")]
        public bool EnablePathSmoothing = true;

        [Tooltip("Determines if the motion planning task keeps active after manipulation stop.")]
        public static bool KeepPlanningActive = true;

        [Tooltip("Enable Path Animation")]
        public bool EnablePathAnimation = false;

        public int NumberOfLocalClosestTries = 3;

        bool _interaction_allowed = true;
        public bool InteractionAllowed
        {
            get
            {
                return _interaction_allowed;
            }
            set
            {
                GetComponent<ObjectManipulator>().AllowFarManipulation = value;
                foreach (var item in GetComponents<Collider>())
                {
                    item.enabled = value;
                }
                _interaction_allowed = value;
            }
        }
        bool _planning_enabled = true;
        public bool EnablePlanning
        {
            get
            {
                return _planning_enabled;
            }
            set
            {
                if (!_planned_once)
                    _planning_enabled = value;
                else
                    _planning_enabled = true;
            }
        }

        /////////
        // unity interfacing
        /////////

        public void Start()
        {
            BaseStart();

            // init objects
            _user_history = new ConfigurationHistory();
            _planned_history = new ConfigurationHistory();

            // setup materials
            _original_material = new Material(GetComponent<MeshRenderer>().material);
            _ghost_material = new Material(FindObjectOfType<IMPConfiguration>().GhostMaterial)
            {
                color = new Color(
                _original_material.color.r * .9f,
                _original_material.color.g * .9f,
                _original_material.color.b * .9f,
                0.1f
            )
            };

            // get mesh data
            var mesh_filter = GetComponent<MeshFilter>();
            var mesh = mesh_filter.mesh;

            // as this is a moveable object, attach listeners for interaction
            _object_manipulator = GetComponent<ObjectManipulator>();
            _object_manipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
            _object_manipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
            _object_manipulator.enabled = false;
            _object_manipulator.TwoHandedManipulationType =
                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Move |
                Microsoft.MixedReality.Toolkit.Utilities.TransformFlags.Rotate;

            // create companion object
            _companion_object = new GameObject(gameObject.name + "_companion");
            _companion_object.SetActive(false);
            _companion_object.AddComponent<MeshFilter>();
            _companion_object.GetComponent<MeshFilter>().mesh = mesh;
            _companion_object.AddComponent<MeshRenderer>();
            _companion_object.GetComponent<MeshRenderer>().material = _ghost_material;
            _companion_object.transform.position = gameObject.transform.position;
            _companion_object.transform.rotation = gameObject.transform.rotation;
            _companion_object.transform.localScale = gameObject.transform.localScale;
            Companions.Add(_companion_object);
        }

        public void Update()
        {
            // initialize object
            CreateRemote(true);
            BaseUpdate();

            _update_counter++;

            // enable manipulation as this object is successfully submitted
            if (WebId != -1 && !_object_manipulator.enabled)
                _object_manipulator.enabled = true;

            // manipulation active => track user input
            if (_manipulation)
            {
                // get current configuration
                var configuration = new Configuration()
                {
                    Position = gameObject.transform.position,
                    Rotation = gameObject.transform.rotation
                };

                // if configuration changed enough add to history
                if (MetricSelector.Metric.Distance(_user_history.Last(), configuration) > 0.01)
                    _user_history.Add(gameObject.transform.position, gameObject.transform.rotation);

                // visualize the user path if configured
                RenderUserPath();
            }

            // update position of planned object while planning 
            if (_planning_thread != null && !_planned_history.IsEmpty())
            {
                var config = _planned_history.Last();
                if (_manipulation)
                {
                    _companion_object.transform.position = config.Position;
                    _companion_object.transform.rotation = config.Rotation;

                    if (_planning_state != MotionPlanningState.Rapair)
                    {
                        if (_update_counter % 120 == 0)
                        {
                            var renderer = _companion_object.GetComponent<MeshRenderer>();
                            if (_blink_ghost)
                            {
                                renderer.material = _ghost_material;
                                _blink_ghost = !_blink_ghost;
                            }
                            else
                            {
                                renderer.material = _original_material;
                                _blink_ghost = !_blink_ghost;
                            }
                        }
                    }
                    else
                    {
                        _companion_object.GetComponent<MeshRenderer>().material = _original_material;
                    }
                }
                else
                {
                    gameObject.transform.position = config.Position;
                    gameObject.transform.rotation = config.Rotation;

                    if (_planning_state != MotionPlanningState.Rapair)
                    {
                        if (_update_counter % 120 == 0)
                        {
                            var renderer = gameObject.GetComponent<MeshRenderer>();
                            if (_blink_ghost)
                            {
                                renderer.material = _ghost_material;
                                _blink_ghost = !_blink_ghost;
                            }
                            else
                            {
                                renderer.material = _original_material;
                                _blink_ghost = !_blink_ghost;
                            }
                        }
                    }
                    else
                    {
                        gameObject.GetComponent<MeshRenderer>().material = _original_material;
                    }
                }
            }
            else if (EnablePathAnimation && _planned_history.Size() >= 2)
            {
                ActivateCompanion();

                if (_lerp_value > 1.0f)
                {
                    _animation_index++;
                    _lerp_value = 0.0f;
                    _load_animation_data = true;
                }

                if (_animation_index + 1 >= _planned_history.Size())
                {
                    _animation_index = 0;
                    _lerp_value = 0.0f;
                    _load_animation_data = true;
                }

                if (_load_animation_data)
                {
                    _animation_start = _planned_history.Get(_animation_index);
                    _animation_end = _planned_history.Get(_animation_index + 1);
                    int POSITIONAL_STEPS = (int)((_animation_end.Position - _animation_start.Position).magnitude / 0.01f);
                    int ROTATIONAL_STEPS = (int)(Quaternion.Angle(_animation_end.Rotation, _animation_start.Rotation) / 1.0f);
                    int steps = (Mathf.Max(POSITIONAL_STEPS, ROTATIONAL_STEPS) + 1) * 10;
                    _lerp_step = 1.0f / steps;
                }

                if (_lerp_value > 0.0f)
                {
                    if (_lerp_value < 1.0f)
                    {
                        Vector3 position = Vector3.Lerp(_animation_start.Position, _animation_end.Position, _lerp_value);
                        Quaternion rotation = Quaternion.Lerp(_animation_start.Rotation, _animation_end.Rotation, _lerp_value);

                        _companion_object.transform.position = position;
                        _companion_object.transform.rotation = rotation;
                    }
                    else
                    {
                        _companion_object.transform.position = _animation_end.Position;
                        _companion_object.transform.rotation = _animation_end.Rotation;
                    }
                }
                else
                {
                    _companion_object.transform.position = _animation_start.Position;
                    _companion_object.transform.rotation = _animation_start.Rotation;
                }

                _lerp_value += _lerp_step;
            }
            else
            {
                DeactivateCompanion();
            }

            // updates the path visualization if "dirty"
            if (_dirty_path)
            {
                RenderPlannedPath();
                _dirty_path = false;
            }

            // start path smoothing thread
            if (EnablePathSmoothing && _smoothing_thread == null)
            {
                _smoothing_thread = new Thread(PathSmoothing);
                _smoothing_thread.Start();
            }
            else if (!EnablePathSmoothing && _smoothing_thread != null)
            {
                _smoothing_thread.Abort();
                _smoothing_thread = null;
            }

            // stop motion planning if finished and no manipulation is active
            if (!_manipulation &&
                KeepPlanningActive &&
                _planning_thread != null &&
                !_planning_thread.IsAlive)
            {

                KillPlanningThread(); // stop the motion planning worker thread
                RenderPlannedPath(); // visualize the tracked path if enabled
                DeactivateCompanion(); // hide the companion object
            }
        }

        public void OnApplicationQuit()
        {
            // Stop all running threads on application exit.

            if (_smoothing_thread != null)
                _smoothing_thread.Abort();

            if (_planning_thread != null)
                _planning_thread.Abort();
        }

        public void StartPlan()
        {
            _planned_once = true;

            // manipulation started
            _manipulation = true;

            // stop motion planning if still active and reset user history
            if (_planning_thread != null)
            {
                KillPlanningThread();
                RenderPlannedPath();
            }

            // clear previous user history
            _user_history.Clear();

            // add initial position to planned and user history
            _user_history.Add(transform.position, transform.rotation);
            _planned_history.Add(transform.position, transform.rotation);

            // adjust materials
            // motion planning && manipulation  => this is ghost
            // motion planning && !manipulation => companion is ghost
            // else                             => none is ghost
            Ghost();
            // activate companion object
            ActivateCompanion();

            // start motion planning worker thread
            _planning_thread = new Thread(MotionPlanning);
            _planning_thread.Start();
        }

        public void EndPlan()
        {
            if (!KeepPlanningActive)
            {
                // stop planning and visualize
                KillPlanningThread();
                RenderPlannedPath();

                // set the objects transform to result from motion planning
                transform.position = _companion_object.transform.position;
                transform.rotation = _companion_object.transform.rotation;

                // hide the companion object
                DeactivateCompanion();
            }
            else
            {
                // as we continue planning the actual object becomes the
                // companion (to continue planning interactively without jumps)
                SwapPositions();
            }

            // ensure correct visualization
            CompanionGhost();

            _manipulation = false;
        }

        /////////
        // input handler
        /////////

        public void OnManipulationStarted(ManipulationEventData manipulation)
        {
            if (_planning_enabled)
                StartPlan();
        }

        public void OnManipulationEnded(ManipulationEventData manipulation)
        {
            if (_planning_enabled)
                EndPlan();
        }

        /////////
        // methods
        /////////

        /// <summary>
        /// Path smoothing worker thread.
        /// 
        /// Continously runs the smoothing algorithm and gradually simplifies the path.
        /// </summary>
        void PathSmoothing()
        {
            SmoothState state = SmoothState.Active;

            int work_index = 0;
            bool changed_any = false;
            int known_size = 0;
            while (true)
            {
                if (state == SmoothState.Active)
                {
                    if (_planned_history.Size() > work_index + 2)
                    {
                        var config0 = _planned_history.Get(work_index);
                        var config2 = _planned_history.Get(work_index + 2);

                        if (MetricSelector.Metric.Distance(config0, config2) < 1.0f)
                        {
                            // todo: Store collision results (in some way ?)
                            if (Web.Requests.IsCollisionFreePathRemote(WebId, config0, config2).Result)
                            {
                                _planned_history.Remove(work_index + 1);
                                _dirty_path = true;
                                changed_any = true;
                            }
                        }

                        work_index++;
                    }
                    else if (!_planned_history.IsEmpty())
                    {
                        if (!changed_any)
                        {
                            state = SmoothState.Sleep;
                            known_size = _planned_history.Size();
                        }
                        work_index = 0;
                        changed_any = false;
                    }
                    else
                    {
                        Thread.Sleep(200);
                    }
                }
                else
                {
                    if (known_size != _planned_history.Size())
                    {
                        state = SmoothState.Active;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }

        /// <summary>
        /// Motion Planning Task
        /// 
        /// This thread runs in the background analyzing the user input path and 
        /// matching it collision free.
        /// </summary>
        void MotionPlanning()
        {
            Configuration last = new Configuration(), next = new Configuration();
            int path_to_request_id = -1;
            int tried_count = 0;
            _planning_state = MotionPlanningState.Rapair;
            while (true)
            {
                if (_user_history.Remain() > 5 || (!_manipulation && _user_history.HasNext())) // process user input
                {
                    if (_planning_state == MotionPlanningState.Rapair)
                    {
                        last = _planned_history.Last();
                        next = _user_history.Next();
                        if (EnableInitialPathTest &&
                            Web.Requests.IsCollisionFreePathRemote(WebId, last, next).Result)
                        {

                            Web.Requests.MovedRequest(WebId, _planned_history.Last(), next).Wait();

                            _planned_history.Add(next);
                        }
                        else
                        {
                            var nlc_res = Web.Requests.NewLocalClosestRemote(WebId, last, next).Result;
                            if (nlc_res.First)
                            {
                                Web.Requests.MovedRequest(WebId, _planned_history.Last(), nlc_res.Second).Wait();
                                _planned_history.Add(nlc_res.Second);

                            }
                            else if (tried_count++ > NumberOfLocalClosestTries)
                            {
                                _user_history.Skip(-NumberOfLocalClosestTries + 1);
                                _planning_state = MotionPlanningState.RequestExploration;
                                tried_count = 0;
                            }
                        }
                        _dirty_path = true;
                    }
                    else if (_planning_state == MotionPlanningState.RequestExploration)
                    {
                        var configs = _user_history.NextConfigurations();
                        configs.Insert(0, next);
                        var pathto_res = Web.Requests.PathTo(WebId, last, configs).Result;
                        if (pathto_res.Second)
                        {
                            path_to_request_id = pathto_res.First;
                            _planning_state = MotionPlanningState.Exploration;
                            Debug.Log("Exploration request sent!");
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    else // MotionPlanningState.Exploration
                    {
                        var pts_res = Web.Requests.PathToStatus(path_to_request_id).Result;
                        if (pts_res)
                        {
                            var pt_res = Web.Requests.PathToGet(path_to_request_id, WebId).Result;
                            foreach (var c in pt_res.Second)
                            {
                                _planned_history.Add(c);
                            }
                            if (pt_res.Second.Count > 0)
                            {
                                _user_history.SmartSkip(_planned_history.Last());
                            }
                            _planning_state = MotionPlanningState.Rapair;
                            Debug.Log("Applied explored path of length " + pt_res.Second.Count);
                        }
                    }
                }
                else if (KeepPlanningActive && !_manipulation) // finished path planning
                {
                    break;
                }
                else // wait for user input
                {
                    Thread.Sleep(200);
                }
            }

            _planning_state = MotionPlanningState.Rapair;
        }

        /// <summary>
        /// Activate the companion (ghost) object and set its position to this.
        /// </summary>
        private void ActivateCompanion()
        {
            _companion_object.SetActive(true);
            _companion_object.transform.position = transform.position;
            _companion_object.transform.rotation = transform.rotation;
        }

        /// <summary>
        /// Disables the companion (ghost) object.
        /// </summary>
        private void DeactivateCompanion()
        {
            _companion_object.SetActive(false);
        }

        /// <summary>
        /// Visualizes the companion as ghost object and this as original object.
        /// </summary>
        private void CompanionGhost()
        {
            gameObject.GetComponent<MeshRenderer>().material = _original_material;
            _companion_object.GetComponent<MeshRenderer>().material = _ghost_material;
        }

        /// <summary>
        /// Visualizes this as ghost object and the companion object as original object.
        /// </summary>
        private void Ghost()
        {
            gameObject.GetComponent<MeshRenderer>().material = _ghost_material;
            _companion_object.GetComponent<MeshRenderer>().material = _original_material;
        }

        private void KillPlanningThread()
        {
            if (_planning_thread.IsAlive)
                _planning_thread.Abort();
            _planning_thread = null;
        }

        private void SwapPositions()
        {
            var tmp_pos = gameObject.transform.position;
            var tmp_rot = gameObject.transform.rotation;
            gameObject.transform.position = _companion_object.transform.position;
            gameObject.transform.rotation = _companion_object.transform.rotation;
            _companion_object.transform.position = tmp_pos;
            _companion_object.transform.rotation = tmp_rot;
        }

        /// <summary>
        /// Visualizes the already planned path.
        /// Must run in main thread!
        /// </summary>
        void RenderPlannedPath()
        {
            if (VisualizePlannedPath)
            {
                Vector3[] positions = _planned_history.Positions.ToArray();

                if (_path_object == null)
                {
                    _path_object = new GameObject(gameObject.name + "_path");
                    _path_object.transform.position = Vector3.zero;
                    _path_object.transform.rotation = Quaternion.identity;
                    Companions.Add(_path_object);

                    var renderer = _path_object.AddComponent<LineRenderer>();
                    renderer.positionCount = positions.Length;
                    renderer.SetPositions(positions);
                    renderer.useWorldSpace = true;
                    renderer.startWidth = 0.01f;
                    renderer.endWidth = renderer.startWidth;
                    renderer.material = _original_material;
                }
                else
                {
                    var renderer = _path_object.GetComponent<LineRenderer>();
                    renderer.positionCount = positions.Length;
                    renderer.SetPositions(positions);
                }
            }
        }

        void RenderUserPath()
        {
            if (VisualizeUserPath)
            {
                Vector3[] positions = _user_history.Positions.ToArray();

                if (_user_path_object == null)
                {
                    _user_path_object = new GameObject(gameObject.name + "_user_path");
                    _user_path_object.transform.position = Vector3.zero;
                    _user_path_object.transform.rotation = Quaternion.identity;
                    Companions.Add(_user_path_object);

                    var renderer = _user_path_object.AddComponent<LineRenderer>();
                    renderer.positionCount = positions.Length;
                    renderer.SetPositions(positions);
                    renderer.useWorldSpace = true;
                    renderer.startWidth = 0.01f;
                    renderer.endWidth = renderer.startWidth;
                    renderer.material = _ghost_material;
                }
                else
                {
                    var renderer = _user_path_object.GetComponent<LineRenderer>();
                    renderer.positionCount = positions.Length;
                    renderer.SetPositions(positions);
                }
            }
        }
    }
}