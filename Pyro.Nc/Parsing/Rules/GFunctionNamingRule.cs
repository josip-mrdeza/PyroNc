namespace Pyro.Nc.Parsing.Rules
{
    public class GFunctionNamingRule : Rule
    {
        public GFunctionNamingRule() : base(Whitelist, Blacklist)
        {
        }
        
        public static string[] Whitelist = new string[]
        {
            "G00", "G0",
            "G01", "G1",
            "G02", "G2",
            "G03", "G3"
        };

        public static string[] Blacklist = new string[]
        {
            "M"
        };
    }
}