using System;
using System.Collections.Generic;
using System.Linq;

namespace Pyro.IO.PyroSc
{
    public class Keyword
    {
        public string Name { get; set; }
        public string Contents { get; private set; }
        public bool IsRunnable { get; private protected set;}
        protected PyroScSourceParser Source { get; }
        public IEnumerable<Keyword> Elements => Source.Words[LineIndex];
        public readonly int ElementIndex;
        public readonly int LineIndex;
        public Keyword CachedNext;
        public Keyword(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source)
        {
            Name = name;
            Contents = contents;
            Source = source;
            LineIndex = lineIndex;
            ElementIndex = elementIndex;
        }

        public Keyword Skip()
        {
            if (CachedNext is null)
            {
                CachedNext = Elements.ElementAt(ElementIndex + 1);
            }
            return CachedNext.Next();
        }
        
        public Keyword Next()
        {
            if (CachedNext is null)
            {
                CachedNext = Elements.ElementAt(ElementIndex + 1);
            }
            return CachedNext;
        }
        
        public string NextAsString()
        {
            if (CachedNext is null)
            {
                CachedNext = Elements.ElementAt(ElementIndex + 1);
            }
            return CachedNext.Contents;
        }
        
        public virtual object Run()
        {
            if (Source.Scope.Exists(Contents))
            {
                return Source.Scope.Get(Contents);
            }
            return Contents;
        }

        public void EnableExecution()
        {
            IsRunnable = true;
        }
    }
}