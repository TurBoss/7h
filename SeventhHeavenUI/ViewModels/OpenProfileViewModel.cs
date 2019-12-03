using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeaven.Windows;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
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
                Sys.Message(new WMessage($"Successfully deleted profile {name}"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage($"Failed to delete profile: {name}"));
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
                newProfileName = OpenProfileViewModel.InputNewProfileName();

                if (newProfileName == null)
                {
                    return; // user canceled inputting a profile name
                }

                File.Copy(pathToProfile, Path.Combine(Sys.PathToProfiles, $"{newProfileName}.xml"));
                Sys.Message(new WMessage($"Successfully copied profile {name} to the new profile {newProfileName}"));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Sys.Message(new WMessage($"Failed to copy profile {name} to {newProfileName}"));
            }

            ReloadProfiles();
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
                    MessageBox.Show("Profile Name is empty.", "Profile Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            } while (!isValid);

            return profileName;
        }

    }
}
