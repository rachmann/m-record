using m_record.Dialogs;
using m_record.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m_record.Services
{
    public class DialogService : IDialogService
    {
        public ISettingsDialog? SettingsDialog { get; set; }

        public bool? ShowSettingsDialog()
        {
            if (SettingsDialog != null)
            {
                SettingsDialog.Close();
                SettingsDialog = null;
            }
            SettingsDialog = new SettingsDialog();
            return SettingsDialog.ShowDialog();
           
        }

        public void ShowHelpDialog()
        {
            var dlg = new HelpDialog();
            dlg.ShowDialog();
        }

        public void ShowAboutDialog()
        {
            var dlg = new AboutDialog();
            dlg.ShowDialog();
        }
    }

}
