using Gma.System.MouseKeyHook;
using m_record.Constants;
using m_record.Dialogs;
using m_record.Enums;
using m_record.Helpers;
using m_record.Services;
using m_record.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MouseEventArgs = System.Windows.Input.MouseEventArgs; // Add this at the top if not already present
using Path = System.IO.Path;

namespace m_record
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel MainViewModel { get; }

        private bool isDarkMode = false;

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel = App.Services.GetRequiredService<MainViewModel>();
            DataContext = MainViewModel;
            isDarkMode = Properties.Settings.Default.IsDarkMode;

            ApplyTheme();

            // Subscribe to ViewModel events
            MainViewModel.RequestClose += () => this.Close();
            MainViewModel.RequestCloseForm += () => this.Close();
            MainViewModel.RequestOpenSettings += OpenSettingsDialog;
            MainViewModel.RequestOpenHelp += OpenHelpDialog;
            MainViewModel.RequestOpenAbout += OpenAboutDialog;
        }

        // Move dialog logic to private methods:
        private void OpenSettingsDialog()
        {
            var dlg = new SettingsDialog() { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                isDarkMode = dlg.IsDarkMode;
                Properties.Settings.Default.IsDarkMode = isDarkMode;
                Properties.Settings.Default.NotifyStyle = (int)dlg.SelectedNotificationStyle;
                Properties.Settings.Default.RecordPath = dlg.SelectedRecordingPath;
                Properties.Settings.Default.ScreenCaptureStyle = (int)dlg.SelectedScreenCaptureStyle;
                Properties.Settings.Default.Save();
                ApplyTheme();
            }
        }

        private void OpenHelpDialog()
        {
            var dlg = new HelpDialog { Owner = this };
            dlg.ShowDialog();
        }

        private void OpenAboutDialog()
        {
            var dlg = new AboutDialog { Owner = this };
            dlg.ShowDialog();
        }

        private void ApplyTheme()
        {
            // Window and border
            this.Background = isDarkMode ? Brushes.Black : Brushes.White;
            if (this.Content is Border border)
            {
                border.Background = isDarkMode ? Brushes.Black : Brushes.White;
                border.BorderBrush = isDarkMode ? Brushes.Gray : new SolidColorBrush(Color.FromRgb(136, 136, 136));
            }
            // Menu items:
            // Menu and Close icons
            ContextMenuButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;
            CloseButton.Style = isDarkMode ? (Style)FindResource("DarkModeTopButtonStyle") : null;

            if (isDarkMode)
            {
                // Clear any local Foreground value so the style can take effect
                MenuIcon.ClearValue(ForegroundProperty);
                CloseIcon.ClearValue(ForegroundProperty);

                MenuIcon.Style = (Style)FindResource("MenuIconDarkModeStyle");
                CloseIcon.Style = (Style)FindResource("CloseIconDarkModeStyle");
            }
            else
            {
                MenuIcon.Style = null;
                MenuIcon.Foreground = Brushes.Black;
                CloseIcon.Style = null;
                CloseIcon.Foreground = Brushes.Black;
            }

            // Context menu background, text, and icon colors
            if (ContextMenuButton.ContextMenu is ContextMenu menu)
            {
                menu.Background = isDarkMode ? Brushes.Black : Brushes.White;
                menu.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
                foreach (var item in menu.Items)
                {
                    if (item is MenuItem mi)
                    {
                        mi.Background = isDarkMode ? Brushes.Black : Brushes.White;
                        mi.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
                        mi.Style = isDarkMode ? (Style)FindResource("DarkModeMenuItemStyle") : null;

                        // Set icon foreground for each menu item
                        if (mi.Icon is MahApps.Metro.IconPacks.PackIconMaterial icon)
                        {
                            icon.Foreground = isDarkMode ? Brushes.White : Brushes.Black;
                        }
                    }
                }
            }

            // Main text
            TimerText.Foreground = isDarkMode ? Brushes.White : Brushes.Black;

            // Play/Stop icon
            PlayStopButton.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;
            // Play/Stop style
            PlayStopButton.Style = isDarkMode ? (Style)FindResource("DarkModePlayStopButtonStyle") : null;
            // Status icon: keep red/green, but set background if needed
            StatusIcon.Background = isDarkMode ? Brushes.Black : Brushes.Transparent;

        }

    }
}