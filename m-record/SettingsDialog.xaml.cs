using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace m_record
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public bool IsDarkMode { get; private set; }

        public SettingsDialog(bool currentDarkMode)
        {
            InitializeComponent();
            DarkModeCheckBox.IsChecked = currentDarkMode;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            IsDarkMode = DarkModeCheckBox.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
