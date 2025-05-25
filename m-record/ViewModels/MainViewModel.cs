using m_record.Constants;
using m_record.Enums;
using m_record.Helpers;
using m_record.Interfaces;
using m_record.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace m_record.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<MainViewModel> _logger;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? RequestClose;

        private bool _isRecording;
        public bool CanCaptureScreens => IsRecording;
        private int _elapsedSeconds;
        private bool _isDarkMode;
        private string _notificationText = string.Empty;
        private string _timerText = AppConstants.TimerInitialText;
        private Brush _statusIconForeground = Brushes.Green;
        private object _playStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;


        private Brush _windowBackground = Brushes.Black;
        public Brush WindowBackground
        {
            get => _windowBackground;
            set { _windowBackground = value; OnPropertyChanged(); }
        }

        private Brush _contentBorderBackground = Brushes.Black;
        public Brush ContentBorderBackground
        {
            get => _contentBorderBackground;
            set { _contentBorderBackground = value; OnPropertyChanged(); }
        }
        
        /////////////////////////////////////////////////
        // context menu
        //
        // context menu
        private Style? _contextMenuButtonStyle;
        public Style? ContextMenuButtonStyle
        {
            get => _contextMenuButtonStyle;
            set {
                _contextMenuButtonStyle = value;
                OnPropertyChanged(); }
        }
        // context menu icon
        private Style? _contextMenuIconStyle;
        public Style? ContextMenuIconStyle
        {
            get => _contextMenuIconStyle;
            set { _contextMenuIconStyle = value; OnPropertyChanged(); }
        }

        // context menu item
        private Style? _contextMenuItemStyle;
        public Style? ContextMenuItemStyle
        {
            get => _contextMenuItemStyle;
            set { _contextMenuItemStyle = value; OnPropertyChanged(); }
        }

        // context menu icon style
        private Style? _contextMenuItemIconStyle;
        public Style? ContextMenuItemIconStyle
        {
            get => _contextMenuItemIconStyle;
            set { _contextMenuItemIconStyle = value; OnPropertyChanged(); }
        }

        /////////////////////////////////////////////////
        //close button
        private Style? _closeButtonStyle;
        public Style? CloseButtonStyle
        {
            get => _closeButtonStyle;
            set { _closeButtonStyle = value; OnPropertyChanged(); }
        }

        // close button icon
        private Style? _closeIconStyle;
        public Style? CloseIconStyle
        {
            get => _closeIconStyle;
            set { _closeIconStyle = value; OnPropertyChanged(); }
        }

        private Brush _timerTextForeground = Brushes.Black;
        public Brush TimerTextForeground
        {
            get => _timerTextForeground;
            set { _timerTextForeground = value; OnPropertyChanged(); }
        }

        private Style? _playStopButtonStyle;
        public Style? PlayStopButtonStyle
        {
            get => _playStopButtonStyle;
            set { _playStopButtonStyle = value; OnPropertyChanged(); }
        }

        private Style? _statusIconButtonStyle;
        public Style? StatusIconButtonStyle
        {
            get => _statusIconButtonStyle;
            set { _statusIconButtonStyle = value; OnPropertyChanged(); }
        }

        private readonly ScreenCaptureService _screenCaptureService;
        private readonly InputLoggingService _inputLoggingService;
        private readonly NotificationService _notificationService;
        private InputHookService? _inputHookService;
        private readonly IDialogService _dialogService;

        // add error logging here with a logging framework

        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _notificationTimer = new();

        private Visibility _notificationAreaVisibility = Visibility.Collapsed;

        public ICommand PlayStopCommand { get; }
        public ICommand CaptureScreensCommand { get; }
        public ICommand NotificationAreaMouseEnterCommand { get; }
        public ICommand NotificationAreaMouseLeaveCommand { get; }
        public ICommand NotificationAreaGotFocusCommand { get; }
        public ICommand NotificationAreaLostFocusCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenHelpCommand { get; }
        public ICommand OpenAboutCommand { get; }

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDialogService dialogService,
            ScreenCaptureService screenCaptureService,
            InputLoggingService loggingService,
            NotificationService notificationService)
        {
            _logger = logger;
            _dialogService = dialogService;
            _screenCaptureService = screenCaptureService;
            _inputLoggingService = loggingService;

            _notificationService = notificationService;
            _notificationService.SetActionDelegates(AppendNotificationText);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => ElapsedSeconds++;

            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());

            PlayStopCommand = new RelayCommand(_ => ToggleRecording());
            CaptureScreensCommand = new RelayCommand(_ => CaptureScreens());
            NotificationAreaMouseEnterCommand = new RelayCommand(_ => OnNotificationAreaMouseEnter());
            NotificationAreaMouseLeaveCommand = new RelayCommand(_ => OnNotificationAreaMouseLeave());
            NotificationAreaGotFocusCommand = new RelayCommand(_ => OnNotificationAreaGotFocus());
            NotificationAreaLostFocusCommand = new RelayCommand(_ => OnNotificationAreaLostFocus());
          
            OpenSettingsCommand = new RelayCommand(_ =>
            {
                var result = _dialogService.ShowSettingsDialog();
                if (result != null && (bool)result == true)
                {
                    _isDarkMode = _dialogService.SettingsDialog?.IsDarkMode ?? false;
                    Properties.Settings.Default.IsDarkMode = _isDarkMode;
                    Properties.Settings.Default.NotifyStyle = (int)(_dialogService.SettingsDialog?.SelectedNotificationStyle ?? NotificationStyle.None);
                    Properties.Settings.Default.RecordPath = _dialogService.SettingsDialog?.SelectedRecordingPath ?? string.Empty;
                    Properties.Settings.Default.ScreenCaptureStyle = (int)(_dialogService.SettingsDialog?.SelectedScreenCaptureStyle ?? ScreenCaptureStyle.None);
                    Properties.Settings.Default.Save();

                    ApplyTheme();
                }
            });
            OpenHelpCommand = new RelayCommand(_ => _dialogService.ShowHelpDialog());
            OpenAboutCommand = new RelayCommand(_ => _dialogService.ShowAboutDialog());

            _notificationTimer.Interval = TimeSpan.FromSeconds(4);
            _notificationTimer.Tick += (s, e) =>
            {
                NotificationAreaVisibility = Visibility.Collapsed;
                _notificationTimer.Stop();
            };

            _isDarkMode = Properties.Settings.Default.IsDarkMode;

            ApplyTheme();

        }

        private void ApplyTheme()
        {
            var app = Application.Current;
            WindowBackground = IsDarkMode ? Brushes.Black : Brushes.White;
            ContentBorderBackground = IsDarkMode ? Brushes.Black : Brushes.White;

            ContextMenuButtonStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeTopButtonStyle") : null;
            ContextMenuIconStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeContextMenuIconStyle") : null;
            ContextMenuItemStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeTopMenuItemStyle") : null;
            ContextMenuItemIconStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeContextMenuIconStyle") : null;
            
            CloseButtonStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeTopButtonStyle") : null;
            CloseIconStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeCloseIconStyle") : null;

            TimerTextForeground = IsDarkMode ? Brushes.White : Brushes.Black;
            PlayStopButtonStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeActionButtonStyle") : null;
            StatusIconButtonStyle = IsDarkMode ? (Style?)app.TryFindResource("DarkModeActionButtonStyle") : null;

        }


        //private void ApplyTheme2()
        //{
        //    // Window and border
        //    this.Background = isDarkMode ? Brushes.Black : Brushes.White;
        //    if (this.Content is Border border)
        //    {
        //        border.Background = isDarkMode ? Brushes.Black : Brushes.White;
        //        border.BorderBrush = isDarkMode ? Brushes.Gray : new SolidColorBrush(Color.FromRgb(136, 136, 136));
        //    }
        //    // Menu items:
        //    // Menu and Close icons
        //    ContextMenuButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;
        //    CloseButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;

        //    if (isDarkMode)
        //    {
        //        // Clear any local Foreground value so the style can take effect
        //        MenuIcon.ClearValue(ForegroundProperty);
        //        CloseIcon.ClearValue(ForegroundProperty);

        //        MenuIcon.Style = (Style)FindResource("ContextMenuIconDarkModeStyle");
        //        CloseIcon.Style = (Style)FindResource("CloseIconDarkModeStyle");
        //    }
        //    else
        //    {
        //        MenuIcon.Style = null;
        //        MenuIcon.Foreground = Brushes.Black;
        //        CloseIcon.Style = null;
        //        CloseIcon.Foreground = Brushes.Black;
        //    }

        //    // Context menu background, text, and icon colors
        //    if (ContextMenuButton.ContextMenu is ContextMenu menu)
        //    {
        //        menu.Background = isDarkMode ? Brushes.Black : Brushes.White;
        //        menu.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
        //        foreach (var item in menu.Items)
        //        {
        //            if (item is MenuItem mi)
        //            {
        //                mi.Background = isDarkMode ? Brushes.Black : Brushes.White;
        //                mi.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
        //                mi.Style = isDarkMode ? (Style)FindResource("DarkModeMenuItemStyle") : null;

        //                // Set icon foreground for each menu item
        //                if (mi.Icon is MahApps.Metro.IconPacks.PackIconMaterial icon)
        //                {
        //                    icon.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
        //                }
        //            }
        //        }
        //    }

        //    // Main text
        //    TimerText.Foreground = isDarkMode ? Brushes.White : Brushes.Black;

        //    // Play/Stop icon
        //    PlayStopButton.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;
        //    // Play/Stop style
        //    PlayStopButton.Style = isDarkMode ? (Style)FindResource("DarkModePlayStopButtonStyle") : null;
        //    // Status icon: keep red/green, but set background if needed
        //    StatusIcon.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;

        //}

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanCaptureScreens));
                }
            }
        }

        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set
            {
                if (_elapsedSeconds != value)
                {
                    _elapsedSeconds = value;
                    TimerText = TimeSpan.FromSeconds(_elapsedSeconds).ToString(@"hh\:mm\:ss");
                    OnPropertyChanged();
                }
            }
        }

        public string TimerText
        {
            get => _timerText;
            set { _timerText = value; OnPropertyChanged(); }
        }

        public Brush StatusIconForeground
        {
            get => _statusIconForeground;
            set { _statusIconForeground = value; OnPropertyChanged(); }
        }

        public object PlayStopIconKind
        {
            get => _playStopIconKind;
            set { _playStopIconKind = value; OnPropertyChanged(); }
        }

        public string NotificationText
        {
            get => _notificationText;
            set { _notificationText = value; OnPropertyChanged(); }
        }

        public Visibility NotificationAreaVisibility
        {
            get => _notificationAreaVisibility;
            set { _notificationAreaVisibility = value; OnPropertyChanged(); }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    Properties.Settings.Default.IsDarkMode = _isDarkMode;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged();
                }
            }
        }


        private void ToggleRecording()
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                StatusIconForeground = Brushes.Red;
                PlayStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.StopCircleOutline;
                ElapsedSeconds = 0;
                TimerText = AppConstants.TimerInitialText;
                _timer.Start();
                _inputLoggingService.GetCurrentLogFilePath(); // Ensure log file is ready
                _inputHookService = new InputHookService(
                    csvRow => _inputLoggingService.AppendLog(csvRow),
                    csvRow => _inputLoggingService.AppendLog(csvRow)
                );
                _inputHookService.Start();
                _logger.LogInformation(AppConstants.InfoRecordingStarted);
            }
            else
            {
                StatusIconForeground = Brushes.Green;
                PlayStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;
                _timer.Stop();
                _inputHookService?.Stop();
                _inputHookService?.Dispose();
                _inputHookService = null;

                _notificationService.ShowNotification(AppConstants.KeystrokeLogSavedMessage, AppConstants.KeystrokeLogSavedTitle, MessageBoxImage.Information);
                _logger.LogInformation(AppConstants.InfoRecordingStopped);
            }
        }

        private void CaptureScreens()
        {
            if (!IsRecording)
            {
                _notificationTimer.Stop();
                NotificationAreaVisibility = NotificationAreaVisibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                if (NotificationAreaVisibility == Visibility.Visible)
                    _notificationTimer.Start();
                return;
            }

            var nowDate = DateTime.Now;
            string timestamp = nowDate.ToString(AppConstants.LogTimestampFormat);
            var (processName, processfileName) = ForegroundAppHelper.GetForegroundAppInfo();
            string directory = InputLoggingService.GetRecordPath();

            var screenFilePaths = ScreenCaptureService.CaptureAllScreens(nowDate, directory);

            if (screenFilePaths == null || screenFilePaths.Count == 0)
            {
                if (_screenCaptureService.screenCaptureStyle != ScreenCaptureStyle.None)
                {
                    _notificationService.ShowNotification(AppConstants.NoScreensDetectedMessage, AppConstants.ErrorTitle, MessageBoxImage.Error);
                    _logger.LogError(AppConstants.NoScreensDetectedMessage);
                }
                return;
            }

            for (int i = 0; i < screenFilePaths.Count; i++)
            {
                _notificationService.ShowNotification($"{AppConstants.ScreenCaptureSavedMessage} : {screenFilePaths[i]}", AppConstants.ScreenCaptureSavedTitle, MessageBoxImage.Information);

                string csvRow = string.Format(AppConstants.LogContentsFormat,
                                      timestamp,
                                      AppConstants.LogTypeScreenCap,
                                      string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                      processName, processfileName,
                                      string.Empty,
                                      $"Screen_{i + 1}",
                                      screenFilePaths[i],
                                      string.Empty, string.Empty);

                _inputLoggingService.AppendLog(csvRow);
            }
        }

        // Private handlers for notification area commands
        private void OnNotificationAreaMouseEnter() => _notificationTimer.Stop();

        private void OnNotificationAreaMouseLeave()
        {
            if (NotificationAreaVisibility == Visibility.Visible)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        private void OnNotificationAreaGotFocus() => _notificationTimer.Stop();

        private void OnNotificationAreaLostFocus()
        {
            if (NotificationAreaVisibility == Visibility.Visible)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        public void ShowNotification(string message, string reason, MessageBoxImage messageType)
        {
            _notificationService.ShowNotification(message, reason, messageType);
        }

        private void AppendNotificationText(string message)
        {
            NotificationAreaVisibility = Visibility.Visible;
            NotificationText += (string.IsNullOrEmpty(NotificationText) ? "" : "\n") + message;
            _notificationTimer.Stop();
            _notificationTimer.Start();
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}