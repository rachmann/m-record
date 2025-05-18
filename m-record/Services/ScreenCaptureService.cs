using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using m_record.Services;

namespace m_record.Services
{
    public class ScreenCaptureService
    {
        public List<string> CaptureAllScreens(DateTime dateNow, string directory, string filePrefix, string fileSuffix)
        {
            var screens = Screen.AllScreens;
            var filePaths = new List<string>();
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = screens[i];
                var bounds = screen.Bounds;
                using var bmp = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                }
                // The service will save the file and return the path
                string fileName = $"{filePrefix}_{dateNow:yyyy}_{dateNow.DayOfYear:D3}_{dateNow.Hour:D2}_{dateNow.Minute:D2}_{dateNow.Second:D2}_{i + 1}{fileSuffix}";
                string filePath = Path.Combine(directory, fileName);
                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                filePaths.Add(filePath);
            }
            return filePaths;
        }
    }
}