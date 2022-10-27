using System.Threading.Tasks;
using Pyro.Nc.Configuration.Managers;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.GCommands
{
    public class G54 : BaseCommand
    {
        public G54(ITool tool, ICommandParameters parameters) : base(tool, parameters)
        {
            Tool.EventSystem.AddAsyncSubscriber("ProgramEnd", this);
        }

        public override async Task Execute(bool draw)
        {
            //??
            Tool.Values.WorkOffsets[0] = new Vector3(Parameters.GetValue("X"), Parameters.GetValue("Y"), Parameters.GetValue("Z"));
            await Tool.EventSystem.FireAsync("WorkOffsetChange_0");
        }
    }
}