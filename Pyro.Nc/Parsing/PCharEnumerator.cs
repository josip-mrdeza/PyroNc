using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Pyro.Nc.Parsing
{
    public class PCharEnumerator : IEnumerator<char>
    {
        private string str;
        private int index;
        private char _currentElement;

        public PCharEnumerator(string s)
        {
            str = s;
            index = -1;
        }
        
        public void Dispose()
        {
            if (str != null)
            {
                index = str.Length;
            }

            str = null;
        }

        public bool MoveNext()
        {
            if (index < str!.Length - 1)
            {
                index++;
                _currentElement = str[index];

                return true;
            }

            index = str.Length;
            return false;
        }

        public void Reset()
        {
            _currentElement = (char) 0;
            index = -1;
        }

        public char Current { get => _currentElement; }

        object IEnumerator.Current => Current;
    }
}