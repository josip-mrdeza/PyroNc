using System.Threading.Tasks;
using Pyro.Nc.Exceptions;
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

        public override Task Execute(bool draw)
        {
            var value = Parameters.GetValue("value");
            var spindleSpeed = Tool.Values.SpindleSpeed;
            if (spindleSpeed.UpperLimit < value)
            {
                throw new SpindleSpeedOverLimitException(this, value, spindleSpeed.UpperLimit);
            }
            spindleSpeed.Set(value);
            var rigidBody = Tool.Self;
            if (rigidBody.angularVelocity.y != 0)
            {
                var current = rigidBody.angularVelocity;
                var v = spindleSpeed.Get() * 0.10472f;//rpm -> radian per sec
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
            Tool.InvokeOnSpindleSpeedChanged(value);
            return Task.CompletedTask;
        }
    }
}