using _7thHeaven.Code;
using Iros._7th;
using Iros._7th.Workshop;
using SeventhHeaven.Windows;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{


    public class ImportModViewModel : ViewModelBase
    {
        enum ImportTabIndex
        {
            FromIro,
            FromFolder,
            BatchImport
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _pathToIroArchiveInput;
        private string _pathToModFolderInput;
        private string _pathToBatchFolderInput;
        private string _helpText;
        private string _modNameInput;
        private int _selectedTabIndex;
        private bool _isImporting;
        private int _progressValue;

        public string PathToIroArchiveInput
        {
            get
            {
                return _pathToIroArchiveInput;
            }
            set
            {
                _pathToIroArchiveInput = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToModFolderInput
        {
            get
            {
                return _pathToModFolderInput;
            }
            set
            {
                _pathToModFolderInput = value;
                NotifyPropertyChanged();
            }
        }

        public string PathToBatchFolderInput
        {
            get
            {
                return _pathToBatchFolderInput;
            }
            set
            {
                _pathToBatchFolderInput = value;
                NotifyPropertyChanged();
            }
        }

        public string HelpText
        {
            get
            {
                return _helpText;
            }
            set
            {
                _helpText = value;
                NotifyPropertyChanged();
            }
        }

        public string ModNameInput
        {
            get
            {
                return _modNameInput;
            }
            set
            {
                _modNameInput = value;
                NotifyPropertyChanged();
            }
        }

        public bool ModNameInputIsEnabled
        {
            get
            {
                return (ImportTabIndex)SelectedTabIndex != ImportTabIndex.BatchImport;
            }
        }

        public bool IsImporting
        {
            get
            {
                return _isImporting;
            }
            set
            {
                _isImporting = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(IsNotImporting));
            }
        }

        public bool IsNotImporting
        {
            get
            {
                return !_isImporting;
            }
        }

        public int SelectedTabIndex
        {
            get
            {
                return _selectedTabIndex;
            }
            set
            {
                _selectedTabIndex = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ModNameInputIsEnabled));
                UpdateHelpText();
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
                NotifyPropertyChanged(nameof(ProgressBarVisibility));
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                if (ProgressValue == 0)
                    return Visibility.Hidden;

                return Visibility.Visible;
            }
        }

        public ImportModViewModel()
        {
            SelectedTabIndex = 0;
            ProgressValue = 0;
            ModNameInput = "";
            UpdateHelpText();
        }

        private void UpdateHelpText()
        {
            if ((ImportTabIndex)SelectedTabIndex == ImportTabIndex.FromIro)
            {
                HelpText = "Select a .iro archive file to import into the library folder.";
            }
            else if ((ImportTabIndex)SelectedTabIndex == ImportTabIndex.FromFolder)
            {
                HelpText = "Select a folder that contain mod files. The mod file(s) will be copied into the library folder";
            }
            else
            {
                HelpText = "Select a folder that contains .iro mod files and mod folders. All mod files found will be copied into the library folder.";
            }
        }


        public Task<bool> ImportModFromWindowAsync()
        {
            IsImporting = true;
            ProgressValue = 10;
            Sys.Message(new WMessage("Importing mod(s)... Please wait ..."));

            Task<bool> t = Task.Factory.StartNew(() =>
            {
                bool didImport = false;

                switch ((ImportTabIndex)SelectedTabIndex)
                {
                    case ImportTabIndex.FromIro:
                        didImport = TryImportFromIroArchive();
                        break;
                    case ImportTabIndex.FromFolder:
                        didImport = TryImportFromFolder();
                        break;
                    case ImportTabIndex.BatchImport:
                        didImport = TryBatchImport();
                        break;
                }

                IsImporting = false;
                return didImport;
            });

            return t;
        }

        private bool TryImportFromIroArchive()
        {
            if (string.IsNullOrWhiteSpace(PathToIroArchiveInput))
            {
                MessageDialogWindow.Show("Enter a path to a .iro archive file", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ModNameInput))
            {
                MessageDialogWindow.Show("Enter a name for the mod.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!File.Exists(PathToIroArchiveInput))
            {
                MessageDialogWindow.Show(".iro archive file does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ModImporter importer = null;
            try
            {
                importer = new ModImporter();
                importer.ImportProgressChanged += Importer_ImportProgressChanged;
                importer.Import(PathToIroArchiveInput, ModNameInput, true, false);

                Sys.Message(new WMessage($"Successfully imported {ModNameInput}!"));
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageDialogWindow.Show("Failed to import mod. The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                importer.ImportProgressChanged -= Importer_ImportProgressChanged;
            }

        }

        private void Importer_ImportProgressChanged(string message, double percentComplete)
        {
            HelpText = message;
            ProgressValue = (int)percentComplete;
            App.ForceUpdateUI();
        }

        private bool TryImportFromFolder()
        {
            if (string.IsNullOrWhiteSpace(PathToModFolderInput))
            {
                MessageDialogWindow.Show("Enter a path to a folder containing mod files.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ModNameInput))
            {
                MessageDialogWindow.Show("Enter a name for the mod", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Directory.Exists(PathToModFolderInput))
            {
                MessageDialogWindow.Show("Directory does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ModImporter importer = null;
            try
            {
                importer = new ModImporter();
                importer.ImportProgressChanged += Importer_ImportProgressChanged;
                importer.Import(PathToModFolderInput, ModNameInput, false, false);

                Sys.Message(new WMessage($"Successfully imported {ModNameInput}!", true));
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageDialogWindow.Show("Failed to import mod. The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                importer.ImportProgressChanged -= Importer_ImportProgressChanged;
            }
        }

        private bool TryBatchImport()
        {
            if (string.IsNullOrWhiteSpace(PathToBatchFolderInput))
            {
                MessageDialogWindow.Show("Enter a path to a folder containing .iro mod files and/or mod folders", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Directory.Exists(PathToBatchFolderInput))
            {
                MessageDialogWindow.Show("Directory does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ModImporter importer = null;

            try
            {
                importer = new ModImporter();
                importer.ImportProgressChanged += Importer_ImportProgressChanged;

                int modImportCount = 0;

                foreach (string iro in Directory.GetFiles(PathToBatchFolderInput, "*.iro"))
                {
                    string modName = ModImporter.ParseNameFromFileOrFolder(Path.GetFileNameWithoutExtension(iro));
                    importer.Import(iro, modName, true, false);
                    modImportCount++;
                }

                foreach (string dir in Directory.GetDirectories(PathToBatchFolderInput))
                {
                    string modName = ModImporter.ParseNameFromFileOrFolder(Path.GetFileNameWithoutExtension(dir));
                    importer.Import(dir, modName, false, false);
                    modImportCount++;
                }

                Sys.Message(new WMessage($"Successfully imported {modImportCount} mod(s)!", true));
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                MessageDialogWindow.Show("Failed to import mod(s). The error has been logged", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                importer.ImportProgressChanged -= Importer_ImportProgressChanged;
            }
        }
    }
}
