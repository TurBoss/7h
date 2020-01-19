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

                didLaunch = GameLauncher.LaunchGame(variableDump, debugLogging, IsLaunchingWithNoMods);
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

            App.ForceUpdateUI();
        }
    }
}
