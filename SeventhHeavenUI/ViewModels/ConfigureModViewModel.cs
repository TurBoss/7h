using _7thWrapperLib;
using Iros._7th.Workshop;
using SeventhHeaven.Classes;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SeventhHeaven.ViewModels
{
    public class ConfigureModViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private NAudio.Wave.IWavePlayer _audio;
        private string _iroPath;
        private ModInfo _info;
        private Func<string, string> _imageReader;
        private Func<string, Stream> _audioReader;
        private List<Constraint> _constraints;
        
        /// <summary>
        /// Dictionary mapping mod settings to their values
        /// </summary>
        private Dictionary<string, int> _values;


        private string _windowTitle;
        private string _description;
        private string _compatibilityNote;
        private string _checkBoxText;
        private bool _isOptionChecked;
        private int _selectedOptionIndex;
        private int _dropdownSelectedIndex;
        private Uri _imageOptionSource;
        private List<ConfigOptionViewModel> _modOptions;
        private List<OptionValueViewModel> _dropdownOptions;
        private ConfigOption _selectedOption;

        private Visibility _checkBoxVisibility;
        private Visibility _comboBoxVisibility;
        private Visibility _previewButtonVisibility;

        private bool _checkBoxIsEnabled;
        private bool _comboBoxIsEnabled;
        private string _audioFileName;
        private string _audioPath;
        private Stream _audioFile;

        private NAudio.Vorbis.VorbisWaveReader _waveReader;
        private NAudio.Wave.Mp3FileReader _mp3Reader;

        public string WindowTitle
        {
            get
            {
                return _windowTitle;
            }
            set
            {
                _windowTitle = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsPlayingAudio
        {
            get
            {
                return _audio != null;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged();
            }
        }

        public string CheckBoxText
        {
            get
            {
                return _checkBoxText;
            }
            set
            {
                _checkBoxText = value;
                NotifyPropertyChanged();
            }
        }

        public string CompatibilityNote
        {
            get
            {
                return _compatibilityNote;
            }
            set
            {
                _compatibilityNote = value;
                NotifyPropertyChanged();
            }
        }

        public string SelectedDropdownText
        {
            get
            {
                if (DropdownSelectedIndex < 0 || DropdownSelectedIndex >= DropdownOptions.Count)
                {
                    return "";
                }

                return DropdownOptions[DropdownSelectedIndex].ValueName;
            }
        }

        public bool IsOptionChecked
        {
            get
            {
                return _isOptionChecked;
            }
            set
            {
                _isOptionChecked = value;
                NotifyPropertyChanged();
                UpdateViewForCheckChanged();
            }
        }

        public bool CheckBoxIsEnabled
        {
            get
            {
                return _checkBoxIsEnabled;
            }
            set
            {
                _checkBoxIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public bool ComboBoxIsEnabled
        {
            get
            {
                return _comboBoxIsEnabled;
            }
            set
            {
                _comboBoxIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public List<ConfigOptionViewModel> ModOptions
        {
            get
            {
                if (_modOptions == null)
                    _modOptions = new List<ConfigOptionViewModel>();

                return _modOptions;
            }
            set
            {
                _modOptions = value;
                NotifyPropertyChanged();
            }
        }

        public List<OptionValueViewModel> DropdownOptions
        {
            get
            {
                if (_dropdownOptions == null)
                    _dropdownOptions = new List<OptionValueViewModel>();

                return _dropdownOptions;
            }
            set
            {
                _dropdownOptions = value;
                NotifyPropertyChanged();
            }
        }

        public Uri ImageOptionSource
        {
            get
            {
                return _imageOptionSource;
            }
            set
            {
                _imageOptionSource = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedOptionIndex
        {
            get
            {
                return _selectedOptionIndex;
            }
            set
            {
                _selectedOptionIndex = value;
                NotifyPropertyChanged();
                LoadSelectedOptionDetails();
            }
        }

        public int DropdownSelectedIndex
        {
            get
            {
                return _dropdownSelectedIndex;
            }
            set
            {
                _dropdownSelectedIndex = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SelectedDropdownText));
                UpdateViewForDropdownSelectionChanged();
            }
        }

        public Visibility CheckBoxVisibility
        {
            get
            {
                return _checkBoxVisibility;
            }
            set
            {
                _checkBoxVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility ComboBoxVisibility
        {
            get
            {
                return _comboBoxVisibility;
            }
            set
            {
                _comboBoxVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility PreviewButtonVisibility
        {
            get
            {
                return _previewButtonVisibility;
            }
            set
            {
                _previewButtonVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public ConfigureModViewModel()
        {
        }

        internal void Init(ModInfo info, Func<string, string> imageReader, Func<string, Stream> audioReader, Iros._7th.Workshop.ProfileItem activeModInfo, List<Constraint> modConstraints, string pathToModFolderOrIro)
        {
            _iroPath = pathToModFolderOrIro;
            _info = info;
            _imageReader = imageReader;
            _audioReader = audioReader;
            _constraints = modConstraints;
            _values = activeModInfo.Settings.ToDictionary(s => s.ID, s => s.Value, StringComparer.InvariantCultureIgnoreCase);
            ModOptions.AddRange(info.Options.Select(o => new ConfigOptionViewModel(o)).ToList());
            WindowTitle = $"Configure Mod - {Sys.Library.GetItem(activeModInfo.ModID)?.CachedDetails.Name}";

            SelectedOptionIndex = ModOptions.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// Returns the mod configuration as list of <see cref="ProfileSetting"/>s to be saved with a <see cref="Iros._7th.Workshop.ProfileItem"/>.
        /// </summary>
        internal List<ProfileSetting> GetSettings()
        {
            return _values.Select(kv => new ProfileSetting() { ID = kv.Key, Value = kv.Value }).ToList();
        }

        /// <summary>
        /// Updates the UI with the selected mod option to configure.
        /// </summary>
        internal void LoadSelectedOptionDetails()
        {
            _selectedOption = ModOptions.ElementAt(SelectedOptionIndex)?.Option;

            if (_selectedOption == null)
            {
                Logger.Warn($"option with index {SelectedOptionIndex} not found");
                return;
            }

            Description = _selectedOption.Description;

            int value;
            if (!_values.TryGetValue(_selectedOption.ID, out value))
                value = _selectedOption.Default;

            Constraint ct = _constraints.Find(c => c.Setting.Equals(_selectedOption.ID, StringComparison.InvariantCultureIgnoreCase));

            switch (_selectedOption.Type)
            {
                case OptionType.Bool:
                    UpdateViewForBoolOption(ct, value);
                    break;

                case OptionType.List:
                    UpdateViewForListOption(ct, value);
                    break;
            }
        }

        private void UpdateViewForListOption(Constraint ct, int value)
        {
            IEnumerable<OptionValue> values = _selectedOption.Values;
            if (ct.Require.Any())
            {
                value = ct.Require[0];
                CompatibilityNote = $"This option cannot be changed due to compatibility with other mods ({string.Join(", ", ct.ParticipatingMods)})";
                ComboBoxIsEnabled = false;
            }
            else if (ct.Forbid.Any())
            {
                var remove = values.Where(v => ct.Forbid.Contains(v.Value));
                CompatibilityNote = $"The following values: {string.Join(", ", remove.Select(o => o.Name))} have been removed due to compatibility with other mods ({string.Join(", ", ct.ParticipatingMods)})";
                values = values.Except(remove);
                if (!values.Any(v => v.Value == value)) value = values.First().Value;
                ComboBoxIsEnabled = true;
            }
            else
            {
                CompatibilityNote = string.Empty;
                ComboBoxIsEnabled = true;
            }

            CheckBoxVisibility = Visibility.Hidden;
            ComboBoxVisibility = Visibility.Visible;

            DropdownOptions.Clear();
            DropdownOptions = values.Select(v => new OptionValueViewModel(v)).ToList();
            DropdownSelectedIndex = DropdownOptions.FindIndex(m => m.OptionValue.Value == value);
        }

        private void UpdateViewForBoolOption(Constraint ct, int value)
        {
            if (ct.Require.Any())
            {
                value = ct.Require[0];
                CompatibilityNote = $"This option cannot be changed due to compatibility with other mods ({string.Join(", ", ct.ParticipatingMods)})";
                CheckBoxIsEnabled = false;
            }
            else if (ct.Forbid.Any())
            {
                value = new[] { 0, 1 }.Except(ct.Forbid).FirstOrDefault();
                CompatibilityNote = $"This option cannot be changed due to compatibility with other mods ({string.Join(", ", ct.ParticipatingMods)})";
                CheckBoxIsEnabled = false;
            }
            else
            {
                CompatibilityNote = "";
                CheckBoxIsEnabled = true;
            }

            CheckBoxVisibility = Visibility.Visible;
            ComboBoxVisibility = Visibility.Hidden;

            CheckBoxText = _selectedOption.Name;
            IsOptionChecked = (value == 1);
        }

        private void UpdateViewForCheckChanged()
        {
            if (_selectedOption == null)
                return;

            OptionValue o = _selectedOption.Values.Find(v => v.Value == (IsOptionChecked ? 1 : 0));

            if (o != null)
            {
                ImageOptionSource = null;
                ImageOptionSource = SetPreviewImage(_imageReader(o.PreviewFile));
            }
            else
            {
                ImageOptionSource = null;
            }

            SetupAudioPreview(o);
            _values[_selectedOption.ID] = IsOptionChecked ? 1 : 0;
        }

        private void SetupAudioPreview(OptionValue o)
        {
            if (!string.IsNullOrWhiteSpace(o.PreviewAudio))
            {
                PreviewButtonVisibility = Visibility.Visible;
            }
            else
            {
                PreviewButtonVisibility = Visibility.Hidden;
            }

            StopAudio();

            _audioFileName = null;
            _audioPath = null;
            _audioFile = null;

            if (!string.IsNullOrWhiteSpace(o.PreviewAudio))
            {
                Console.WriteLine("AUDIO = " + o.PreviewAudio);
                _audioFileName = Path.GetFileName(o.PreviewAudio);

                if (_iroPath.EndsWith(".iro"))
                {
                    string mod = _iroPath;
                    _audioPath = ExtractAudioFileFromIro(mod, o.PreviewAudio);
                }
                else
                {
                    _audioFile = _audioReader(o.PreviewAudio);
                }
            }
            
        }

        private void StopAudio()
        {
            if (_audio != null)
            {
                if (_waveReader != null)
                {
                    _waveReader.Dispose();
                    _waveReader = null;
                }

                if (_mp3Reader != null)
                {
                    _mp3Reader.Dispose();
                    _mp3Reader = null;
                }


                try
                {
                    _audio.PlaybackStopped -= PreviewAudio_PlaybackStopped;
                    _audio.Stop();
                    _audio.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Warn($"Error stopping audio: {e.Message}");
                }


                _audio = null;
                NotifyPropertyChanged(nameof(IsPlayingAudio));
            }
        }

        private Uri SetPreviewImage(string pathToImage)
        {
            if (string.IsNullOrWhiteSpace(pathToImage))
            {
                return null;
            }

            if (!File.Exists(pathToImage))
            {
                return null;
            }

            return new Uri(pathToImage);
        }

        private void UpdateViewForDropdownSelectionChanged()
        {
            if (_selectedOption == null || DropdownSelectedIndex == -1)
            {
                return;
            }

            OptionValue o = DropdownOptions.ElementAt(DropdownSelectedIndex)?.OptionValue;
            if (o != null)
            {
                ImageOptionSource = SetPreviewImage(_imageReader(o.PreviewFile));
            }
            else
            {
                ImageOptionSource = null;
            }

            SetupAudioPreview(o);

            _values[_selectedOption.ID] = o.Value;
        }

        internal static void DeleteTempFolder()
        {
            string config_temp_folder = Path.Combine(Sys.PathToTempFolder, "configmod");
            if (Directory.Exists(config_temp_folder))
            {
                try
                {
                    Directory.Delete(config_temp_folder, true);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private string ExtractAudioFileFromIro(string iroFile, string path)
        {
            IrosArc iro = new IrosArc(iroFile);


            string filter = path;
            string fullPathToAudioFile = "";

            foreach (string file in iro.AllFileNames())
            {
                if (!String.IsNullOrEmpty(filter) && (file.IndexOf(filter) < 0))
                    continue;

                byte[] data = iro.GetBytes(file);

                // ... temp audio files will be deleted on cleanup of the viewmodel
                fullPathToAudioFile = Path.Combine(Sys.PathToTempFolder, "audio", file);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPathToAudioFile));

                File.WriteAllBytes(fullPathToAudioFile, data); // write audio file to temp location
            }

            return fullPathToAudioFile;
        }

        internal void PlayPreviewAudio()
        {
            if (_audio == null)
            {
                try
                {

                    if (_audioFileName.EndsWith(".ogg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(_audioPath))
                        {
                            _audio = new NAudio.Wave.WaveOut();
                            _waveReader = new NAudio.Vorbis.VorbisWaveReader(_audioPath);
                            _audio.Init(_waveReader);
                        }
                        else if (_audioFile != null)
                        {
                            _audio = new NAudio.Wave.WaveOut();
                            _waveReader = new NAudio.Vorbis.VorbisWaveReader(_audioFile);
                            _audio.Init(_waveReader);
                        }
                    }
                    else if (_audioFileName.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(_audioPath))
                        {
                            _audio = new NAudio.Wave.WaveOut();
                            _mp3Reader = new NAudio.Wave.Mp3FileReader(_audioPath);
                            _audio.Init(_mp3Reader);
                        }
                        else if (_audioFile != null)
                        {
                            _audio = new NAudio.Wave.WaveOut();
                            _mp3Reader = new NAudio.Wave.Mp3FileReader(_audioFile);
                            _audio.Init(_mp3Reader);
                        }
                    }
                    else
                    {
                        Logger.Warn($"Unsupported audio file: {_audioFileName}");
                        return;
                    }

                    if (_waveReader != null || _mp3Reader != null)
                    {
                        _audio.PlaybackStopped += PreviewAudio_PlaybackStopped;
                        _audio.Play();
                    }
                    else
                    {
                        Sys.Message(new WMessage($"Failed to play preview audio {_audioFileName}"));
                    }

                    NotifyPropertyChanged(nameof(IsPlayingAudio));
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Sys.Message(new WMessage($"Failed to play preview audio {_audioFileName}: {e.Message}"));
                }

            }
            else
            {
                StopAudio();
            }
        }

        private void PreviewAudio_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            StopAudio();
        }

        internal void CleanUp()
        {
            StopAudio();
            ImageOptionSource = null;
            DeleteTempAudioFiles();
        }

        internal void ResetToModDefaultValues()
        {
            // loop through each viewmodel representing a modoption and set the value to the default
            foreach (ConfigOptionViewModel option in ModOptions)
            {
                _values[option.Option.ID] = option.Option.Default;
            }

            // reload what is selected in the UI to reflect new defaults set
            LoadSelectedOptionDetails();
        }

        /// <summary>
        /// deletes files from temp/audio that may have been extracted from an IRO for audio preview
        /// </summary>
        private void DeleteTempAudioFiles()
        {
            string pathToTemp = Path.Combine(Sys.PathToTempFolder, "audio");

            if (!Directory.Exists(pathToTemp))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(pathToTemp, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to delete temp audio file at {filePath}");
                }
            }
        }
    }

    public class ConfigOptionViewModel : ViewModelBase
    {
        internal ConfigOption Option { get; set; }

        private string _optionName;

        public string OptionName
        {
            get
            {
                return _optionName;
            }
            set
            {
                _optionName = value;
                NotifyPropertyChanged();
            }
        }

        public ConfigOptionViewModel(ConfigOption option)
        {
            Option = option;
            OptionName = option.Name;
        }
    }

    public class OptionValueViewModel : ViewModelBase
    {
        internal OptionValue OptionValue { get; set; }

        private string _valueName;

        public string ValueName
        {
            get
            {
                return _valueName;
            }
            set
            {
                _valueName = value;
                NotifyPropertyChanged();
            }
        }

        public OptionValueViewModel(OptionValue optionValue)
        {
            OptionValue = optionValue;
            ValueName = OptionValue.Name;
        }
    }
}
