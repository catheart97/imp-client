using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace IMP
{
    class ConfigurationHistory
    {
        /////////
        // data
        /////////

        private readonly List<Configuration> _configurations = new List<Configuration>();
        int _pointer = 0;

        /////////
        // properties
        /////////

        public List<Vector3> Positions { get { lock (_configurations) { return _configurations.Select(x => x.Position).ToList(); } } }

        public List<Quaternion> Rotations { get { lock (_configurations) { return _configurations.Select(x => x.Rotation).ToList(); } } }

        public int Pointer { get { return _pointer; } }

        public void Add(Configuration configuration)
        {
            lock (_configurations)
            {
                _configurations.Add(configuration);
            }
        }

        public void Add(Vector3 position, Quaternion rotation)
        {
            Add(new Configuration() { Position = position, Rotation = rotation });
        }

        public void Clear()
        {
            lock (_configurations)
            {
                _pointer = 0;
                _configurations.Clear();
            }
        }

        public Configuration Get(int i)
        {
            lock (_configurations)
            {
                return _configurations[i];
            }
        }

        public Configuration Last()
        {
            lock (_configurations)
            {
                return _configurations[_configurations.Count - 1];
            }
        }

        public bool HasNext()
        {
            lock (_configurations)
            {
                return _pointer < _configurations.Count;
            }
        }

        public bool IsEmpty()
        {
            lock (_configurations)
            {
                return _configurations.Count == 0;
            }
        }

        public void Remove(int i)
        {
            lock (_configurations)
            {
                _configurations.RemoveAt(i);
                if (_pointer >= i)
                    _pointer--;
            }
        }

        public int Size()
        {
            lock (_configurations)
            {
                return _configurations.Count;
            }
        }

        public Configuration Next()
        {
            lock (_configurations)
            {
                var index = _pointer;
                _pointer++;
                return _configurations[index];
            }
        }

        public void Skip(int amount)
        {
            lock (_configurations)
            {
                _pointer += amount;
            }
        }

        public int Remain()
        {
            lock (_configurations)
            {
                if (_configurations.Count - _pointer > 0)
                    return _configurations.Count - _pointer;
                return 0;
            }
        }

        public void SmartSkip(Configuration matchee)
        {
            lock (_configurations)
            {
                int min_index = _pointer;
                float min_distance = float.MaxValue;
                for (int p = _pointer; p < _configurations.Count; ++p)
                {
                    float distance = MetricSelector.Metric.Distance(matchee, _configurations[p]);
                    if (distance < min_distance)
                    {
                        min_index = p;
                        min_distance = distance;
                    }
                }
                _pointer = min_index;
            }
        }

        public List<Configuration> NextConfigurations()
        {
            lock (_configurations)
            {
                if (_pointer < _configurations.Count)
                {
                    return _configurations.GetRange(_pointer, _configurations.Count - _pointer);
                }
                else
                {
                    return new List<Configuration>();
                }
            }
        }
    }
}