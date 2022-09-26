using System;
using System.Collections.Generic;
using System.Linq;
using Pyro.IO.PyroSc.Keywords;

namespace Pyro.IO.PyroSc
{
    public class KeywordLocalStorage
    {
        private Dictionary<string, object> DefinedVariables { get; set; }

        public KeywordLocalStorage()
        {
            DefinedVariables = new Dictionary<string, object>();
        }

        public void Add(string name, object value)
        {
            name = name.TrimEnd();
            if (DefinedVariables.ContainsKey(name))
            {
                Change(name, value);

                return;
            }
            DefinedVariables.Add(name, value);
        }

        public object Get(string name)
        {
            name = name.TrimEnd();
            if (Exists(name))
            {
                var value = DefinedVariables[name];
                if (value is Function f)
                {
                    return f.Run();
                }

                return value;
            }

            throw new NotSupportedException();
        }

        public void Change(string name, object value)
        {
            DefinedVariables[name.TrimEnd()] = value;
        }

        public bool Exists(string name)
        {
            return DefinedVariables.ContainsKey(name.TrimEnd());
        }
    }
}