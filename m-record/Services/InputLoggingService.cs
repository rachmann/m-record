using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m_record.Constants;
using m_record.Interfaces;
using m_record.Models;
using m_record.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace m_record.Services
{
    /// <summary>
    /// Logging service for input data.
    /// </summary>
    public class InputLoggingService
    {
        private string? _currentLogFilePath;
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings Settings => _appSettingsService.Current;
        public InputLoggingService()
        {
            _appSettingsService =App.Services.GetRequiredService<IAppSettingsService>();
            if (_appSettingsService == null)
            {
                throw new InvalidOperationException("AppSettingsService not found in service provider.");
            }
            _currentLogFilePath = Settings.RecordPath;
        }

        public string GetRecordPath()
        {
            var path = Settings.RecordPath;
            if (string.IsNullOrWhiteSpace(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public string GetCurrentLogFilePath()
        {
            var dir = Settings.RecordPath;
            var fileName = string.Format(AppConstants.LogFileNameTemplate, DateTime.Now, DateTime.Now.DayOfYear);
            var path = Path.Combine(dir, fileName);

            if (_currentLogFilePath != path)
            {
                _currentLogFilePath = path;
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, AppConstants.LogFileHeader + Environment.NewLine);
                }
            }
            return path;
        }

        public void AppendLog(string csvRow)
        {
            var logFilePath = GetCurrentLogFilePath();
            File.AppendAllText(logFilePath, csvRow + Environment.NewLine);
        }
    }
}