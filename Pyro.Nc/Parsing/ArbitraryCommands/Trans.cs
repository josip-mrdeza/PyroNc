using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using UnityEngine;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Trans : BaseCommand
    {
        public Trans(ITool tool, ArbitraryCommandParameters parameters) : base(tool, parameters)
        {
            Parameters.AddValue("X", 0);
            Parameters.AddValue("Y", 0);
            Parameters.AddValue("Z", 0);
        }

        public override string Description => Locals.Trans;

        public override Task Execute(bool draw)
        {
            Tool.Values.TransPosition =
                new Vector3(ResolveNan(Parameters.GetValue("X"), 0),
                            ResolveNan(Parameters.GetValue("Y"), 0),
                            ResolveNan(Parameters.GetValue("Z"), 0));
            var viewer = Globals.ReferencePointHandler.Trans;
            viewer.Position = Tool.Values.TransPosition;
            viewer.IsDirty = true;
            return Task.CompletedTask;
        }
    }
}