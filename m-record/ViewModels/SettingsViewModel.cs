using m_record.Constants;
using m_record.Enums;
using m_record.Extensions;
using m_record.Interfaces;
using m_record.Models;
using m_record.Services;
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
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings Settings => _appSettingsService.Current;

        public ObservableCollection<KeyValuePair<Enum, string>> ScreenCaptureOptions { get; }
        public ObservableCollection<KeyValuePair<Enum, string>> NotifyOptions { get; }

        public SettingsViewModel()
        {
            _appSettingsService = App.Services.GetRequiredService<IAppSettingsService>();
            if (_appSettingsService == null)
            {
                throw new InvalidOperationException("AppSettingsService not found in service provider.");
            }
            _logger = App.Services.GetService<ILogger<SettingsViewModel>>();
            if (_logger == null)
            {
                throw new InvalidOperationException("Logger not found in service provider.");
            }
            ScreenCaptureOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<ScreenCaptureStyle>());
            NotifyOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<NotificationStyle>());


            // Read from Settings
            var notifySetting = (NotificationStyle)Settings.NotifyStyle;
            if (!Enum.IsDefined(notifySetting))
            {
                _logger.LogError($"{AppConstants.ErrorNotificationStyleInvalid} {Settings.NotifyStyle}");
                notifySetting = NotificationStyle.None;
            }

            if (notifySetting == NotificationStyle.None)
            {
                _logger.LogInformation(AppConstants.InfoNotificationStyleSetToNone);
            }

            var screenCaptureSetting = (ScreenCaptureStyle)Settings.ScreenCaptureStyle;
            if (!Enum.IsDefined(screenCaptureSetting))
            {
                _logger.LogError($"{AppConstants.ErrorNotificationStyleInvalid} {Settings.ScreenCaptureStyle}");
                screenCaptureSetting = ScreenCaptureStyle.None;
            }

            if (screenCaptureSetting == ScreenCaptureStyle.None)
            {
                _logger.LogInformation(AppConstants.InfoScreenCapStyleSetToNone);
            }

            var recordingPath = Settings.RecordPath ??
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

          
            SelectedNotificationStyle = notifySetting;
            SelectedRecordingPath = recordingPath;
            SelectedScreenCaptureStyle = screenCaptureSetting;
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

        public string SelectedRecordingPath
        {
            get => Settings.RecordPath;
            set
            {
                if (Settings.RecordPath != value)
                {
                    _appSettingsService.Update(s => s.RecordPath = value);
                    OnPropertyChanged();
                }
            }
        }

        public NotificationStyle SelectedNotificationStyle
        {
            get => (NotificationStyle)Settings.NotifyStyle;
            set
            {
                if ((NotificationStyle)Settings.NotifyStyle != value)
                {
                    _appSettingsService.Update(s => s.NotifyStyle = (int)value);
                    OnPropertyChanged();
                }
            }
        }

        public ScreenCaptureStyle SelectedScreenCaptureStyle
        {
            get => (ScreenCaptureStyle)Settings.ScreenCaptureStyle;
            set
            {
                if ((ScreenCaptureStyle)Settings.ScreenCaptureStyle != value)
                {
                    _appSettingsService.Update(s => s.ScreenCaptureStyle = (int)value);
                    OnPropertyChanged();
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
