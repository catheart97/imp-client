using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tool
{
    public class JoystickCore : MonoBehaviour
    {
        /////////
        // unity properties
        /////////
        public JoystickMenu Menu;
        public float MaxDistance = 0.3f;
        public Follow FollowInstance;

        public uint PowerValue = 4;
        public float PositionalScale = 0.001f;

        public bool Pulsating = false;

        /////////
        // data
        /////////

        Rotator[] _rotators;
        ObjectManipulator _manipulator;
        IMP.IMPMovableObject _object = null;
        bool _manipulation = false;

        float _lerp_value = 1.0f;
        Quaternion _lerp_start_rot;
        Vector3 _lerp_start_pos;

        Quaternion _start_rot;

        /////////
        // unity interfaces
        /////////

        public void Start()
        {
            _rotators = FindObjectsOfType<Rotator>();
            _manipulator = GetComponent<ObjectManipulator>();
            _manipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
            _manipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
        }

        public void Update()
        {
            if (Pulsating)
            {
                float val = Mathf.PingPong(Time.time / 100, 0.02f);
                GetComponent<MeshRenderer>().material.SetFloat("_VertexExtrusionValue", val);
            }

            if (_manipulation)
            {
                float magnitude = transform.localPosition.magnitude;
                Vector3 normalized_pos = transform.localPosition / magnitude;

                if (magnitude > MaxDistance)
                {
                    transform.localPosition *= MaxDistance / magnitude;
                    magnitude = MaxDistance;
                }

                float distance = magnitude / MaxDistance;

                if (_object != null)
                {
                    Quaternion menu_rotation = Menu.gameObject.transform.rotation;
                    Vector3 rotated_pos_direction = menu_rotation * normalized_pos;

                    _object.transform.position += rotated_pos_direction * Math.Power(distance, PowerValue) * PositionalScale;

                    transform.localRotation.ToAngleAxis(out float angle, out Vector3 axis);

                    axis = menu_rotation * axis;
                    _object.transform.rotation = Quaternion.AngleAxis(angle, axis) * _start_rot;
                }
            }
            else if (_lerp_value < 1.0f)
            {
                transform.localPosition = Vector3.Lerp(_lerp_start_pos, Vector3.zero, _lerp_value);
                transform.localRotation = Quaternion.Lerp(_lerp_start_rot, Quaternion.identity, _lerp_value);
                _lerp_value += 0.05f;
            }
            else
            {
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
            }
        }

        /////////
        // methods
        /////////
        ///
        public void OnManipulationStarted(ManipulationEventData manipulation)
        {
            _manipulation = true;
            FollowInstance.enabled = false;

            foreach (var rot in _rotators)
                rot.EnableRotation = true;

            _object = Menu.Selected.Object;

            _start_rot = _object.transform.rotation;

            if (_object.EnablePlanning)
                _object.StartPlan();
        }

        public void OnManipulationEnded(ManipulationEventData manipulation)
        {
            if (_object.EnablePlanning)
                _object.EndPlan();
            _object = null;

            _lerp_value = 0.05f;
            _lerp_start_pos = transform.localPosition;
            _lerp_start_rot = transform.localRotation;

            foreach (var rot in _rotators)
                rot.EnableRotation = false;


            FollowInstance.enabled = true;
            _manipulation = false;
        }
    }
}
