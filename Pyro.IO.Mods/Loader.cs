using System.Collections.Generic;
using System.Reflection;

namespace Pyro.IO.Mods
{
    public class Loader
    {
        public List<Assembly> LoadedAssemblies { get; set; }
        public Dictionary<int, Assembly> LoaderOrder { get; }
                                             
        public Dictionary<string, Dictionary<string, Method>> Methods { get; }

        public Method Load()
        {
            return null;
        }

        public Method Prefix(Method method, Method prefixMethod)
        {
            method.Prefix(prefixMethod);

            return method;
        }
    }
}