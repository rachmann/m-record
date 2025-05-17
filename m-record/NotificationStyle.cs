using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record
{
    public enum NotificationStyle
    {
        [Description("None")]
        None = 0,
        [Description("Message Only")]
        MessageOnly = 1,
        [Description("Notification Area Only")]
        NotificationArea = 2,
        [Description("Both")]
        Both = 4
    }
}