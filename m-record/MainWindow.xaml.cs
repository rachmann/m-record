using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace m_record
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isRecording = false;
        private DispatcherTimer timer;
        private int elapsedSeconds = 0;
        private bool isDarkMode = false;

        public MainWindow()
        {
            InitializeComponent();
            UpdateRecordingState();

            // Set up timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            elapsedSeconds++;
            TimerText.Text = TimeSpan.FromSeconds(elapsedSeconds).ToString(@"hh\:mm\:ss");
        }

        private void UpdateRecordingState()
        {
            // This method can be used to update UI or other states when recording state changes.
        }
        private void ApplyTheme()
        {
            // Window and border
            this.Background = isDarkMode ? Brushes.Black : Brushes.White;
            if (this.Content is Border border)
            {
                border.Background = isDarkMode ? Brushes.Black : Brushes.White;
                border.BorderBrush = isDarkMode ? Brushes.Gray : new SolidColorBrush(Color.FromRgb(136, 136, 136));
            }
            // Menu items:
            // Menu and Close icons
            ContextMenuButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;
            CloseButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;

            if (isDarkMode)
            {
                MenuIcon.Style = (Style)FindResource("MenuIconDarkModeStyle");
                CloseIcon.Style = (Style)FindResource("CloseIconDarkModeStyle");
            }
            else
            {
                MenuIcon.Style = null;
                MenuIcon.Foreground = Brushes.Black;
                CloseIcon.Style = null;
                CloseIcon.Foreground = Brushes.Black;
            }

            // Context menu background and text
            if (ContextMenuButton.ContextMenu is ContextMenu menu)
            {
                menu.Background = isDarkMode ? Brushes.Black : Brushes.White;
                menu.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
                foreach (var item in menu.Items)
                {
                    if (item is MenuItem mi)
                    {
                        mi.Background = isDarkMode ? Brushes.Black : Brushes.White;
                        mi.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
                        mi.Style = isDarkMode ? (Style)FindResource("DarkModeMenuItemStyle") : null;
                    }
                }
            }

            // Main text
            TimerText.Foreground = isDarkMode ? Brushes.White : Brushes.Black;

            // Play/Stop icon
            PlayStopButton.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;
            // use this for other buttons?
            // PlayStopButton.Style = isDarkMode ? (Style)FindResource("DarkModeButtonStyle") : null;
            PlayStopButton.Style = isDarkMode ? (Style)FindResource("DarkModePlayStopButtonStyle") : null;

            // Status icon: keep red/green, but set background if needed
            StatusIcon.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;

        }

        #region Event Handlers
        private void PlayStopButton_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse entered PlayStopButton");
             System.Diagnostics.Debug.WriteLine($"PlayStopIcon.Foreground: {PlayStopIcon.Foreground}");
        }

        private void PlayStopButton_MouseLeave(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse left PlayStopButton");
        }

        private void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            isRecording = !isRecording;
            if (isRecording)
            {
                StatusIcon.Foreground = Brushes.Red;
                PlayStopIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.StopCircleOutline;
                elapsedSeconds = 0;
                TimerText.Text = "00:00:00";
                timer.Start();
            }
            else
            {
                StatusIcon.Foreground = Brushes.Green;
                PlayStopIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;
                timer.Stop();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseForm_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog(isDarkMode) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                isDarkMode = dlg.IsDarkMode;
                ApplyTheme();
            }
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Help clicked.");
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("About clicked.");
        }

        #endregion
    }
}