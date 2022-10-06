using System.Threading.Tasks;
using Pyro.Nc.Parsing.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using UnityEngine;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class SpindleSpeedSetter : BaseCommand
    {
        public SpindleSpeedSetter(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
        }

        public override string Description => Locals.SpindleSpeedSetter;

        public override async Task Execute(bool draw)
        {
            var value = Parameters.GetValue("value");
            var spindleSpeed = Tool.Values.SpindleSpeed;
            if (spindleSpeed.UpperLimit < value)
            {
                throw new SpindleSpeedOverLimitException(this, value, spindleSpeed.UpperLimit);
            }
            spindleSpeed.Set(Parameters.GetValue("value"));
            var rigidBody = Tool.Self;
            if (rigidBody.angularVelocity.y != 0)
            {
                var current = rigidBody.angularVelocity;
                rigidBody.angularVelocity = new Vector3(0, 1, 0) * ((current.y < 0 ? -1 : 1) * spindleSpeed);
            }
        }
    }
}