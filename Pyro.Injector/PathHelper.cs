using System.Text;

namespace Pyro.Injector
{
    public static class PathHelper
    {
        internal static string RemoveBasePath(this string s, string path)
        {
            StringBuilder builder = new StringBuilder(s);
            builder.Replace(path, string.Empty).Remove(0, 1);
            return builder.ToString();
        }
    }
}