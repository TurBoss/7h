using SeventhHeaven.Classes;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SeventhHeaven.ViewModels
{
    public class GameLaunchViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _statusLog;
        private bool variableDump;
        private bool debugLogging;

        public GameLaunchViewModel(bool variableDump, bool debugLogging)
        {
            this.variableDump = variableDump;
            this.debugLogging = debugLogging;
            StatusLog = "";
        }

        public string StatusLog
        {
            get
            {
                return _statusLog;
            }
            set
            {
                _statusLog = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsLaunchingWithNoMods { get; internal set; }

        internal async Task<bool> BeginLaunchProcessAsync()
        {
            bool didLaunch = false;

            try
            {
                GameLauncher.Instance.ProgressChanged += LaunchGame_ProgressChanged;

                if (IsLaunchingWithNoMods)
                {
                    LogStatus("Launching 'vanilla' FF7 - with no mods ...");
                    GameLauncher.Instance.LaunchAdditionalProgramsToRunPrior();
                    didLaunch = GameLauncher.LaunchFF7Exe();
                }
                else
                {
                    didLaunch = GameLauncher.LaunchGame(variableDump, debugLogging);
                }
            }
            finally
            {
                GameLauncher.Instance.ProgressChanged -= LaunchGame_ProgressChanged;
            }

            return didLaunch;
        }

        internal void LogStatus(string message)
        {
            Logger.Info(message);
            AppendToStatusLog(message);
        }

        private void LaunchGame_ProgressChanged(string message)
        {
            AppendToStatusLog(message);
        }

        internal void AppendToStatusLog(string message)
        {
            // invoke on UI thread to show message asap to user
            App.Current.Dispatcher.Invoke(() =>
            {
                StatusLog += $"{message}\n";
            }, DispatcherPriority.Background);

            ForceUpdateUI();
        }

        public static void ForceUpdateUI()
        {
            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }
    }
}
