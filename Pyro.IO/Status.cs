using System.Runtime.InteropServices;
using System.Windows;

namespace Pyro.IO
{
    public static class Status
    {
        public static bool IsIdle
        {
            get
            {
                var current = GetCursorPosition();
                if (LastPosition is null)
                {
                    LastPosition = current;

                    return false;
                }

                var lp = LastPosition.Value;
                return (current.X == lp.X) && (current.Y == lp.Y);
            }
        }

        private static POINT? LastPosition;
        private struct POINT
        {
            public int X;
            public int Y;
        }
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private static POINT GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)
        
            return lpPoint;
        }
        
    }
}