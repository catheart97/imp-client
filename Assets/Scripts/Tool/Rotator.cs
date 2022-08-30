using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tool
{
    public class Rotator : MonoBehaviour
    {
        public enum Axis
        {
            X, Y, Z
        };

        public bool EnableRotation
        {
            get { return _rotation_enabled; }
            set
            {
                _lerp_value = 0.0f;
                _lerp_start = transform.localRotation;
                _rotation_enabled = value;
            }
        }
        public Axis RotationAxis;
        public float Speed = 1.0f;

        bool _rotation_enabled = true;
        float _angle = 0.0f;
        Vector3 _axis;
        Quaternion _lerp_start;
        float _lerp_value = 0.0f;

        // Start is called before the first frame update
        public void Start()
        {
            switch (RotationAxis)
            {
                case Axis.X: _axis = new Vector3(1.0f, 0.0f, 0.0f); break;
                case Axis.Y: _axis = new Vector3(0.0f, 1.0f, 0.0f); break;
                case Axis.Z: _axis = new Vector3(0.0f, 0.0f, 1.0f); break;
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (EnableRotation)
            {
                transform.localRotation = Quaternion.AngleAxis(_angle, _axis);
                _angle += Speed / (Mathf.PI * 2.0f);
            }
            else
            {
                if (_lerp_value < 1.0f)
                {
                    transform.localRotation = Quaternion.Lerp(_lerp_start, Quaternion.identity, _lerp_value);
                    _lerp_value += Speed / (Mathf.PI * 2.0f);
                }
                else
                {
                    transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
