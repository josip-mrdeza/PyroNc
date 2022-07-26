using Pyro.Math;
using Pyro.Math.Geometry;

namespace Pyro.Nc
{
    public class PObject : ComplexShape
    {
        public PObject() : base(new []
        {
            new Vector3D(0, 0, 0),
            new Vector3D(10, 0, 0),
            new Vector3D(10, 20, 0),
            new Vector3D(0, 20, 0),
            
            new Vector3D(0, 0, 10),
            new Vector3D(10, 0, 10),
            new Vector3D(10, 20, 10),
            new Vector3D(0, 20, 10)
        }, new [] { new Limit(Axis3D.X, 0, 10)}, 
            new []{ new Limit(Axis3D.Y, 0, 20)}, 
            new []{ new Limit(Axis3D.Z, 0, 10)})
        {
            
        }
    }
}