using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.UI;
using Pyro.Math;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;
using Physics = Pyro.Math.Physics;

namespace Pyro.Nc.Configuration.Sim3D_Legacy
{
    public static class Sim3DLegacyMethods
    {
        public static bool IsInPosition { get; set; }
        public static async Task TraverseForceBased(this ToolBase toolBase, Vector3[] points, bool draw, bool logStats)
        {
            /*var toolValues = toolBase.Values;
            double averageTimeForCut = 0;
            long totalCut = 0;
            toolBase.SetupTranslation(points);
            var currentPosition = toolBase.Position;
            var rigidBody = (toolBase as MonoBehaviour).GetComponent<Rigidbody>();
            var last = points.Last();
            var force = Physics.ForceRequiredToPush(currentPosition.ToVector3D(), last.ToVector3D(), rigidBody.mass,
                                                    toolBase.Values.FeedRate);
            var magn = (currentPosition - last).RemoveNan().magnitude;
            var totalForce = (last - currentPosition).normalized.RemoveNan() * force;
            rigidBody.AddForce(totalForce, ForceMode.Force);
            //Debug.Log(totalForce);
            
            for (;magn > 0.7f || !IsInPosition;) //empirical value
            {
                if (totalForce == Vector3.zero)
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(Time.fixedDeltaTime));
                var altPos = toolBase.Position;
                //Debug.DrawLine(currentPosition, altPos, Color.red, 4);
                currentPosition = altPos;
                magn = (currentPosition - last).RemoveNan().magnitude;
                //Debug.Log(magn);
                await Task.Yield();
            }
            if (toolValues.ExactStopCheck)
            {
                //await toolBase.InvokeOnConsumeStopCheck();
            }
            if (logStats)
            {
                PyroConsoleView.PushTextStatic("Traverse finished!", $"Total vertices cut: {totalCut} ({((double) totalCut/toolBase.Vertices.Count).Round().ToString(CultureInfo.InvariantCulture)}%)",
                                               $"Average time spent cutting: {averageTimeForCut.Round().ToString(CultureInfo.InvariantCulture)}ms");
                  
            }
            */
        }
    
    }
}