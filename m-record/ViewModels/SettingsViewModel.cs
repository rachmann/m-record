using m_record.Constants;
using m_record.Enums;
using m_record.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace m_record.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<SettingsViewModel>? _logger;
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isDarkMode;
        private string _selectedRecordingPath = string.Empty;
        private NotificationStyle _selectedNotificationStyle;
        private ScreenCaptureStyle _selectedScreenCaptureStyle;

        public ObservableCollection<KeyValuePair<Enum, string>> ScreenCaptureOptions { get; }
        public ObservableCollection<KeyValuePair<Enum, string>> NotifyOptions { get; }

        public SettingsViewModel()
        {
            _logger = App.Services.GetService<ILogger<SettingsViewModel>>();
            if (_logger == null)
            {
                throw new InvalidOperationException("Logger not found in service provider.");
            }
            ScreenCaptureOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<ScreenCaptureStyle>());
            NotifyOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<NotificationStyle>());


            // Read from Properties.Settings.Default
            var notifySetting = (NotificationStyle)Properties.Settings.Default.NotifyStyle;
            if (!Enum.IsDefined(notifySetting))
            {
                _logger.LogError($"{AppConstants.ErrorNotificationStyleInvalid} {Properties.Settings.Default.NotifyStyle}");
                notifySetting = NotificationStyle.None;
            }

            if (notifySetting == NotificationStyle.None)
            {
                _logger.LogInformation(AppConstants.InfoNotificationStyleSetToNone);
            }

            var screenCaptureSetting = (ScreenCaptureStyle)Properties.Settings.Default.ScreenCaptureStyle;
            if (!Enum.IsDefined(screenCaptureSetting))
            {
                _logger.LogError($"{AppConstants.ErrorNotificationStyleInvalid} {Properties.Settings.Default.ScreenCaptureStyle}");
                screenCaptureSetting = ScreenCaptureStyle.None;
            }

            if (screenCaptureSetting == ScreenCaptureStyle.None)
            {
                _logger.LogInformation(AppConstants.InfoScreenCapStyleSetToNone);
            }

            var recordingPath = Properties.Settings.Default.RecordPath ??
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    AppConstants.AppName
                );

            if (string.IsNullOrWhiteSpace(recordingPath))
            {
                recordingPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    AppConstants.AppName
                );
                _logger.LogInformation($"{AppConstants.InfoRecordPathIsDefault} {recordingPath}");
            }

            if (!Directory.Exists(recordingPath))
            {
                try
                {
                    Directory.CreateDirectory(recordingPath);
                    _logger.LogInformation($"{AppConstants.InfoCreatedReportDirectory} {recordingPath}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"{AppConstants.ErrorFailedToCreateDir} {ex.Message}",
                        AppConstants.ErrorTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    _logger.LogError($"{AppConstants.ErrorFailedToCreateDir} {ex.Message}");
                }
            }

            IsDarkMode = Properties.Settings.Default.IsDarkMode;
            SelectedNotificationStyle = notifySetting;
            SelectedRecordingPath = recordingPath;
            SelectedScreenCaptureStyle = screenCaptureSetting;
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedRecordingPath
        {
            get => _selectedRecordingPath;
            set
            {
                if (_selectedRecordingPath != value)
                {
                    _selectedRecordingPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public NotificationStyle SelectedNotificationStyle
        {
            get => _selectedNotificationStyle;
            set
            {
                if (_selectedNotificationStyle != value)
                {
                    _selectedNotificationStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        public ScreenCaptureStyle SelectedScreenCaptureStyle
        {
            get => _selectedScreenCaptureStyle;
            set
            {
                if (_selectedScreenCaptureStyle != value)
                {
                    _selectedScreenCaptureStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
