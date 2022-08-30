using System.Linq;
using System.Collections.Generic;

namespace Tool
{
    public static class Math
    {

        /// <summary>
        ///     Transforms a vector into global space.
        /// </summary>
        /// <param name="transform">The transform world-to-local.</param>
        /// <param name="v">The vector.</param>
        /// <returns>The transformed vector.</returns>
        public static UnityEngine.Vector3 TransformVertex(UnityEngine.Transform transform, UnityEngine.Vector3 v)
        {
            return TransformVertex(transform.position, transform.localScale, transform.rotation, v);
        }

        public static UnityEngine.Vector3 TransformVertex(UnityEngine.Vector3 position, UnityEngine.Vector3 scale, UnityEngine.Quaternion rotation, UnityEngine.Vector3 v)
        {
            return rotation * v * scale.x + position;
        }


        /// <summary>
        ///     Checks if all members of the vector are equal.
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>If all equal true else false.</returns>
        public static bool AllEqual(UnityEngine.Vector3 v)
        {
            return v.x == v.y && v.y == v.z;
        }

        public static float RandomFloat(System.Random r_gen)
        {
            //var sign = r_gen.Next(2);
            //var exponent = r_gen.Next((1 << 8) - 1); // do not generate 0xFF (infinities and NaN)
            //var mantissa = r_gen.Next(1 << 23);
            //var bits = (sign << 31) + (exponent << 23) + mantissa;
            //return IntBitsToFloat(bits);

            return (float) r_gen.NextDouble();
        }

        public static UnityEngine.Vector3 RandomOnUnitSphere(System.Random r_gen)
        {
            float theta = 2.0f * UnityEngine.Mathf.PI * RandomFloat(r_gen);
            float phi = UnityEngine.Mathf.Acos(1.0f - 2.0f * RandomFloat(r_gen));

            float x = UnityEngine.Mathf.Sin(phi) * UnityEngine.Mathf.Cos(theta);
            float y = UnityEngine.Mathf.Sin(phi) * UnityEngine.Mathf.Sin(theta);
            float z = UnityEngine.Mathf.Cos(phi);

            return new UnityEngine.Vector3(x, y, z);
        }

        public static uint SDIV(uint x, uint y)
        {
            return (x + y - 1) / y;
        }
        public static UnityEngine.Quaternion RandomBiasedUnitRotation(System.Random r_gen, float scale = 1.0f)
        {
            var axis = RandomOnUnitSphere(r_gen);
            float eta = UnityEngine.Mathf.PI * UnityEngine.Mathf.Sqrt(RandomFloat(r_gen)) * scale;
            float sineta = UnityEngine.Mathf.Sin(eta / 2);
            float coseta = UnityEngine.Mathf.Cos(eta / 2);

            return new UnityEngine.Quaternion(axis.x * sineta, axis.y * sineta, axis.z * sineta, coseta);
        }

        public static UnityEngine.Vector3 RandomInsideUnitSphere(System.Random r_gen)
        {
            var on_sphere = RandomOnUnitSphere(r_gen);
            var radius = RandomFloat(r_gen);
            return radius * on_sphere;
        }

        public static UnityEngine.Quaternion RandomRotation(System.Random r_gen)
        {
            // James J. Kuffner 2004 
            var s0 = RandomFloat(r_gen);
            var s1 = RandomFloat(r_gen);
            var s2 = RandomFloat(r_gen);
            var sigma1 = UnityEngine.Mathf.Sqrt(1.0f - s0);
            var sigma2 = UnityEngine.Mathf.Sqrt(s0);
            var theta1 = 2.0f * UnityEngine.Mathf.PI * s1;
            var theta2 = 2.0f * UnityEngine.Mathf.PI * s2;
            var w = UnityEngine.Mathf.Cos(theta2) * sigma2;
            var x = UnityEngine.Mathf.Sin(theta1) * sigma1;
            var y = UnityEngine.Mathf.Cos(theta1) * sigma1;
            var z = UnityEngine.Mathf.Sin(theta2) * sigma2;
            return new UnityEngine.Quaternion(x, y, z, w);
        }

        private static float IntBitsToFloat(int bits)
        {
            unsafe
            {
                return *(float*)&bits;
            }
        }

        public static float Power(float x, uint exp)
        {
            float res = 0.0f;

            string binary = System.Convert.ToString(exp, 2);
            var list = binary.ToList().Select(x => x == '1' ? 1 : 0).Reverse().ToArray();

            float buffer = x;
            for (int i = 0; i < list.Length; i++)
            {
                res += buffer * list[i];
                buffer *= buffer;
            }

            return res;
        }
    }

}