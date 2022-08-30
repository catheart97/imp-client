using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Tool
{
    public class HandMenu : MonoBehaviour
    {
        /////////
        // unity properties
        /////////

        public Vector3 PinnedPosition = Vector3.zero;
        public GameObject ChildTree;
        public Interactable JoyStickInteractable = null;
        public Interactable EnvironmentVisibleInteractable = null;
        public Interactable SubmittedEnvironmentVisibleInteractable = null;
        public Interactable OcclusionInteractable = null;
        public Material EnvironmentWireframe = null;
        public Material EnvironmentOcclusion = null;

        public bool InitialJoystickToggle = false;

        /////////
        // data
        /////////

        // environment mesh information
        bool _env_submitted = false;
        Task<int> _env_submission_task = null;
        readonly Queue<MeshBundle> _env_submission_queue = new Queue<MeshBundle>();
        List<int> _env_remote_ids = new List<int>();
        List<GameObject> _env_objects = new List<GameObject>();
        bool _env_finished_submission = false;
        Task _trash_task = null;

        // joystick bound and release 
        bool _bound_to_joystick = false;
        JoystickMenu _joystick_menu;
        GameObject _joystick;

        // occlusion
        bool _occlusion = false;

        // sub env
        bool _env_submission_visible = false;

        /////////
        // nested
        /////////
        private struct MeshBundle
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public Vector3 LocalScale;
            public Vector3 Position;
            public Quaternion Rotation;
        }

        /////////
        // unity interfacing
        /////////

        public void Reset()
        {
            _env_submitted = false;
            _env_submission_task = null;
            _env_submission_queue.Clear();
            _env_remote_ids.Clear();
            foreach(var o in _env_objects) Destroy(o);
            _env_objects.Clear();
            _env_finished_submission = false;
        }

        public void Start()
        {
            _joystick_menu = FindObjectOfType<JoystickMenu>();
            _joystick = _joystick_menu.gameObject;
            _joystick.SetActive(false);

            if (InitialJoystickToggle)
                ToggleJoystick();
        }

        public void Update()
        {
            SubmissionUpdate();
        }

        /////////
        // worker methods
        /////////

        private void SubmissionUpdate()
        {
            if (_env_submitted && !_env_finished_submission)
            {
                if (_trash_task != null)
                {
                    if (_trash_task.IsCompleted)
                        _trash_task = null;
                }
                else
                {
                    if (_env_submission_task == null || (_env_submission_task != null && _env_submission_task.IsCompleted))
                    {
                        if (_env_submission_task != null)
                            _env_remote_ids.Add(_env_submission_task.Result);

                        if (_env_submission_queue.Count > 0)
                        {
                            var next = _env_submission_queue.Dequeue();

                            _env_submission_task = Web.Requests.CreateWebObject(
                                false,
                                next.Vertices,
                                next.Triangles,
                                next.LocalScale,
                                new IMP.Configuration()
                                {
                                    Position = next.Position,
                                    Rotation = next.Rotation
                                },
                                null);

                            var obj = new GameObject();
                            obj.transform.position = next.Position;
                            obj.transform.rotation = next.Rotation;
                            obj.transform.localScale = next.LocalScale;
                            var mesh = new Mesh
                            {
                                vertices = next.Vertices,
                                triangles = next.Triangles
                            };
                            var mesh_filter = obj.AddComponent<MeshFilter>();
                            mesh_filter.mesh = mesh;
                            var renderer = obj.AddComponent<MeshRenderer>();
                            renderer.material = _occlusion ? EnvironmentOcclusion : EnvironmentWireframe;
                            obj.SetActive(_env_submission_visible);
                            _env_objects.Add(obj);
                        }
                        else
                        {
                            _env_finished_submission = true;
                            Tool.Console.Log("Finished environment mesh submission!");
                        }
                    }
                }

            }
        }

        /////////
        // helper methods
        /////////

        private bool IsEnvironmentVisible()
        {
            var visible = false;
            if (CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
            {
                foreach (var observer in provider.GetDataProviders())
                {
                    if (observer is IMixedRealitySpatialAwarenessMeshObserver meshObserver &&
                        meshObserver.DisplayOption == SpatialAwarenessMeshDisplayOptions.Visible)
                    {
                        visible = true;
                    }
                }
            }
            return visible;
        }
        public void BindToHand()
        {
            ChildTree.SetActive(false);
            transform.parent = null;
            transform.localPosition = Vector3.zero;
            GetComponent<HandConstraintPalmUp>().enabled = true;
            _bound_to_joystick = false;
        }

        public void BindToJoystickMenu()
        {
            GetComponent<HandConstraintPalmUp>().enabled = false;
            transform.parent = FindObjectOfType<JoystickMenu>().transform;
            transform.localPosition = PinnedPosition;
            transform.localRotation = Quaternion.identity;
            ChildTree.SetActive(true);
            _bound_to_joystick = true;
        }

        private void SetEnvironmentVisible(SpatialAwarenessMeshDisplayOptions option)
        {
            if (CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
            {
                foreach (var observer in provider.GetDataProviders())
                {
                    if (observer is IMixedRealitySpatialAwarenessMeshObserver meshObserver)
                    {
                        meshObserver.DisplayOption = option;
                    }
                }
            }
        }

        private bool IsCoarseEnvironmentLOD()
        {
            var coarse = false;
            if (CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
            {
                foreach (var observer in provider.GetDataProviders())
                {
                    if (observer is IMixedRealitySpatialAwarenessMeshObserver meshObserver &&
                        meshObserver.LevelOfDetail == SpatialAwarenessMeshLevelOfDetail.Coarse)
                    {
                        coarse = true;
                    }
                }
            }
            return coarse;
        }

        private void ApplyEnvironmentLOD(SpatialAwarenessMeshLevelOfDetail option)
        {
            if (CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
            {
                foreach (var observer in provider.GetDataProviders())
                {
                    if (observer is IMixedRealitySpatialAwarenessMeshObserver meshObserver)
                    {
                        meshObserver.LevelOfDetail = option;
                    }
                }
            }
        }

        /////////
        // methods
        /////////

        public void ToggleProfiler()
        {
            CoreServices.DiagnosticsSystem.ShowProfiler = !CoreServices.DiagnosticsSystem.ShowProfiler;
            if (CoreServices.DiagnosticsSystem.ShowProfiler)
            {
                Console.Log("GPU-Profiler enabled");
            }
            else
            {
                Console.Log("GPU-Profiler disabled");
            }
        }

        public void ToggleEnvironmentVisibility()
        {
            if (IsEnvironmentVisible())
            {
                SetEnvironmentVisible(SpatialAwarenessMeshDisplayOptions.None);
                Console.Log("Tracked-Mesh visibility set to None");
                if (EnvironmentVisibleInteractable != null)
                    EnvironmentVisibleInteractable.IsToggled = false;
            }
            else
            {
                SetEnvironmentVisible(SpatialAwarenessMeshDisplayOptions.Visible);
                Console.Log("Tracked-Mesh visibility set to Visible");
                if (EnvironmentVisibleInteractable != null)
                    EnvironmentVisibleInteractable.IsToggled = true;
            }
        }

        public void ToggleSubmitEnvironmentVisibility()
        {
            _env_submission_visible = !_env_submission_visible;
            foreach (var obj in _env_objects)
            {
                obj.SetActive(_env_submission_visible);
            }
        }

        public void ToggleOcclusion()
        {
            _occlusion = !_occlusion;
            foreach (var obj in _env_objects)
            {
                obj.GetComponent<MeshRenderer>().material = _occlusion ? EnvironmentOcclusion : EnvironmentWireframe;
            }
        }

        public void ToggleEnvironmentLOD()
        {
            if (IsCoarseEnvironmentLOD())
            {
                ApplyEnvironmentLOD(SpatialAwarenessMeshLevelOfDetail.Unlimited);
                Console.Log("Tracked-Mesh LOD is set to unlimited");
            }
            else
            {
                ApplyEnvironmentLOD(SpatialAwarenessMeshLevelOfDetail.Coarse);
                Console.Log("Tracked-Mesh LOD is set to coarse");
            }
        }

        public void SubmitEnvironmentMesh()
        {
            if (!_env_submitted)
            {
                if (CoreServices.SpatialAwarenessSystem is IMixedRealityDataProviderAccess provider)
                {
                    var observer = provider.GetDataProviders()[0];

                    var meshObserver = observer as IMixedRealitySpatialAwarenessMeshObserver;
                    var meshes = meshObserver.Meshes;

                    foreach (var mesh in meshes)
                    {
                        var mesh_value = mesh.Value;
                        var vertices = mesh_value.Filter.mesh.vertices;
                        var triangles = GenericUtility<int>.Copy(mesh_value.Filter.mesh.triangles);

                        var position = mesh_value.GameObject.transform.position;
                        var rotation = mesh_value.GameObject.transform.rotation;
                        var localScale = mesh_value.GameObject.transform.localScale;

                        _env_submission_queue.Enqueue(
                            new MeshBundle
                            {
                                Triangles = triangles,
                                Vertices = vertices,
                                Position = position,
                                Rotation = rotation,
                                LocalScale = localScale,
                            });
                    }

                    Console.Log("Tracked-Mesh uploads enqueued! Total: " + _env_submission_queue.Count);
                    _env_submitted = true;
                }
                else
                {
                    Console.Log("ERROR: The provider is null.");
                }
            }
            else
            {
                if (_env_submission_queue.Count > 0)
                {
                    Console.Log("Previous mesh-submission still in progress.");
                }
                else
                {
                    Pair<int, bool>[] to_delete = new Pair<int, bool>[_env_remote_ids.Count];
                    for (int i = 0; i < to_delete.Length; i++)
                    {
                        to_delete[i] = new Pair<int, bool>()
                        {
                            First = _env_remote_ids[i],
                            Second = false
                        };
                    }
                    _trash_task = Web.Requests.Trashes(to_delete);
                    _env_remote_ids = new List<int>();
                    _env_objects.ForEach(x => x.SetActive(false));
                    _env_objects = new List<GameObject>();
                    _env_submitted = false;
                    _env_submission_task = null;
                    SubmitEnvironmentMesh();
                    _env_finished_submission = false;
                }
            }
        }

        public void ToggleJoystick()
        {
            if (_bound_to_joystick)
            {
                _bound_to_joystick = false;
                BindToHand();
                _joystick.SetActive(false);
                if (JoyStickInteractable != null)
                    JoyStickInteractable.IsToggled = false;
            }
            else
            {
                _bound_to_joystick = true;
                _joystick.SetActive(true);
                BindToJoystickMenu();
                if (JoyStickInteractable != null)
                    JoyStickInteractable.IsToggled = true;
            }
        }

    }
}
