using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Helpers
{

    public static class ForegroundAppHelper
    {
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

        private static Process? GetForegroundProcess()
        {
            nint hwnd = GetForegroundWindow();
            if (hwnd == nint.Zero)
                return null;

            GetWindowThreadProcessId(hwnd, out uint pid);
            try
            {
                return Process.GetProcessById((int)pid);
            }
            catch
            {
                return null;
            }
        }

        public static (string ProcessName, string? FileName) GetForegroundAppInfo()
        {
            var proc = GetForegroundProcess();
            if (proc == null)
                return (string.Empty, null);

            string? fileName = null;
            try
            {
                fileName = proc.MainModule?.FileName;
            }
            catch
            {
                // Access denied or process exited
            }
            return (proc.ProcessName, fileName);
        }
    }
}
