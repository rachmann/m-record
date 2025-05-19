using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Enums
{

    public enum ScreenCaptureStyle
    {
        [Description("None")]
        None = 0,
        [Description("Primary")]
        PrimaryScreen = 1,
        [Description("All Screens")]
        AllScreens = 2,
    }
}
