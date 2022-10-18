using System.Diagnostics;

namespace Pyro.Nc.Parser
{
    public class Block
    {
        public Block(string text, bool isCommand)
        {
            Text = text;
            IsCommand = isCommand;
        }
        public string Text { get; set; }
        public int AdditionalInfo { get; set; }
        public bool IsCommand { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}