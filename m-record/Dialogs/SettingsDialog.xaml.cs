using m_record.Enums;
using m_record.ViewModels;
using System.Windows;

namespace m_record
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsViewModel SettingsViewModel { get; }

        public SettingsDialog(bool currentDarkMode, string recordingPath, NotificationStyle currentNotify, ScreenCaptureStyle currentScreenCapture)
        {
            InitializeComponent();

            SettingsViewModel = new SettingsViewModel();
            DataContext = SettingsViewModel;

            SettingsViewModel.LoadSettings(currentDarkMode, recordingPath, currentNotify, currentScreenCapture);
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
