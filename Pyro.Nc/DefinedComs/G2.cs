using System.Diagnostics;
using UnityEngine;

namespace Pyro.Nc.DefinedComs
{
    public class G2 : Command
    {
        public G2(CommandArgs args, bool reverse = false) : base(args, async fargs => {
             await Logger.LogAsync("Preparing command 'G2'!");
             var ga = fargs.GCommandArgs;
             var vc3 = new Vector3(ga.x, ga.y, ga.z);
             var pos = Movement.Instance.Position; //Vector3
             await Logger.LogAsync("Plotting circle for command 'G2'!");
             //set the center of the circle on the position of the destination coordinates.
             var radius = Space3D.Distance(vc3.ToVector3D(), pos.ToVector3D());
             var curve = new Circle3D(radius, CircleSmoothness.Fine); //Plots a circle with 360 points.
             if (reverse) {
               curve.Points.Reverse();
             }
             var index = 0;
             Measurement msm = new Measurement();
             var points = curve.Points.Mutate<Vector3>(p => p.ToVector3());
            //linearly interpolate between each point to the next with a delay to simulate 'gliding';
            msm.Start();
            await Movement.Instance.CutMove(points);
            //stop measurements
            msm.Stop();
            await Logger.LogAsync("Execution for command 'G2' finished, elapsed: " + msm + " ms");
          })
          {}
    }
}
