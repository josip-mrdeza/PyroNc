namespace Pyro.IO.PyroSc
{
    public class ExecutableKeyword : Keyword
    {
        public ExecutableKeyword(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
            EnableExecution();
        }
    }
}