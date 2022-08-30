using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Tool
{
    class Stopwatch
    {
        private TimeSpan _delta = new TimeSpan();
        private DateTime _start_time;
        readonly private string _name;
        private bool _running = false;

        public Stopwatch(string name, bool start = true)
        {
            _name = name;
            if (start) Start();
        }

        public void Start()
        {
            Reset();
            _start_time = DateTime.Now;
            _running = true;
        }

        public void Resume()
        {
            _start_time = DateTime.Now;
            _running = true;
        }

        public string Leap(string additional_text = "")
        {
            if (_running)
            {
                var delta = DateTime.Now - _start_time + _delta;
                var val = "Stopwatch [" + _name + "] [" + additional_text + "] LEAP " + delta.ToString();
                Console.Log(val);
                return val;
            }
            else
            {
                var val = "Stopwatch [" + _name + "] [" + additional_text + "] LEAP " + _delta.ToString();
                Console.Log(val);
                return val;
            }
        }

        public void Pause()
        {
            _delta = DateTime.Now - _start_time + _delta;
            _running = false;
        }

        public void Reset()
        {
            _delta = new TimeSpan();
            _running = false;
        }
    }
}
