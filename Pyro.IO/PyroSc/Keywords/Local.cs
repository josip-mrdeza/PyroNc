namespace Pyro.IO.PyroSc.Keywords
{
    public class Local : Keyword
    {
        public Local(string name, string contents, int lineIndex, int elementIndex, PyroScSourceParser source) : base(name, contents, lineIndex, elementIndex, source)
        {
            IsRunnable = false;
        }
        public override object Run()
        {
            var nextName = NextAsString();
            var value = Source.Scope.Get(nextName);

            return value;
        }
    }
}