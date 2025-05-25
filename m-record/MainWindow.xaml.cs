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


        public MainWindow()
        {

            InitializeComponent();
            var vm = App.Services.GetRequiredService<MainViewModel>();
            DataContext = vm;
            vm.RequestClose += () => this.Close();
        }
    }
}