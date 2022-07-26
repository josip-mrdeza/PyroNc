using System.Collections;
using System.Collections.Generic;

namespace Pyro.Nc.Parsing
{
    public class PLineEnumerator : IEnumerator<GCode.Line>
    {
        private GCode _code;
        private int index;
        private GCode.Line _currentElement;
        
        public void Dispose()
        {
            if (_code != null)
            {
                index = _code.Length;
            }

            _code = null;
        }

        public bool MoveNext()
        {
            if (index < _code!.Length - 1)
            {
                index++;
                _currentElement = _code![index];

                return true;
            }

            index = _code.Length;
            return false;
        }

        public void Reset()
        {
            _currentElement = null;
            index = -1;
        }

        public GCode.Line Current { get => _currentElement; }

        object IEnumerator.Current => Current;
    }
}