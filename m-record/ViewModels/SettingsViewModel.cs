using m_record.Enums;
using m_record.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace m_record.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isDarkMode;
        private string _selectedRecordingPath = string.Empty;
        private NotificationStyle _selectedNotificationStyle;
        private ScreenCaptureStyle _selectedScreenCaptureStyle;

        public ObservableCollection<KeyValuePair<Enum, string>> ScreenCaptureOptions { get; }
        public ObservableCollection<KeyValuePair<Enum, string>> NotifyOptions { get; }

        public SettingsViewModel()
        {
            ScreenCaptureOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<ScreenCaptureStyle>());
            NotifyOptions = new ObservableCollection<KeyValuePair<Enum, string>>(EnumExtensions.GetEnumDescriptions<NotificationStyle>());
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

        public void LoadSettings(bool isDarkMode, string recordingPath, NotificationStyle notificationStyle, ScreenCaptureStyle screenCaptureStyle)
        {
            IsDarkMode = isDarkMode;
            SelectedRecordingPath = recordingPath;
            SelectedNotificationStyle = notificationStyle;
            SelectedScreenCaptureStyle = screenCaptureStyle;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
