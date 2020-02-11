using SeventhHeaven.Windows;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{
    public class UnpackIroViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _pathToOutputFolder;
        private string _pathToIroFile;
        private bool _isUnpacking;
        private int _progressValue;
        private string _statusText;

        public string PathToOutputFolder
        {
            get
            {
                return _pathToOutputFolder;
            }
            set
            {
                _pathToOutputFolder = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToIroFile
        {
            get
            {
                return _pathToIroFile;
            }
            set
            {
                _pathToIroFile = value;
                NotifyPropertyChanged();
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                NotifyPropertyChanged();
            }
        }

        public int ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsUnpacking
        {
            get
            {
                return _isUnpacking;
            }
            set
            {
                _isUnpacking = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotUnpacking));
            }
        }

        public bool IsNotUnpacking
        {
            get
            {
                return !_isUnpacking;
            }
        }


        public UnpackIroViewModel()
        {
            ProgressValue = 0;
            IsUnpacking = false;
            PathToOutputFolder = "";
            PathToIroFile = "";
            StatusText = "";
        }

        internal bool Validate(bool showErrorMsg = true)
        {
            string errorMsg = "";
            bool isValid = true;

            if (string.IsNullOrEmpty(PathToOutputFolder))
            {
                errorMsg = "Path to output folder is required";
                isValid = false;
            }
            else if (string.IsNullOrEmpty(PathToIroFile))
            {
                errorMsg = "Path to source iro file is required";
                isValid = false;
            }

            if (!isValid && showErrorMsg)
            {
                Logger.Warn($"invalid unpack iro options: {errorMsg}");
                MessageDialogWindow.Show(errorMsg, "Missing Required Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        internal Task UnpackIro()
        {
            string pathToOutput = PathToOutputFolder;
            string pathToIro = PathToIroFile;

            IsUnpacking = true;

            Task unpackTask = Task.Factory.StartNew(() =>
            {
                using (_7thWrapperLib.IrosArc arc = new _7thWrapperLib.IrosArc(pathToIro))
                {
                    List<string> files = arc.AllFileNames().ToList();
                    int count = 0;
                    foreach (string file in files)
                    {
                        string path = Path.Combine(pathToOutput, file);

                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.WriteAllBytes(path, arc.GetBytes(file));
                        
                        count++;
                        IroProgress(1.0 * count / files.Count, file);
                    }
                }
            });

            unpackTask.ContinueWith((result) =>
            {
                IsUnpacking = false;
                ProgressValue = 0;

                if (result.IsFaulted)
                {
                    Logger.Warn(result.Exception.GetBaseException());
                    StatusText = $"An error occured while unpacking: {result.Exception.GetBaseException().Message}";
                    return;
                }

                StatusText = "Unpacking complete!";
            });

            return unpackTask;
        }

        private void IroProgress(double d, string s)
        {
            StatusText = s;
            ProgressValue = (int)(100 * d);
        }
    }
}
