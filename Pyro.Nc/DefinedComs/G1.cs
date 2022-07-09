using System.Diagnostics;
using UnityEngine;

namespace Pyro.Nc.DefinedComs
{
    public class G1 : G0
    {
        public G1(CommandArgs args) : base(args)
        {
            base.Function = async fargs =>
            {
                await Logger.LogAsync("Preparing command: 'G01'");
                var ga = fargs.GCommandArgs;
                var vc3 = new Vector3(ga.X, ga.Y, ga.Z);
                await Logger.LogAsync($"Started execution of: 'G01' with arguments: '{args.ToString()}'!");
                Stopwatch metrics = Stopwatch.StartNew();
                await Movement.Instance.CutMove(vc3);
                metrics.Stop();
                await Logger.LogAsync($"Execution finished: {metrics.ElapsedMilliseconds.ToString()}");
            };
        }
    }
}