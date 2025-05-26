using m_record.Enums;
using m_record.Constants;
using System;
using System.Windows;
using m_record.Interfaces;
using m_record.Models;
using Microsoft.Extensions.DependencyInjection;

namespace m_record.Services
{
    /// <summary>
    /// Service for displaying notifications to the user.
    /// </summary>
    public class NotificationService
    {
        private Action<string> _showInArea;
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings Settings => _appSettingsService.Current;
        public NotificationService()
        {
            _appSettingsService = App.Services.GetRequiredService<IAppSettingsService>();

            // to default to a no-op
            _showInArea = message => { };
           
        }
     
        public void SetActionDelegates(Action<string> showInArea)
        {
            _showInArea = showInArea;
        }

        public void ShowNotification(string message, string reason, MessageBoxImage messageType)
        {
            if (_showInArea == null)
            {
                // log an error here 
                return;
            }
                

            NotificationStyle notifySetting;

            try
            {
                notifySetting = (NotificationStyle)Settings.NotifyStyle;
                if (!Enum.IsDefined(typeof(NotificationStyle), notifySetting))
                    notifySetting = NotificationStyle.None;
            }
            catch
            {
                notifySetting = NotificationStyle.None;
            }

            if (notifySetting == NotificationStyle.None)
                return;

            if (notifySetting == NotificationStyle.MessageOnly || notifySetting == NotificationStyle.Both)
            {
                MessageBox.Show(message, reason, MessageBoxButton.OK, messageType);
            }

            if (notifySetting == NotificationStyle.NotificationArea || notifySetting == NotificationStyle.Both)
            {
                string prefix = messageType switch
                {
                    MessageBoxImage.Error => AppConstants.ErrorPrefix,
                    MessageBoxImage.Information => AppConstants.InfoPrefix,
                    MessageBoxImage.Warning => AppConstants.WarningPrefix,
                    _ => String.Empty
                };
                _showInArea(prefix + message);
            }
        }

    }
}