using Gma.System.MouseKeyHook;
using m_record.Constants;
using m_record.Helpers;
using System;
using System.Windows.Forms;

namespace m_record.Services
{
    /// <summary>
    /// Service for capturing keyboard and mouse input.
    /// </summary>
    /// <param name="onKeyLog"></param>
    /// <param name="onMouseLog"></param>
    // Primary Constructor for InputHookService
    public class InputHookService(Action<string> onKeyLog, Action<string> onMouseLog) : IDisposable
    {
        private IKeyboardMouseEvents? _globalHook;
        private readonly Action<string> _onKeyLog = onKeyLog;
        private readonly Action<string> _onMouseLog = onMouseLog;

        public void Start()
        {
            if (_globalHook == null)
            {
                _globalHook = Hook.GlobalEvents();
                _globalHook.KeyDown += GlobalHook_KeyDown;
                _globalHook.MouseDownExt += GlobalHook_MouseDownExt;
            }
        }

        public void Stop()
        {
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= GlobalHook_KeyDown;
                _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
                _globalHook.Dispose();
                _globalHook = null;
            }
        }

        private void GlobalHook_KeyDown(object? sender, KeyEventArgs e)
        {
            string timestamp = DateTime.Now.ToString(AppConstants.LogTimestampFormat);
            var (processName, processfileName) = ForegroundAppHelper.GetForegroundAppInfo();

            bool isCtrl = e.Control;
            bool isAlt = e.Alt;
            bool isShift = e.Shift;
            bool isWin =
                (Control.ModifierKeys & Keys.LWin) == Keys.LWin ||
                (Control.ModifierKeys & Keys.RWin) == Keys.RWin;

            string csvRow = string.Format(AppConstants.LogContentsFormat,
                                timestamp,
                                AppConstants.LogTypeKeyBoard,
                                isCtrl ? AppConstants.LogLetterTrue : AppConstants.LogLetterFalse,
                                isAlt ? AppConstants.LogLetterTrue : AppConstants.LogLetterFalse,
                                isShift ? AppConstants.LogLetterTrue : AppConstants.LogLetterFalse,
                                isWin ? AppConstants.LogLetterTrue : AppConstants.LogLetterFalse,
                                e.KeyCode,
                                processName, processfileName,
                                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            _onKeyLog(csvRow);
        }

        private void GlobalHook_MouseDownExt(object? sender, MouseEventExtArgs e)
        {
            var point = new System.Drawing.Point(e.X, e.Y);

            var windowTitle = MouseWindowHelper.GetWindowTitleAtPoint(point);
            var parentWindowTitle = MouseWindowHelper.GetParentWindowTitleAtPoint(point);
            var processName = MouseWindowHelper.GetProcessNameAtPoint(point);
            var processfileName = MouseWindowHelper.GetProcessFileNameAtPoint(point);

            string timestamp = DateTime.Now.ToString(AppConstants.LogTimestampFormat);
            string csvRow = string.Format(AppConstants.LogContentsFormat,
                                  timestamp,
                                  AppConstants.LogTypeMouse,
                                  string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                  processName, processfileName,
                                  parentWindowTitle, windowTitle,
                                  string.Empty, $"{e.X}", $"{e.Y}");

            _onMouseLog(csvRow);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}