using System;
using System.Linq;
using System.Threading.Tasks;
using Pyro.IO;
using Pyro.Math;
using Pyro.Math.Geometry;
using Pyro.Nc.Pathing;
using TrCore;
using TrCore.Logging;
using UnityEngine;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G00 : ICommand
    {

        public G00(ITool tool, GCommandParameters parameters)
        {
            Tool = tool.GuardNullVariable("G00<ctor>.Tool");
            Parameters = parameters.GuardNullVariable("G00<ctor>.Parameters");
        }
        public ITool Tool { get; set; }
        public virtual bool IsModal => true;
        public virtual bool IsArc => false;
        public ICommandParameters Parameters { get; set; }
        public virtual string Description
        {
            get => Locals.G00;
        }

        public async Task Execute()
        {
            await Execute(false);
        }

        public virtual async Task Execute(bool draw)
        {
            await Tool.Traverse(new Vector3D(Parameters.GetValue("X"), Parameters.GetValue("Y"), Parameters.GetValue("Z")).ToVector3(),
                                LineTranslationSmoothness.Rough, draw);
            Expire();
        }

        public void Expire()
        {
            Tool.CurrentPath.Expired = true;
        }

        public virtual void Plan() => throw new NotImplementedException();

        public virtual ICommand Copy()
        {
            return MemberwiseClone() as ICommand;
        }
    }
}