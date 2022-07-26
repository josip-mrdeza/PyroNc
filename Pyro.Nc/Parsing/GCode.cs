using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrCore;

namespace Pyro.Nc.Parsing
{
    public class GCode : IEnumerable<GCode.Line>
    {
        public int Length { get => Lines.Count; }
        public List<Line> Lines { get; }

        public Line this[int index]
        {
            get => Lines[index];
        }
        public class Line : IEnumerable<char>, IEquatable<Line>
        {
            public string Contents;

            public Line(string contents)
            {
                Contents = contents;
            }
            
            public IEnumerator<char> GetEnumerator() => new PCharEnumerator(Contents.GuardNullVariable("_line"));

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Equals(Line other)
            {
                return Contents == other.GuardNull().Contents;
            }
        }

        public IEnumerator<Line> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}