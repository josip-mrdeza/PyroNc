using System;

namespace Pyro.Math.Geometry
{
    public class GeometricalException : NotSupportedException
    {
        public GeometricalException(){}
        public GeometricalException(string message) : base(message) {}
    }
}