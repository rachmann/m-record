using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m_record.Constants;

namespace m_record.Services
{
    public class LoggingService
    {
        private string? _currentLogFilePath;

        public string GetRecordPath()
        {
            var path = Properties.Settings.Default.RecordPath;
            if (string.IsNullOrWhiteSpace(path))
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public string GetCurrentLogFilePath()
        {
            var dir = GetRecordPath();
            var fileName = string.Format(Constants.Constants.LogFileNameTemplate, DateTime.Now, DateTime.Now.DayOfYear);
            var path = Path.Combine(dir, fileName);

            if (_currentLogFilePath != path)
            {
                _currentLogFilePath = path;
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, Constants.Constants.LogFileHeader + Environment.NewLine);
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