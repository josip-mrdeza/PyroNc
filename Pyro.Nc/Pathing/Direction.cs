using System;
using System.IO;
using UnityEngine;

namespace Pyro.Nc.Pathing
{
    public struct Direction
    {
        public float X;
        public float Y;
        public float Z;
        public Direction(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Direction FromVectors(Vector3 toolPosition, Vector3 newPosition)
        {
            var resultingVector = toolPosition - newPosition;    
            // return new Direction(Determine(resultingVector.x), 
            //                      Determine(resultingVector.y), 
            //                      Determine(resultingVector.z));
            return new Direction(resultingVector.x, resultingVector.y, resultingVector.z);
        }

        private static float Determine(float f)
        {
            if (f < 0)
            {
                return -1;
            }

            if (f > 0)
            {
                return 1;
            }

            if (f == 0)
            {
                return 0;
            }

            throw new ArithmeticException("Impossible case.");
        }

        public static explicit operator Vector3(Direction direction)
        {
            return new Vector3(direction.X, direction.Y, direction.Z);
        }
    }
}