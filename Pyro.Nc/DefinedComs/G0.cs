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
            await Logger.LogAsync("Preparing command: 'G0'");
            var ga = fargs.GCommandArgs;
            var vc3 = new Vector3(ga.X, ga.Y, ga.Z);
            await Logger.LogAsync($"Started execution of: 'G0' with arguments: '{args.ToString()}'!");
            Measurement msm = new Measurement();
            msm.Start();
            await Movement.Instance.FastMove(vc3, fargs.TranslationSmoothness);
            msm.Stop();
            await Logger.LogAsync("Execution for command 'G0' finished, elapsed: " + msm + " ms");
        })
        {
        }
    }
}
