using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m_record.Constants
{
    public static class AppConstants
    {
        public const string AppName = "m_record";

        // Log file
        public const string LogContentsFormat = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}";
        public const string LogFileHeader = "timestamp,Type,IsCtrl,IsAlt,IsShift,IsWin,KeyCode,Application,ApplicationPath,ParentWindow,Window,FilePath,MouseLocationX,MouseLocationY";
        public const string LogTimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";
        public const string LogFileNameTemplate = "mrecord_{0:yyyy}_{1:D3}.csv";
        public const string LogLetterFalse = "F";
        public const string LogLetterTrue = "T";
        public const string LogTypeKeyBoard = "KEYSTROKE";
        public const string LogTypeMouse = "MOUSECLICK";
        public const string LogTypeScreenCap = "SCREENCAP";

        // Notification messages
        public const string ErrorFailedToCreateDir = "Failed to create directory:";
        public const string ErrorNotificationStyleInvalid = "Invalid notification style in settings:";
        public const string KeystrokeLogSavedMessage = "Keystroke log file updated as events occurred.";
        public const string KeystrokeLogSavedTitle = "Saved Keystroke Log";
        public const string NoScreensDetectedMessage = "No screens detected.";
        public const string ErrorTitle = "Error";
        public const string ScreenCaptureSavedMessage = "Screen capture saved";
        public const string ScreenCaptureSavedTitle = "Saved Screen Shot";

        public const string InfoNotificationStyleSetToNone = "Notification style is set to None.";
        public const string InfoScreenCapStyleSetToNone = "Screen capture style is set to None.";
        public const string InfoRecordPathIsDefault = "Recording path is empty; setting to default:";
        public const string InfoCreatedReportDirectory = "Created report directory:";
        public const string InfoRecordingStarted = "Recording started.";
        public const string InfoRecordingStopped = "Recording stopped.";

        public const string ScreenCaptureFilePrefix = "screensave";
        public const string ScreenCaptureFileSuffix = ".png";

        // Notification prefixes
        public const string ErrorPrefix = "Error: ";
        public const string InfoPrefix = "Info: ";
        public const string WarningPrefix = "Warning: ";
        public const string AlertPrefix = "Alert: ";

        //Timer
        public const string TimerInitialText = "00:00:00";

    }
}
