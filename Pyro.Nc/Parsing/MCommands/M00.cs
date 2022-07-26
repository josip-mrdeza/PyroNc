using System.Threading.Tasks;
using Pyro.Nc.Parsing.GCommands;
using Pyro.Nc.Pathing;

namespace Pyro.Nc.Parsing.MCommands
{
    public class M00 : ICommand
    {
        public M00(ITool tool, ICommandParameters parameters)
        {
            
        }
        public ITool Tool { get; }
        public virtual bool IsModal => false;
        public virtual bool IsArc => false;
        public string Description { get; }
        public ICommandParameters Parameters { get; set; }

        public async Task Execute()
        {
            
        }
        public async Task Execute(bool draw) => throw new System.NotImplementedException();

        public void Expire() => throw new System.NotImplementedException();

        public void Plan() => throw new System.NotSupportedException();

        public ICommand Copy()
        {
            return this.MemberwiseClone() as M00;
        }
    }
}