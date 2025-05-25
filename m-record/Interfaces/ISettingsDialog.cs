using m_record.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Interfaces
{
    public interface ISettingsDialog
    {
        public bool? ShowDialog();
        public void Close();

        public bool IsDarkMode {  get; }
        public NotificationStyle SelectedNotificationStyle { get; }
        public string SelectedRecordingPath { get; }
        public ScreenCaptureStyle SelectedScreenCaptureStyle { get; }
    }
}
