using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class Trans : BaseCommand
    {
        //TODO fix trans, does not work if the tool is at trans pos, moved tool backward for trans pos, which is very wrong.
        public Trans(ToolBase toolBase, ArbitraryCommandParameters parameters) : base(toolBase, parameters)
        {
            Parameters.AddValue("X", 0);
            Parameters.AddValue("Y", 0);
            Parameters.AddValue("Z", 0);
        }

        public override string Description => Locals.Trans;

        public override Task Execute(bool draw)
        {
            var defaultValues = Globals.ReferencePointParser.ReferencePoint;
            ToolBase.Values.TransPosition =
                new Vector3(ResolveNan(Parameters.GetValue("X"), defaultValues.x),
                            ResolveNan(Parameters.GetValue("Y"), defaultValues.y),
                            ResolveNan(Parameters.GetValue("Z"), defaultValues.z));
            var viewer = Globals.ReferencePointHandler.Trans;
            viewer.Position = ToolBase.Values.TransPosition;
            viewer.IsDirty = true;
            Machine.SetTrans(ToolBase.Values.TransPosition);
            return Task.CompletedTask;
        }
    }
}