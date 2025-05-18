using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Helpers
{
    public static class MouseWindowHelper
    {
        [DllImport("user32.dll")]
        private static extern nint WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        private static extern nint GetParent(nint hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

        public static string GetWindowTitleAtPoint(Point screenPoint)
        {
            nint hWnd = WindowFromPoint(screenPoint);
            return GetWindowTitle(hWnd);
        }

        public static string GetParentWindowTitleAtPoint(Point screenPoint)
        {
            nint hWnd = WindowFromPoint(screenPoint);
            nint hParent = GetParent(hWnd);
            return GetWindowTitle(hParent);
        }

        public static string GetWindowTitle(nint hWnd)
        {
            if (hWnd == nint.Zero)
                return string.Empty;

            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetProcessNameAtPoint(Point screenPoint)
        {
            nint hWnd = WindowFromPoint(screenPoint);
            GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                var proc = Process.GetProcessById((int)pid);
                return proc.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the full process filename (executable path) for a given window handle.
        /// </summary>
        public static string GetProcessFileName(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return string.Empty;

            GetWindowThreadProcessId(hWnd, out uint pid);
            try
            {
                var proc = Process.GetProcessById((int)pid);
                return proc.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the full process filename (executable path) for the window at a given screen point.
        /// </summary>
        public static string GetProcessFileNameAtPoint(Point screenPoint)
        {
            IntPtr hWnd = WindowFromPoint(screenPoint);
            return GetProcessFileName(hWnd);
        }
    }
}
