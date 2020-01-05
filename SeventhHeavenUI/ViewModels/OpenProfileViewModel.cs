using Iros._7th;
using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{
    public class OpenProfileViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        private List<string> _profiles;
        private string _selectedProfile;

        public List<string> Profiles
        {
            get
            {
                return _profiles;
            }
            set
            {
                _profiles = value;
                SelectedProfile = null;
                NotifyPropertyChanged();
            }
        }

        public bool OkButtonIsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedProfile);
            }
        }

        public string SelectedProfile
        {
            get
            {
                return _selectedProfile;
            }
            set
            {
                _selectedProfile = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(OkButtonIsEnabled));
            }
        }


        public OpenProfileViewModel()
        {
            ReloadProfiles();
        }

        public void ReloadProfiles()
        {
            List<string> profiles = new List<string>();

            foreach (string file in Directory.GetFiles(Sys.PathToProfiles, "*.xml"))
            {
                profiles.Add(Path.GetFileNameWithoutExtension(file));
            }

            Profiles = profiles;
        }

        /// <summary>
        /// Opens a window to input a new profile name.
        /// if new profile is created, then current profile is saved before switching to new profile.
        /// </summary>
        internal void CreateNewProfile()
        {
            string profileName = InputNewProfileName();

            if (profileName == null)
            {
                return; // user canceled inputting a profile name
            }

            try
            {
                MainWindowViewModel.SaveProfile(); // save current profile

                // create new profile and save
                Sys.ActiveProfile = new Profile();
                Sys.Settings.CurrentProfile = profileName;
                MainWindowViewModel.SaveProfile();


                ReloadProfiles();
                SelectedProfile = profileName;

                Sys.Message(new WMessage($"Successfully created new profile {profileName}!", true));
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Failed to create new profile {profileName}: {e.Message}", true));
            }
        }

        public void DeleteProfile(string name)
        {
            string pathToProfile = Path.Combine(Sys.PathToProfiles, $"{name}.xml");

            if (!File.Exists(pathToProfile))
            {
                Logger.Warn($"profile does not exist: {pathToProfile}");
                return;
            }

            try
            {
                File.Delete(pathToProfile);
                Sys.Message(new WMessage($"Successfully deleted profile {name}", true));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage($"Failed to delete profile: {name}", true));
            }

            ReloadProfiles();
        }

        public void CopyProfile(string name)
        {
            string pathToProfile = Path.Combine(Sys.PathToProfiles, $"{name}.xml");

            if (!File.Exists(pathToProfile))
            {
                Logger.Warn($"profile does not exist: {pathToProfile}");
                return;
            }

            string newProfileName = null;

            try
            {
                newProfileName = InputNewProfileName();

                if (newProfileName == null)
                {
                    return; // user canceled inputting a profile name
                }

                File.Copy(pathToProfile, Path.Combine(Sys.PathToProfiles, $"{newProfileName}.xml"));
                Sys.Message(new WMessage($"Successfully copied profile {name} to the new profile {newProfileName}", true));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage($"Failed to copy profile {name} to {newProfileName}", true));
            }

            ReloadProfiles();
        }

        public void ViewProfileDetails(string name)
        {
            string pathToProfile = Path.Combine(Sys.PathToProfiles, $"{name}.xml");

            if (!File.Exists(pathToProfile))
            {
                Logger.Warn($"profile does not exist: {pathToProfile}");
                return;
            }

            try
            {
                Profile profileToView = Util.Deserialize<Profile>(pathToProfile);

                string tempFolder = Path.Combine(Sys.PathToTempFolder, "profile_details");
                string tempFile = Path.Combine(tempFolder, $"{Path.GetRandomFileName()}.txt");

                Directory.CreateDirectory(tempFolder);
                File.WriteAllLines(tempFile, profileToView.GetDetails());

                Process.Start("notepad.exe", tempFile);
            }
            catch (Exception e)
            {
                Sys.Message(new WMessage($"Failed to open profile details: {e.Message}", true));
            }
        }

        public static string InputNewProfileName()
        {
            string prompt = "Enter new Profile name:";
            string profileName = null;
            bool isValid = false;

            do
            {
                isValid = true;
                InputTextWindow inputBox = new InputTextWindow("New Profile", prompt)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                bool? dialogResult = inputBox.ShowDialog();

                if (!dialogResult.GetValueOrDefault(false))
                {
                    return null;
                }

                profileName = inputBox.ViewModel.TextInput;

                if (string.IsNullOrEmpty(profileName))
                {
                    isValid = false;
                    MessageDialogWindow.Show("Profile Name is empty.", "Profile Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            } while (!isValid);

            return profileName;
        }

        /// <summary>
        /// Attempts to delete any temp files that were created for viewing profile details
        /// </summary>
        public void AttemptDeleteTempFiles()
        {
            string tempFolder = Path.Combine(Sys.PathToTempFolder, "profile_details");

            if (!Directory.Exists(tempFolder))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(tempFolder))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    // ignore exception here as it could be from the file still being opened
                    Logger.Warn($"Could not delete temp file {file} - {e.Message}");
                }
            }
        }

    }
}
