using Iros._7th;
using Iros._7th.Workshop;
using Iros.Mega;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace SeventhHeavenUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
            this.DataContext = ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitViewModel();

            if (Sys.Settings.MainWindow != null)
            {
                var loc = new System.Drawing.Point((int)Sys.Settings.MainWindow.X, (int)Sys.Settings.MainWindow.Y);

                if (Screen.AllScreens.Any(s => s.Bounds.Contains(loc)))
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    System.Windows.Application.Current.MainWindow.Left = Sys.Settings.MainWindow.X;
                    System.Windows.Application.Current.MainWindow.Top = Sys.Settings.MainWindow.Y;
                }

                Width = Sys.Settings.MainWindow.W;
                Height = Sys.Settings.MainWindow.H;
                WindowState = Sys.Settings.MainWindow.State;
            }

            ProcessCommandLineArgs();
        }

        private void ProcessCommandLineArgs()
        {
            foreach (string parm in Environment.GetCommandLineArgs())
            {
                if (parm.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                {
                    //TODO: ProcessIrosLink(parm);
                }
                else if (parm.StartsWith("/OPENIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string irofile = parm.Substring(9);
                    string irofilenoext = System.IO.Path.GetFileNameWithoutExtension(irofile);
                    Log.Write("Importing IRO from Windows " + irofile);
                    try
                    {
                        //TODO: fImportMod.ImportMod(irofile, irofilenoext, true, false);
                    }
                    catch (Exception ex)
                    {
                        Sys.Message(new WMessage() { Text = "Mod " + irofilenoext + " failed to import: " + ex.ToString() });
                        continue;
                    }
                    Sys.Message(new WMessage() { Text = "Auto imported mod " + irofilenoext });
                    System.Windows.MessageBox.Show("The mod " + irofilenoext + " has been added to your Library.", "Import Mod from Windows");
                    //TODO: Add an IRO "Unpack" option here
                }
                else if (parm.StartsWith("/PROFILE:", StringComparison.InvariantCultureIgnoreCase))
                {
                    Sys.Settings.CurrentProfile = parm.Substring(9);
                    Sys.ActiveProfile = Util.Deserialize<Profile>(Sys.ProfileFile);
                    ViewModel.RefreshProfile();
                }
                else if (parm.Equals("/LAUNCH", StringComparison.InvariantCultureIgnoreCase))
                {
                    //TODO: bLaunch.PerformClick();
                }
                else if (parm.Equals("/QUIT", StringComparison.InvariantCultureIgnoreCase))
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Sys.Settings.MainWindow = new SavedWindow()
            {
                X = (int) System.Windows.Application.Current.MainWindow.Left,
                Y = (int) System.Windows.Application.Current.MainWindow.Top,
                W = (int) ActualWidth,
                H = (int) ActualHeight,
                State = WindowState
            };

            ViewModel.SaveProfile();
            Sys.Save();
        }
    }
}
