using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pyro.Nc
{
    public static class Database
    {
        static Database()
        {
            if (!File.Exists(gfunc__))
            {
                File.Create(gfunc__).Close();
            } 
            
            if (!File.Exists(mfunc__))
            {
                File.Create(mfunc__).Close();
            } 
            
            if (!File.Exists(altfunc__))
            {
                File.Create(altfunc__).Close();
            } 
        }
        public static List<string> GFunctions = File.ReadLines(gfunc__).ToList();
        public static List<string> MFunctions = File.ReadLines(mfunc__).ToList();
        public static List<string> AltFunctions = File.ReadLines(altfunc__).ToList();

        private static readonly string gfunc__ = "g_functions.txt";
        private static readonly string mfunc__ = "m_functions.txt";
        private static readonly string altfunc__ = "alt_functions.txt";

    }
}