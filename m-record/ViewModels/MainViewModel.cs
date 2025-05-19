using m_record.Enums;
using m_record.Extensions;
using m_record.Helpers;
using m_record.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace m_record.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isRecording;
        public bool CanCaptureScreens => IsRecording;
        private int _elapsedSeconds;
        private bool _isDarkMode;
        private string _notificationText = String.Empty;
        private string _timerText = Constants.Constants.TimerInitialText;
        private Brush _statusIconForeground = Brushes.Green;
        private object _playStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;
      
        private readonly ScreenCaptureService _screenCaptureService = new();
        private readonly LoggingService _loggingService = new();
        private readonly NotificationService _notificationService;
        private InputHookService? _inputHookService;

        private readonly DispatcherTimer _timer;
        private DispatcherTimer _notificationTimer = new DispatcherTimer();

        private Visibility _notificationAreaVisibility = Visibility.Collapsed;
     
        public ICommand PlayStopCommand { get; }
        public ICommand CaptureScreensCommand { get; }


        public event PropertyChangedEventHandler? PropertyChanged;



        public MainViewModel()
        {
           
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => ElapsedSeconds++;
            PlayStopCommand = new RelayCommand(_ => ToggleRecording());
            // to only allow screen capture when recording is active...
            //   CaptureScreensCommand = new RelayCommand(_ => CaptureScreens(), _ => IsRecording);
            // but we have logic to show the notification area when not recording
            CaptureScreensCommand = new RelayCommand(_ => CaptureScreens());

            _notificationTimer.Interval = TimeSpan.FromSeconds(4);
            _notificationTimer.Tick += (s, e) =>
            {
                NotificationAreaVisibility = Visibility.Collapsed;
                _notificationTimer.Stop();
            };

            _notificationService = new NotificationService(AppendNotificationText);

            _isDarkMode = Properties.Settings.Default.IsDarkMode;


            
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
        private void Timer_Tick(object? sender, EventArgs e)
        {
            _elapsedSeconds++;
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
            set { _isDarkMode = value; OnPropertyChanged(); }
        }

        private void ToggleRecording()
        {
            IsRecording = !IsRecording;
            if (IsRecording)
            {
                StatusIconForeground = Brushes.Red;
                PlayStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.StopCircleOutline;
                ElapsedSeconds = 0;
                TimerText = Constants.Constants.TimerInitialText;
                _timer.Start();
                _loggingService.GetCurrentLogFilePath(); // Ensure log file is ready
                _inputHookService = new InputHookService(
                    csvRow => _loggingService.AppendLog(csvRow),
                    csvRow => _loggingService.AppendLog(csvRow)
                );
                _inputHookService.Start();
            }
            else
            {
                StatusIconForeground = Brushes.Green;
                PlayStopIconKind = MahApps.Metro.IconPacks.PackIconMaterialKind.RecordCircleOutline;
                _timer.Stop();
                _inputHookService?.Stop();
                _inputHookService?.Dispose();
                _inputHookService = null;

                _notificationService.ShowNotification(Constants.Constants.KeystrokeLogSavedMessage, Constants.Constants.KeystrokeLogSavedTitle, MessageBoxImage.Information);
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
            string timestamp = nowDate.ToString(Constants.Constants.LogTimestampFormat);
            var (processName, processfileName) = ForegroundAppHelper.GetForegroundAppInfo();
            string directory = _loggingService.GetRecordPath();
           
            var screenFilePaths = _screenCaptureService.CaptureAllScreens(nowDate, directory);

            if (screenFilePaths == null || screenFilePaths.Count == 0)
            {
                if(_screenCaptureService.screenCaptureStyle != ScreenCaptureStyle.None)
                    _notificationService.ShowNotification(Constants.Constants.NoScreensDetectedMessage, Constants.Constants.ErrorTitle, MessageBoxImage.Error);
                return;
            }

            for (int i = 0; i < screenFilePaths.Count; i++)
            {
                _notificationService.ShowNotification($"{Constants.Constants.ScreenCaptureSavedMessage} : {screenFilePaths[i]}" , Constants.Constants.ScreenCaptureSavedTitle, MessageBoxImage.Information);

                string csvRow = string.Format(Constants.Constants.LogContentsFormat,
                                      timestamp,
                                      Constants.Constants.LogTypeScreenCap,
                                      string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                                      processName, processfileName,
                                      string.Empty,
                                      $"Screen_{i + 1}",
                                      screenFilePaths[i],
                                      string.Empty, string.Empty);

                _loggingService.AppendLog(csvRow);
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

        // Notification area mouse events for timer control
        public void NotificationArea_MouseEnter()
        {
            _notificationTimer.Stop();
        }

        public void NotificationArea_MouseLeave()
        {
            if (NotificationAreaVisibility == Visibility.Visible)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        public void NotificationArea_GotFocus()
        {
            _notificationTimer.Stop();
        }

        public void NotificationArea_LostFocus()
        {
            if (NotificationAreaVisibility == Visibility.Visible)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}