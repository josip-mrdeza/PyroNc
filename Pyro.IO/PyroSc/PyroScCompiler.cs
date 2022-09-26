using System.Linq;
using Pyro.IO.PyroSc.Keywords;

namespace Pyro.IO.PyroSc
{
    public class PyroScCompiler
    {
        public PyroScSourceParser Parser { get; }
        public Keyword[][] CompiledKeywords { get; private set; }
        public bool IsCompiled { get; private set; }

        public PyroScCompiler(PyroScSourceParser parser)
        {
            Parser = parser;
        }

        public PyroScCompiler Compile()
        {
            CompiledKeywords = Parser.Words.Select(ks => ks.ToArray()).ToArray();
            IsCompiled = true;

            return this;
        }

        public void Execute()
        {
            for (var i = 0; i < CompiledKeywords.Length; i++)
            {
                var line = CompiledKeywords[i];
                foreach (var keyword in line)
                {
                    if (keyword is Function f)
                    {
                        f.Run();
                        i = f.ClosingBraceLineIndex;
                    }
                    else
                    {
                        if (keyword.IsRunnable)
                        {
                            keyword.Run();
                        }
                    }
                }
            }
        }
    }
}