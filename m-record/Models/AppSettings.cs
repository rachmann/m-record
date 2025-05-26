using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Models
{
    public class AppSettings
    {
        public bool IsDarkMode { get; set; }
        public int NotifyStyle { get; set; }
        public int ScreenCaptureStyle { get; set; }
        public string RecordPath { get; set; } = string.Empty;
    }
}
