using UnityEngine;

namespace IMP
{
    public struct Configuration
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator==(Configuration t, Configuration o)
        {
            return t.Position == o.Position && t.Rotation == o.Rotation;
        }

        public static bool operator!=(Configuration t, Configuration o)
        {
            return !(t == o);
        }
    }
}