using Gma.System.MouseKeyHook;
using m_record.Helpers;
using System;
using System.Windows.Forms;

namespace m_record.Services
{
    public class InputHookService : IDisposable
    {
        private IKeyboardMouseEvents? _globalHook;
        private readonly Action<string> _onKeyLog;
        private readonly Action<string> _onMouseLog;

        public InputHookService(Action<string> onKeyLog, Action<string> onMouseLog)
        {
            _onKeyLog = onKeyLog;
            _onMouseLog = onMouseLog;
        }

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
            string timestamp = DateTime.Now.ToString(Constants.Constants.LogTimestampFormat);
            var (processName, processfileName) = ForegroundAppHelper.GetForegroundAppInfo();

            bool isCtrl = e.Control;
            bool isAlt = e.Alt;
            bool isShift = e.Shift;
            bool isWin =
                (Control.ModifierKeys & Keys.LWin) == Keys.LWin ||
                (Control.ModifierKeys & Keys.RWin) == Keys.RWin;

            string csvRow = string.Format(Constants.Constants.LogContentsFormat,
                                timestamp,
                                Constants.Constants.LogTypeKeyBoard,
                                isCtrl ? Constants.Constants.LogLetterTrue : Constants.Constants.LogLetterFalse,
                                isAlt ? Constants.Constants.LogLetterTrue : Constants.Constants.LogLetterFalse,
                                isShift ? Constants.Constants.LogLetterTrue : Constants.Constants.LogLetterFalse,
                                isWin ? Constants.Constants.LogLetterTrue : Constants.Constants.LogLetterFalse,
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

            string timestamp = DateTime.Now.ToString(Constants.Constants.LogTimestampFormat);
            string csvRow = string.Format(Constants.Constants.LogContentsFormat,
                                  timestamp,
                                  Constants.Constants.LogTypeMouse,
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