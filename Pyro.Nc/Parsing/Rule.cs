using System;
using System.Linq;

namespace Pyro.Nc.Parsing
{
    public class Rule
    {
        public Rule(string[] whitelist, string[] blacklist)
        {
            Whitelist = whitelist;
            Blacklist = blacklist;
        }
        
        public string[] Whitelist;
        public string[] Blacklist;

        public virtual bool IsValid(GCode.Line line)
        {
            return Blacklist.All(x =>
            {
                return _parse(new GParser(), x);
            });
        }

        private Func<GParser, string, bool> _parse = (gp, s) =>
        {
            bool flag = false;
            var lineSplit = gp.Split().ToArray();
            foreach (var lineStr in lineSplit)
            {
                foreach (var part in lineStr)
                {
                    flag = part.Contains(s);
                }
            }

            return flag;
        };
    }
}