using System.Text;
using System.IO;
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
using Gma.System.MouseKeyHook;

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
        private IKeyboardMouseEvents? _globalHook;
        private List<string> _keystrokeLog = new();
        private DispatcherTimer _notificationTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            isDarkMode = Properties.Settings.Default.IsDarkMode;
            ApplyTheme();
            UpdateRecordingState();

            // Set up timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            _notificationTimer.Interval = TimeSpan.FromSeconds(4);
            _notificationTimer.Tick += NotificationTimer_Tick;
        }

        private void StartRecordingKeystrokes()
        {
            if (_globalHook == null)
            {
                _globalHook = Hook.GlobalEvents();
                _globalHook.KeyDown += GlobalHook_KeyDown;
            }
        }

        private void StopRecordingKeystrokes()
        {
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= GlobalHook_KeyDown;
                _globalHook.Dispose();
                _globalHook = null;
            }
        }

        private void NotificationTimer_Tick(object? sender, EventArgs e)
        {
            NotificationArea.Visibility = Visibility.Collapsed;
            _notificationTimer.Stop();
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

        // Helper to get the save directory, falling back to Desktop if not set
        private string GetRecordPath()
        {
            var path = Properties.Settings.Default.RecordPath;
            if (string.IsNullOrWhiteSpace(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private MessageBoxResult ShowNotification(string message, string reason, MessageBoxImage messageType)
        {
            NotificationStyle notifySetting;

            // Try to parse the setting, fallback to None if invalid
            try
            {
                notifySetting = (NotificationStyle)Properties.Settings.Default.NotifyStyle;
                // Defensive: check if value is defined in the enum
                if (!Enum.IsDefined(typeof(NotificationStyle), notifySetting))
                    notifySetting = NotificationStyle.None;
            }
            catch
            {
                notifySetting = NotificationStyle.None;
            }

            if (notifySetting == NotificationStyle.None)
                return MessageBoxResult.None;

            if(messageType == MessageBoxImage.Question)
            {
                return MessageBox.Show(message, reason, MessageBoxButton.OKCancel, messageType);
            }

            if (notifySetting == NotificationStyle.MessageOnly || notifySetting == NotificationStyle.Both)
            {
                MessageBox.Show(message, reason, MessageBoxButton.OK, messageType);
            }

            if (notifySetting == NotificationStyle.NotificationArea || notifySetting == NotificationStyle.Both)
            {
                NotificationArea.Visibility = Visibility.Visible;
                var prefix =
                    messageType == MessageBoxImage.Error  
                        ? "Error: "
                        : messageType == MessageBoxImage.Information
                            ? "Info: "
                            : messageType == MessageBoxImage.Warning
                                ? "Warning: "
                                : messageType == MessageBoxImage.Exclamation
                                    ? "Alert: "
                                    : "";
                NotificationArea.Text +=
                 (string.IsNullOrEmpty(NotificationArea.Text) ? "" : "\n") + prefix + message;
                NotificationArea.ScrollToEnd();
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }

            return MessageBoxResult.None;
        }

        #region Event Handlers

        private void NotificationArea_MouseEnter(object sender, MouseEventArgs e)
        {
            _notificationTimer.Stop();
        }

        private void NotificationArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (NotificationArea.Visibility == Visibility.Visible && !NotificationArea.IsFocused)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        private void NotificationArea_GotFocus(object sender, RoutedEventArgs e)
        {
            _notificationTimer.Stop();
        }

        private void NotificationArea_LostFocus(object sender, RoutedEventArgs e)
        {
            if (NotificationArea.Visibility == Visibility.Visible && !NotificationArea.IsMouseOver)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        private void StatusIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                _notificationTimer.Stop();
                if (NotificationArea.Visibility == Visibility.Visible)
                {
                    NotificationArea.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NotificationArea.Visibility = Visibility.Visible;
                    _notificationTimer.Start();
                }
                
                return;
            }
               

            // 1. Get timestamp in the same format as keylog
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // 2. Capture the screen
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            if (primaryScreen == null)
            {
                ShowNotification("No primary screen detected.", "Error", MessageBoxImage.Error);
                return;
            }

            var screenBounds = primaryScreen.Bounds;
            using (var bmp = new System.Drawing.Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(screenBounds.Location, System.Drawing.Point.Empty, screenBounds.Size);
                }
                // 3. Save the image
                string fileName = $"m_r_{timestamp.Replace(":", "-").Replace(" ", "_")}.png";
                string filePath = System.IO.Path.Combine(GetRecordPath(), fileName);
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                ShowNotification($"Screen capture saved to: {filePath}", "Saved Screen Shot", MessageBoxImage.Information);
            }

            // 4. Add to keylog
            _keystrokeLog.Add(string.Format("{0},F,F,F,F,SCREENCAP", timestamp));
        }

        private void GlobalHook_KeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Add CSV header if this is the first entry
            if (_keystrokeLog.Count == 0)
            {
                _keystrokeLog.Add("timestamp,IsCtrl,IsAlt,IsShift,IsWin,KeyCode");
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // Modifiers
            bool isCtrl = e.Control;
            bool isAlt = e.Alt;
            bool isShift = e.Shift;
            bool isWin =
                (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.LWin) == System.Windows.Forms.Keys.LWin ||
                (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.RWin) == System.Windows.Forms.Keys.RWin;

            string csvRow = string.Format("{0},{1},{2},{3},{4},{5}",
                timestamp,
                isCtrl ? "T" : "F",
                isAlt ? "T" : "F",
                isShift ? "T" : "F",
                isWin ? "T" : "F",
                e.KeyCode);

            _keystrokeLog.Add(csvRow);
            // Optionally, write to file or update UI
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
                StartRecordingKeystrokes();
            }
            else
            {
                StatusIcon.Foreground = Brushes.Green;
                PlayStopIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;
                timer.Stop();
                StopRecordingKeystrokes();

                // Save keystroke log to CSV file
                var logFilePath = System.IO.Path.Combine(
                      GetRecordPath(),
                      $"keystrokes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                File.WriteAllLines(logFilePath, _keystrokeLog);

                ShowNotification($"Keystroke log saved to: {logFilePath}", "Saved Keystroke Log", MessageBoxImage.Information);
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
            var notifySetting = (NotificationStyle)Properties.Settings.Default.NotifyStyle;
            // Defensive: check if value is defined in the enum
            if (!Enum.IsDefined(typeof(NotificationStyle), notifySetting))
                notifySetting = NotificationStyle.None;

            var dlg = new SettingsDialog(isDarkMode, notifySetting) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                isDarkMode = dlg.IsDarkMode;
                Properties.Settings.Default.IsDarkMode = isDarkMode;
                Properties.Settings.Default.Save();
                ApplyTheme();
            }
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("Help clicked.", "Info", MessageBoxImage.Information);

        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutDialog { Owner = this };
            dlg.ShowDialog();
        }

        #endregion
    }
}