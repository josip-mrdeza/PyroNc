using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pyro.IO;
using UnityEngine;

namespace Pyro.Nc.DefinedComs
{
    public class G0 : Command
    {
        public G0(CommandArgs args) : base(args, async fargs =>
        {
            await Logger.LogAsync("Preparing command: 'G00'");
            var ga = fargs.GCommandArgs;
            var vc3 = new Vector3(ga.X, ga.Y, ga.Z);
            await Logger.LogAsync($"Started execution of: 'G00' with arguments: '{args.ToString()}'!");
            Stopwatch metrics = Stopwatch.StartNew();
            await Movement.Instance.FastMove(vc3, fargs.TranslationSmoothness);
            metrics.Stop();
            await Logger.LogAsync($"Execution finished: {metrics.ElapsedMilliseconds.ToString()} ms");
        })
        {
        }
    }
}