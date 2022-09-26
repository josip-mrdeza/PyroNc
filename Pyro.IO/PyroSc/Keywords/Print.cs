using System;

namespace Pyro.IO.PyroSc.Keywords
{
    public class Print : ExecutableKeyword
    {
        public Print(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
        }

        public override object Run()
        {
            var next = Next();
            var value = next.Run();
            Console.WriteLine(value);
            return null;
        }
    }
}