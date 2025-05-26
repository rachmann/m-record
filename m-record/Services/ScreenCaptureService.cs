using m_record.Constants;
using m_record.Enums;
using m_record.Interfaces;
using m_record.Models;
using m_record.Properties;
using m_record.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace m_record.Services
{
    /// <summary>
    /// Service for capturing screenshots of the screen(s).
    /// </summary>
    public class ScreenCaptureService
    {
        public ScreenCaptureStyle screenCaptureStyle;
        private readonly IAppSettingsService _appSettingsService;
        private AppSettings Settings => _appSettingsService.Current;

        public ScreenCaptureService()
        {
            _appSettingsService = App.Services.GetRequiredService<IAppSettingsService>();
            if (_appSettingsService == null)
            {
                throw new InvalidOperationException("AppSettingsService not found in service provider.");
            }
            screenCaptureStyle = (ScreenCaptureStyle)Settings.ScreenCaptureStyle;
        }

        public List<string> CaptureAllScreens(DateTime dateNow, string directory)
        {
            ScreenCaptureStyle screenCaptureStyle = (ScreenCaptureStyle)Settings.ScreenCaptureStyle;
            if (screenCaptureStyle == ScreenCaptureStyle.None)
            {
                return new List<string>();
            }

            // New array of Screen[] with the same length as the number of screens
            var screens = new Screen?[Screen.AllScreens.Length];
            // based on the screenCaptureStyle, assign the screens to the array
            if (screenCaptureStyle == ScreenCaptureStyle.AllScreens)
            {
                screens = Screen.AllScreens;
            }
            else if (screenCaptureStyle == ScreenCaptureStyle.PrimaryScreen)
            {
                screens[0] = Screen.PrimaryScreen;
            }

            var filePaths = new List<string>();
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                if (screen == null)
                {
                    continue; // Skip if the screen is null
                }
                var bounds = screen.Bounds;
                using var bmp = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                }
                // The service will save the file and return the path
                string fileName = $"{AppConstants.ScreenCaptureFilePrefix}_{dateNow:yyyy}_{dateNow.DayOfYear:D3}_{i + 1}{AppConstants.ScreenCaptureFileSuffix}";
                string filePath = Path.Combine(directory, fileName);
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                filePaths.Add(filePath);
            }
            return filePaths;
        }
    }
}