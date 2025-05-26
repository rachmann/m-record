using m_record.Constants;
using m_record.Enums;
using m_record.Helpers;
using m_record.Interfaces;
using m_record.Models;
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
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings Settings => _appSettingsService.Current;
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
            NotificationService notificationService,
            IAppSettingsService appSettingsService
             )
        {
            _logger = logger;
            _dialogService = dialogService;
            _screenCaptureService = screenCaptureService;
            _inputLoggingService = loggingService;
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));

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
            OpenSettingsCommand = new RelayCommand(_ => _dialogService.ShowSettingsDialog());
            OpenHelpCommand = new RelayCommand(_ => _dialogService.ShowHelpDialog());
            OpenAboutCommand = new RelayCommand(_ => _dialogService.ShowAboutDialog());
            OpenSettingsCommand = new RelayCommand(_ =>
            {
                var result = _dialogService.ShowSettingsDialog();
                if (result != null && (bool)result == true)
                {
                    ApplyTheme();
                }
            });
            _notificationTimer.Interval = TimeSpan.FromSeconds(4);
            _notificationTimer.Tick += (s, e) =>
            {
                NotificationAreaVisibility = Visibility.Collapsed;
                _notificationTimer.Stop();
            };

            _isDarkMode = Settings.IsDarkMode;

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
            get => Settings.IsDarkMode;
            set
            {
                if (Settings.IsDarkMode != value)
                {
                    _appSettingsService.Update(s => s.IsDarkMode = value);
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
            string directory = _inputLoggingService.GetRecordPath();

            var screenFilePaths = _screenCaptureService.CaptureAllScreens(nowDate, directory);

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