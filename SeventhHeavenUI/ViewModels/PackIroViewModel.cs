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
    public class PackIroViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _pathToSourceFolder;
        private string _pathToOutputFile;
        private List<string> _compressionOptions;
        private bool _isPacking;
        private int _progressValue;
        private int _compressionSelectedIndex;
        private string _statusText;

        public string PathToSourceFolder
        {
            get
            {
                return _pathToSourceFolder;
            }
            set
            {
                _pathToSourceFolder = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToOutputFile
        {
            get
            {
                return _pathToOutputFile;
            }
            set
            {
                _pathToOutputFile = value;
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

        public string SelectedCompressionOptionText
        {
            get
            {
                return ((_7thWrapperLib.CompressType)CompressionSelectedIndex).ToString();
            }
        }
        public List<string> CompressionOptions
        {
            get
            {
                if (_compressionOptions == null)
                    _compressionOptions = new List<string>();

                return _compressionOptions;
            }
            set
            {
                _compressionOptions = value;
                NotifyPropertyChanged();
            }
        }

        public int CompressionSelectedIndex
        {
            get
            {
                return _compressionSelectedIndex;
            }
            set
            {
                _compressionSelectedIndex = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SelectedCompressionOptionText));
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

        public bool IsPacking
        {
            get
            {
                return _isPacking;
            }
            set
            {
                _isPacking = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotPacking));
            }
        }

        public bool IsNotPacking
        {
            get
            {
                return !_isPacking;
            }
        }


        public PackIroViewModel()
        {
            ProgressValue = 0;
            IsPacking = false;
            PathToSourceFolder = "";
            PathToOutputFile = "";
            StatusText = "";
            CompressionOptions = Enum.GetNames(typeof(_7thWrapperLib.CompressType)).ToList();
            CompressionSelectedIndex = 0;
        }

        internal bool Validate(bool showErrorMsg = true)
        {
            string errorMsg = "";
            bool isValid = true;

            if (string.IsNullOrEmpty(PathToSourceFolder))
            {
                errorMsg = "Path to source folder is required";
                isValid = false;
            }
            else if (string.IsNullOrEmpty(PathToOutputFile))
            {
                errorMsg = "Path to output iro file is required";
                isValid = false;
            }
            else if (CompressionSelectedIndex < 0)
            {
                errorMsg = "Select a compression option";
                isValid = false;
            }

            if (!isValid && showErrorMsg)
            {
                Logger.Warn($"invalid pack iro options: {errorMsg}");
                MessageDialogWindow.Show(errorMsg, "Missing Required Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        internal void PackIro()
        {
            string pathToSource = PathToSourceFolder;
            string outputFile = PathToOutputFile;
            _7thWrapperLib.CompressType compressType = (_7thWrapperLib.CompressType)CompressionSelectedIndex;

            IsPacking = true;

            Task packTask = Task.Factory.StartNew(() =>
            {

                var files = Directory.GetFiles(pathToSource, "*", SearchOption.AllDirectories)
                                     .Select(s => s.Substring(pathToSource.Length).Trim('\\', '/'))
                                     .ToList();

                using (var fs = new FileStream(outputFile, FileMode.Create))
                    _7thWrapperLib.IrosArc.Create(fs, files.Select(s => _7thWrapperLib.IrosArc.ArchiveCreateEntry.FromDisk(pathToSource, s)), _7thWrapperLib.ArchiveFlags.None, compressType, IroProgress);
            });

            packTask.ContinueWith((result) =>
            {
                IsPacking = false;
                ProgressValue = 0;

                if (result.IsFaulted)
                {
                    Logger.Warn(result.Exception.GetBaseException());
                    StatusText = $"An error occured while packing: {result.Exception.GetBaseException().Message}";
                    return;
                }

                StatusText = "Packing complete!";
            });
        }

        private void IroProgress(double d, string s)
        {
            StatusText = s;
            ProgressValue = (int)(100 * d);
        }
    }
}
