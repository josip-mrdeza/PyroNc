using System;
using System.Diagnostics;

namespace Pyro.Nc.Parser
{
    public class Block : IDisposable
    {
        public Block(string text, bool isCommand)
        {
            Text = text;
            IsCommand = isCommand;
        }
        public string Text { get; set; }
        public int Line { get; set; }
        public Information AdditionalInfo { get; set; }
        public bool IsCommand { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public void Dispose()
        {
            Text = null;
            Line = 0;
            AdditionalInfo = Information.None;
            IsCommand = false;
        }
    }
}