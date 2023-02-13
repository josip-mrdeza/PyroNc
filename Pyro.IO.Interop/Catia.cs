using System.Runtime.InteropServices;
using INFITF;

namespace Pyro.IO.Interop
{
    public static class Catia
    {
        private static Application _app;

        public static INFITF.Application App
        {
            get
            {
                return _app ??= (Application)Marshal.GetActiveObject("CATIA.Application");
            }
        }
    }
}