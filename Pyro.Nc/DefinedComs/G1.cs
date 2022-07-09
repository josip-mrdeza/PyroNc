using System.Diagnostics;
using UnityEngine;

namespace Pyro.Nc.DefinedComs
{
    public class G1 : G0
    {
        public G1(CommandArgs args) : base(args, async fargs => {
          await Logger.LogAsync("Preparing command: 'G1'!");
          var ga = fargs.GCommandArgs;
          var vc3 = new Vector3(ga.X, ga.Y, ga.Z);
          await Logger.LogAsync($"Started execution of: 'G1' with arguments: '{args.ToString()}'!");
          Measurement msm = new Measurement();
          msm.Start();
          await Movement.Instance.CutMove(vc3);
          msm.Stop();
          await Logger.LogAsync("Execution for command 'G1' finished, elapsed: " + msm + " ms");
          })
        {
        }
    }
}
