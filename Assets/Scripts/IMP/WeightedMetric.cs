using UnityEngine;

namespace IMP
{
    public class WeightedMetric : IMetric
    {
        /////////
        // data
        /////////
        readonly float _alpha = 1.0f;
        readonly float _beta = 1.0f;

        /////////
        // construction
        /////////
        public WeightedMetric(float alpha, float beta)
        {
            _alpha = alpha;
            _beta = beta;
        }

        /////////
        // interface implementations
        /////////
        public float Distance(Configuration a, Configuration b)
        {
            // compute the eucledean distance between points
            var delta_pos = a.Position - b.Position;
            var pos_distance = Mathf.Sqrt(Vector3.Dot(delta_pos, delta_pos));
            float rot_distance = Quaternion.Angle(a.Rotation, b.Rotation) / 360.0f;

            return Mathf.Sqrt(_alpha * pos_distance * pos_distance + _beta * rot_distance * rot_distance);
        }
    }
}