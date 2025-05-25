using m_record.Enums;
using m_record.Interfaces;
using m_record.ViewModels;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace m_record.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window, ISettingsDialog
    {
        public SettingsViewModel SettingsViewModel { get; }

        public SettingsDialog()
        {
            InitializeComponent();
        
            SettingsViewModel = new SettingsViewModel();
            DataContext = SettingsViewModel;
            // No parameters, ViewModel handles initialization
        }
     
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        public bool IsDarkMode => SettingsViewModel.IsDarkMode;
        public NotificationStyle SelectedNotificationStyle => SettingsViewModel.SelectedNotificationStyle;
        public string SelectedRecordingPath => SettingsViewModel.SelectedRecordingPath;
        public ScreenCaptureStyle SelectedScreenCaptureStyle => SettingsViewModel.SelectedScreenCaptureStyle;
    }
}
