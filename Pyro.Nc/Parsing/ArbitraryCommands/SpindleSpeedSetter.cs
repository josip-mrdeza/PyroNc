using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Parsing.MCommands;
using Pyro.Nc.Pathing;
using Pyro.Nc.Simulation.Tools;
using UnityEngine;

namespace Pyro.Nc.Parsing.ArbitraryCommands
{
    public class SpindleSpeedSetter : BaseCommand
    {
        public SpindleSpeedSetter(ToolBase toolBase, ICommandParameters parameters) : base(toolBase, parameters)
        {
        }

        public override string Description => Locals.SpindleSpeedSetter;

        public override Task Execute(bool draw)
        {
            var value = Parameters.GetValue("value");
            var spindle = Machine.SpindleControl;
            if (spindle.SpindleSpeed.UpperLimit < value)
            {
                throw new SpindleSpeedOverLimitException(this, value, spindle.SpindleSpeed.UpperLimit);
            }
            Machine.SetSpindleSpeed(value);
            var rigidBody = ToolBase.Body;
            if (rigidBody.angularVelocity.y != 0)
            {
                var current = rigidBody.angularVelocity;
                var v = spindle.SpindleSpeed.Get() * 0.10472f;//rpm -> radian per sec
                if (v > 360f)
                {
                    v = 360f;
                }
                rigidBody.angularVelocity = new Vector3(0, 1, 0) * ((current.y < 0 ? -1 : 1) * v);
            }
            else
            {
                rigidBody.angularVelocity = new Vector3(0, 1, 0) * (value * 0.10472f);
            }
            return Task.CompletedTask;
        }
    }
}