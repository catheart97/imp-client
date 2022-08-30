using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool
{
    class Optional<T>
    {
        /////////
        // data
        /////////
        private T _content;
        private bool _is = false;

        /////////
        // constructors
        /////////
        public Optional()
        {}

        public Optional(T val)
        {
            _is = true;
            _content = val;
        }

        /////////
        // methods
        /////////
        public void Put(T cnt)
        { 
            _content = cnt; 
            _is = true; 
        }

        public bool Has()
        {
            return _is;
        }

        public T Get()
        {
            if (_is)
                return _content;
            else
                throw new System.Exception("Cannot get from optional as it contains nothing");
        }
    }
}
