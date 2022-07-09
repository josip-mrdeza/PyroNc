using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.Math.Geometry;
using UnityEngine;

namespace Pyro.Nc
{
    public class Movement : MonoBehaviour
    {
        public static Movement Instance
        {
            get
            {
                if (Instance__ is null)
                {
                    var frame = new StackFrame(1);
                    var ftn = frame.GetMethod().DeclaringType.FullName;
                    if (ftn.Contains("DisplayClass"))
                    {

                        ftn = ftn.Replace("+", "_").Replace("<", "_")
                                 .Replace(">", "_").Replace("-", "_").Replace("DisplayClass", "LambdaFunction");
                    }
                    throw new ToolMovementControllerException($"[Pyro.Nc.Movement] Static field 'Instance__'" +
                                                              $" has not been initialized but was tried to be accessed by '{frame.GetMethod().Name}' in type " +
                                                              $"'{ftn}'.");
                }

                return Instance__;
            }
            set => Instance__ = value;
        }

        private static Movement Instance__;
        public TimeSpan FastMoveTick = TimeSpan.FromMilliseconds(0.1d);
        public TargetDestination Destination;
        public Vector3 CurrentLocation;
        private Vector3 CurrentLocation__
        {
            get
            {
                var pos = transform.position;
                CurrentLocation = pos;

                return pos;
            }
        }

        public virtual async Task FastMove(Vector3 location, LineTranslationSmoothness translationSmoothness = LineTranslationSmoothness.Standard)
        {
            if (Destination.IsValid)
            {
                while (Destination.IsValid)
                {
                    await Task.Delay(FastMoveTick);
                }
            }
            
            Destination = new TargetDestination(location);

            Line3D line3D = new Line3D(transform.position.ToVector3D(), location.ToVector3D(), (int) translationSmoothness);

            for (int i = 0; i < line3D.NumberOfPoints; i++)
            {
                await Task.Yield();
                var p = line3D.Points[i].Position;
                transform.position = p.ToVector3();
                await Task.Delay(FastMoveTick);
            }
        }

        public virtual async Task CutMove(Vector3 location, LineTranslationSmoothness translationSmoothness = LineTranslationSmoothness.Standard, Curvature curvature = Curvature.Line)
        {
            
        }
        private void Awake()
        {
            Instance__ ??= this;
        }
    }
}