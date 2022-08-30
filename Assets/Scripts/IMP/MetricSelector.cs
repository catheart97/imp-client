using System.Collections;
using UnityEngine;

namespace IMP
{
    public enum EMetricType : uint
    {
        WeightedMetric = 0
    }

    public class MetricSelector : MonoBehaviour
    {
        /////////
        // data
        /////////
        public static IMetric Metric = null;

        /////////
        // unity properties
        /////////
        [Header("Determines the Configuration Metric")]
        public EMetricType MetricType;

        [Header("Configuration for the Individual Metrics")]
        public float WeightedMetricAlpha = 1.0f;
        public float WeightedMetricBeta = 1.0f;

        /////////
        // data
        /////////
        public void Start()
        {
            Metric = MetricType switch
            {
                _ => new WeightedMetric(WeightedMetricAlpha, WeightedMetricBeta),
            };
        }

        // Update is called once per frame
        public void Update()
        { }
    }
}