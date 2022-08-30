using System.Collections;
using UnityEngine;

namespace IMP
{
    public interface IMetric
    {
        public float Distance(Configuration a, Configuration b);
    }
}