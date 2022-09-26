using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pyro.IO.PyroSc.Keywords
{
    public class Function : ExecutableKeyword
    {
        public Function(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
        }

        public IEnumerable<Keyword>[] Words => Source.Words;
        public Keyword[][] Body;
        public int ClosingBraceLineIndex;
        public override object Run()
        {
            if (Body is not null)
            {
                return _run();
            }

            Name = NextAsString();
            Source.Scope.Add(Name, this);
            var closingBrace = Words.First(l =>
                                    {
                                        return l.Any(k => typeof(FunctionClose) == k.GetType());
                                    })
                                    .First();
            ClosingBraceLineIndex = closingBrace.LineIndex;
            //function opening should be at the next line.
            Body = Words.Skip(LineIndex + 2).Take(closingBrace.LineIndex - LineIndex - 1).Select(l => l.ToArray()).ToArray();

            return null;
        }

        private object _run()
        {
            object last = null;
            foreach (var line in Body)
            {
                foreach (var keyword in line)
                {
                    if (keyword.IsRunnable)
                    {
                        last = keyword.Run();
                    }
                }
            }

            return last;
        }
    }
}