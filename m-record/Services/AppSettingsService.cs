using m_record.Interfaces;
using m_record.Models;
using Microsoft.Extensions.Options;

namespace m_record.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        private readonly AppSettings _settings;

        public AppSettingsService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
            // Optionally, sync with Settings on startup
            LoadFromProperties();
        }

        public AppSettings Current => _settings;

        public void Save()
        {
            Properties.Settings.Default.IsDarkMode = _settings.IsDarkMode;
            Properties.Settings.Default.NotifyStyle = _settings.NotifyStyle;
            Properties.Settings.Default.ScreenCaptureStyle = _settings.ScreenCaptureStyle;
            Properties.Settings.Default.RecordPath = _settings.RecordPath;
            Properties.Settings.Default.Save();
        }

        public void Reload()
        {
            Properties.Settings.Default.Reload();
            LoadFromProperties();
        }

        public void Update(Action<AppSettings> updateAction)
        {
            updateAction(_settings);
            Save();
        }

        private void LoadFromProperties()
        {
            _settings.IsDarkMode = Properties.Settings.Default.IsDarkMode;
            _settings.NotifyStyle = Properties.Settings.Default.NotifyStyle;
            _settings.ScreenCaptureStyle = Properties.Settings.Default.ScreenCaptureStyle;
            _settings.RecordPath = Properties.Settings.Default.RecordPath ?? string.Empty;
        }
    }
}
