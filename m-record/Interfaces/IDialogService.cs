using m_record.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Interfaces
{
    public interface IDialogService
    {
        public ISettingsDialog? SettingsDialog { get; set; }

        bool? ShowSettingsDialog();
        void ShowHelpDialog();
        void ShowAboutDialog();


    }

}
