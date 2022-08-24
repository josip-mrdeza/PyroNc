using System;
using System.Collections.Generic;

namespace Pyro.IO.PyroScript
{
    public class LinkedPiece
    {
        public LinkedPiece(bool isParameter, string value, LinkedPiece previous, LinkedPiece next)
        {
            IsParameter = isParameter;
            Value = value;
            Previous = previous;
            Next = next;
        }
        public bool IsParameter { get; set; }
        public bool IsLambda { get; set; }
        public string Value { get; set; }
        public LinkedPiece Previous { get; set; }
        public LinkedPiece Next { get; set; }
        public virtual void Init()
        {
        }

        public virtual object Run()
        {
            return null;
        }
    }
}