using _7thHeaven.Code;
using Iros._7th.Workshop;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SeventhHeaven.Classes;
using SeventhHeaven.Windows;
using SeventhHeavenUI;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.ViewModels
{
    internal enum AudioChannel
    {
        Left,
        Center,
        Right
    }

    internal enum VolumeSlider
    {
        Music,
        Sfx
    }

    class GameLaunchSettingsViewModel : ViewModelBase
    {
        #region Data Members
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _statusMessage;
        private bool _autoMountChecked;
        private bool _autoUnmountChecked;
        private bool _autoUpdatePathChecked;
        private bool _highDpiFixChecked;
        private bool _isAutoMountSupported;
        private bool _isReverseSpeakersChecked;
        private bool _isLogVolumeChecked;
        private bool _isRivaOptionChecked;
        private bool _isTntOptionChecked;
        private string _selectedSoundDevice;
        private string _selectedMidiDevice;
        private string _selectedRenderer;
        private int _volumeValue;

        private WaveOut _audioTest;

        private ObservableCollection<ProgramToRunViewModel> _programList;
        private string _newProgramPathText;
        private string _newProgramArgsText;
        private bool _isProgramPopupOpen;
        private int _sfxVolumeValue;
        private bool _isQuarterScreenChecked;
        private bool _isFullScreenChecked;
        private bool _isShowLauncherChecked;
        private string _importStatusMessage;
        private bool _importButtonIsEnabled;
        private Visibility _importProgressVisibility;
        private double _importProgressValue;
        private VolumeSlider _lastVolumeSliderChanged;
        private bool _isSoundDevicesLoaded;
        private string _selectedMountOption;

        #endregion


        #region Properties

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsAudioPlaying
        {
            get
            {
                return _audioTest != null && _audioTest.PlaybackState == PlaybackState.Playing;
            }
        }

        public bool IsAudioNotPlaying
        {
            get
            {
                return _audioTest == null;
            }
        }

        public bool IsSoundDevicesLoaded
        {
            get
            {
                return _isSoundDevicesLoaded;
            }
            set
            {
                _isSoundDevicesLoaded = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsAutoMountSupported
        {
            get
            {
                return _isAutoMountSupported;
            }
            set
            {
                _isAutoMountSupported = value;
                NotifyPropertyChanged();
            }
        }

        public bool AutoMountChecked
        {
            get
            {
                return _autoMountChecked;
            }
            set
            {
                _autoMountChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool AutoUnmountChecked
        {
            get
            {
                return _autoUnmountChecked;
            }
            set
            {
                _autoUnmountChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool AutoUpdatePathChecked
        {
            get
            {
                return _autoUpdatePathChecked;
            }
            set
            {
                _autoUpdatePathChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsShowLauncherChecked
        {
            get
            {
                return _isShowLauncherChecked;
            }
            set
            {
                _isShowLauncherChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool HighDpiFixChecked
        {
            get
            {
                return _highDpiFixChecked;
            }
            set
            {
                _highDpiFixChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsReverseSpeakersChecked
        {
            get
            {
                return _isReverseSpeakersChecked;
            }
            set
            {
                _isReverseSpeakersChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsLogVolumeChecked
        {
            get
            {
                return _isLogVolumeChecked;
            }
            set
            {
                _isLogVolumeChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsRivaOptionChecked
        {
            get
            {
                return _isRivaOptionChecked;
            }
            set
            {
                _isRivaOptionChecked = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility RivaOptionsVisibility
        {
            get
            {
                if (SelectedRenderer == "Direct3D Hardware Acceleration")
                    return Visibility.Visible;

                return Visibility.Hidden;
            }
        }

        public bool IsTntOptionChecked
        {
            get
            {
                return _isTntOptionChecked;
            }
            set
            {
                _isTntOptionChecked = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility ScreenModesVisibility
        {
            get
            {
                if (SelectedRenderer != "Custom 7H Game Driver")
                    return Visibility.Visible;

                return Visibility.Hidden;
            }
        }

        public bool IsQuarterScreenChecked
        {
            get
            {
                return _isQuarterScreenChecked;
            }
            set
            {
                _isQuarterScreenChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFullScreenChecked
        {
            get
            {
                return _isFullScreenChecked;
            }
            set
            {
                _isFullScreenChecked = value;
                NotifyPropertyChanged();
            }
        }

        public string SelectedSoundDevice
        {
            get
            {
                return _selectedSoundDevice;
            }
            set
            {
                _selectedSoundDevice = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> SoundDevices
        {
            get
            {
                return SoundDeviceGuids?.Keys.ToList();
            }
        }

        public Dictionary<string, Guid> SoundDeviceGuids { get; set; }

        public string SelectedMidiDevice
        {
            get
            {
                return _selectedMidiDevice;
            }
            set
            {
                _selectedMidiDevice = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> MidiDevices
        {
            get
            {
                return MidiDeviceMap?.Keys.ToList();
            }
        }

        public Dictionary<string, string> MidiDeviceMap { get; set; }

        public string SelectedRenderer
        {
            get
            {
                return _selectedRenderer;
            }
            set
            {
                _selectedRenderer = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(RivaOptionsVisibility));
                NotifyPropertyChanged(nameof(ScreenModesVisibility));

                // ensure atleast one radio button is checked when changing renderer
                if (!IsTntOptionChecked && !IsRivaOptionChecked)
                {
                    IsTntOptionChecked = true;
                }

                if (!IsQuarterScreenChecked && !IsFullScreenChecked)
                {
                    IsFullScreenChecked = true;
                }

                ShowWarningMessageAboutRenderer();
            }
        }

        public List<string> AvailableRenderers
        {
            get
            {
                return RendererMap?.Keys.ToList();
            }
        }

        public Dictionary<string, int> RendererMap { get; set; }

        public int MusicVolumeValue
        {
            get
            {
                return _volumeValue;
            }
            set
            {
                _volumeValue = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(MusicVolumeDisplayText));
                LastVolumeSliderChanged = VolumeSlider.Music;

                if (IsAudioPlaying && LastVolumeSliderChanged == VolumeSlider.Music)
                {
                    _audioTest.Volume = (float)_volumeValue / (float)100.0;
                }
            }
        }

        public int SfxVolumeValue
        {
            get
            {
                return _sfxVolumeValue;
            }
            set
            {
                _sfxVolumeValue = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SfxVolumeDisplayText));
                LastVolumeSliderChanged = VolumeSlider.Sfx;

                if (IsAudioPlaying && LastVolumeSliderChanged == VolumeSlider.Sfx)
                {
                    _audioTest.Volume = (float)_sfxVolumeValue / (float)100.0;
                }
            }
        }

        public string SfxVolumeDisplayText
        {
            get
            {
                return $"{ResourceHelper.Get(StringKey.Volume)}: {SfxVolumeValue}";
            }
        }

        public string MusicVolumeDisplayText
        {
            get
            {
                return $"{ResourceHelper.Get(StringKey.Volume)}: {MusicVolumeValue}";
            }
        }

        public bool IsProgramPopupOpen
        {
            get
            {
                return _isProgramPopupOpen;
            }
            set
            {
                _isProgramPopupOpen = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ProgramToRunViewModel> ProgramList
        {
            get
            {
                if (_programList == null)
                    _programList = new ObservableCollection<ProgramToRunViewModel>();

                return _programList;
            }
            set
            {
                _programList = value;
                NotifyPropertyChanged();
            }
        }

        public string NewProgramPathText
        {
            get
            {
                return _newProgramPathText;
            }
            set
            {
                _newProgramPathText = value;
                NotifyPropertyChanged();
            }
        }

        public string NewProgramArgsText
        {
            get
            {
                return _newProgramArgsText;
            }
            set
            {
                _newProgramArgsText = value;
                NotifyPropertyChanged();
            }
        }

        private bool HasLoaded { get; set; }

        public string ImportStatusMessage
        {
            get
            {
                return _importStatusMessage;
            }
            set
            {
                _importStatusMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool ImportButtonIsEnabled
        {
            get
            {
                return _importButtonIsEnabled;
            }
            set
            {
                _importButtonIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility ImportProgressVisibility
        {
            get
            {
                return _importProgressVisibility;
            }
            set
            {
                _importProgressVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public double ImportProgressValue
        {
            get
            {
                return _importProgressValue;
            }
            set
            {
                _importProgressValue = value;
                NotifyPropertyChanged();
            }
        }

        public VolumeSlider LastVolumeSliderChanged
        {
            get
            {
                return _lastVolumeSliderChanged;
            }
            set
            {
                _lastVolumeSliderChanged = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// List of ways to mount disc (matches the order of enum <see cref="MountDiscOption"/>)
        /// </summary>
        public List<string> MountOptions
        {
            get
            {
                // this list matches the order of the mountoption enum
                return new List<string>()
                {
                    "Mount Disc With PowerShell",
                    "Mount Disc With WinCDEmu"
                };
            }
        }

        public string SelectedMountOption
        {
            get
            {
                return _selectedMountOption;
            }
            set
            {
                if (_selectedMountOption != value)
                {
                    _selectedMountOption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public MountDiscOption SelectedMountOptionAsEnum
        {
            get
            {
                if (string.IsNullOrEmpty(SelectedMountOption))
                {
                    return MountDiscOption.Unknown;
                }

                return (MountDiscOption) MountOptions.IndexOf(SelectedMountOption);
            }
        }

        #endregion


        public GameLaunchSettingsViewModel()
        {
            StatusMessage = "";
            NewProgramPathText = "";
            NewProgramArgsText = "";
            IsProgramPopupOpen = false;
            IsSoundDevicesLoaded = false;
            _audioTest = null;
            HasLoaded = false;
            LastVolumeSliderChanged = VolumeSlider.Music;


            // initialize sound devices on background task because it can take up to 1 second to loop over audio devices and get their names
            SelectedSoundDevice = "Loading Devices ...";
            Task.Factory.StartNew(() =>
            {
                InitSoundDevices();
                SetSelectedSoundDeviceFromSettings(Sys.Settings.GameLaunchSettings);
                IsSoundDevicesLoaded = true;
            });

            InitImportMovieOption();
            InitRenderers();
            InitMidiDevices();
            LoadSettings(Sys.Settings.GameLaunchSettings);
        }

        private void InitImportMovieOption()
        {
            ImportProgressVisibility = Visibility.Hidden;
            ImportProgressValue = 0;
            ImportStatusMessage = "";
            ImportButtonIsEnabled = !GameConverter.AllMovieFilesExist(Sys.Settings.MovieFolder);

            Dictionary<string, string[]> missingMovies = GameConverter.GetMissingMovieFiles(Sys.Settings.MovieFolder);

            if (missingMovies.Count > 0)
            {
                List<string> discsToInsert = GetDiscsToInsertForMissingMovies(missingMovies);

                ImportStatusMessage = string.Format(ResourceHelper.Get(StringKey.InsertAndClickImport), discsToInsert[0]);
            }
        }

        private List<string> GetDiscsToInsertForMissingMovies(Dictionary<string, string[]> missingMovies)
        {
            List<string> discsToInsert = new List<string>();
            missingMovies.Values.GroupBy(s => s)
                                .Select(g => g.Key)
                                .ToList()
                                .ForEach(s => discsToInsert.AddRange(s)); // add all string[] to one List<string> 

            discsToInsert = discsToInsert.Distinct().OrderBy(s => s).ToList();

            return discsToInsert;
        }

        private void LoadSettings(LaunchSettings launchSettings)
        {
            HasLoaded = false;

            if (Sys.Settings.GameLaunchSettings == null)
            {
                Logger.Warn("No game launcher settings found, initializing to defaults.");
                Sys.Settings.GameLaunchSettings = LaunchSettings.DefaultSettings();
                launchSettings = Sys.Settings.GameLaunchSettings;
            }

            ProgramList = new ObservableCollection<ProgramToRunViewModel>(Sys.Settings.ProgramsToLaunchPrior.Select(s => new ProgramToRunViewModel(s.PathToProgram, s.ProgramArgs)));

            AutoUpdatePathChecked = launchSettings.AutoUpdateDiscPath;
            HighDpiFixChecked = launchSettings.HighDpiFix;

            IsShowLauncherChecked = launchSettings.ShowLauncherWindow;


            IsAutoMountSupported = GameDiscMounter.OSHasBuiltInMountSupport();
            AutoMountChecked = launchSettings.AutoMountGameDisc;
            AutoUnmountChecked = launchSettings.AutoUnmountGameDisc;

            // options to select mount method is disabled if user OS does not support and forced to use wincdemu  (i.e. does not support PowerShell Mount-DiskImage)
            if (IsAutoMountSupported)
            {
                SelectedMountOption = MountOptions[(int)launchSettings.MountingOption];
            }
            else
            {
                SelectedMountOption = MountOptions[(int)MountDiscOption.MountWithWinCDEmu];
            }

            SetSelectedSoundDeviceFromSettings(launchSettings);

            SelectedMidiDevice = MidiDeviceMap.Where(s => s.Value == launchSettings.SelectedMidiDevice)
                                              .Select(s => s.Key)
                                              .FirstOrDefault();

            MusicVolumeValue = launchSettings.MusicVolume;
            SfxVolumeValue = launchSettings.SfxVolume;
            IsReverseSpeakersChecked = launchSettings.ReverseSpeakers;
            IsLogVolumeChecked = launchSettings.LogarithmicVolumeControl;

            SelectedRenderer = RendererMap.Where(s => s.Value == launchSettings.SelectedRenderer)
                                          .Select(s => s.Key)
                                          .FirstOrDefault();

            IsRivaOptionChecked = launchSettings.UseRiva128GraphicsOption;
            IsTntOptionChecked = launchSettings.UseTntGraphicsOption;
            IsQuarterScreenChecked = launchSettings.QuarterScreenMode;
            IsFullScreenChecked = launchSettings.FullScreenMode;

            HasLoaded = true;
        }

        private void SetSelectedSoundDeviceFromSettings(LaunchSettings launchSettings)
        {
            if (SoundDeviceGuids == null || SoundDeviceGuids.Count == 0)
            {
                return;
            }

            SelectedSoundDevice = SoundDeviceGuids.Where(s => s.Value == launchSettings.SelectedSoundDevice)
                                                  .Select(s => s.Key)
                                                  .FirstOrDefault();

            // switch back to 'Auto' if device not found
            if (SelectedSoundDevice == null)
            {
                SelectedSoundDevice = SoundDeviceGuids.Where(s => s.Value == Guid.Empty)
                                                      .Select(s => s.Key)
                                                      .FirstOrDefault();
            }
        }

        internal bool SaveSettings()
        {
            try
            {
                Sys.Settings.ProgramsToLaunchPrior = GetUpdatedProgramsToRun();

                Sys.Settings.GameLaunchSettings.AutoMountGameDisc = AutoMountChecked;
                Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc = AutoUnmountChecked;
                Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath = AutoUpdatePathChecked;
                Sys.Settings.GameLaunchSettings.MountingOption = SelectedMountOptionAsEnum;
                Sys.Settings.GameLaunchSettings.ShowLauncherWindow = IsShowLauncherChecked;

                Sys.Settings.GameLaunchSettings.HighDpiFix = HighDpiFixChecked;
                Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch = true; // always have this set to true

                Sys.Settings.GameLaunchSettings.SelectedSoundDevice = SoundDeviceGuids[SelectedSoundDevice];
                Sys.Settings.GameLaunchSettings.SelectedMidiDevice = MidiDeviceMap[SelectedMidiDevice];
                Sys.Settings.GameLaunchSettings.MusicVolume = MusicVolumeValue;
                Sys.Settings.GameLaunchSettings.SfxVolume = SfxVolumeValue;
                Sys.Settings.GameLaunchSettings.ReverseSpeakers = IsReverseSpeakersChecked;
                Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl = IsLogVolumeChecked;

                Sys.Settings.GameLaunchSettings.SelectedRenderer = RendererMap[SelectedRenderer];
                Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption = IsRivaOptionChecked;
                Sys.Settings.GameLaunchSettings.UseTntGraphicsOption = IsTntOptionChecked;
                Sys.Settings.GameLaunchSettings.QuarterScreenMode = IsQuarterScreenChecked;
                Sys.Settings.GameLaunchSettings.FullScreenMode = IsFullScreenChecked;

                Sys.Save();

                Sys.Message(new WMessage(ResourceHelper.Get(StringKey.GameLauncherSettingsUpdated)));
                return true;
            }
            catch (Exception e)
            {
                StatusMessage = $"{ResourceHelper.Get(StringKey.FailedToSaveLaunchSettings)}: {e.Message}";
                Logger.Error(e);
                return false;
            }
        }

        private void InitSoundDevices()
        {
            var deviceGuids = new Dictionary<string, Guid>();

            deviceGuids.Add("Auto-Switch (Windows Default)", Guid.Empty);

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                WaveOutCapabilities caps;

                try
                {
                    caps = WaveOut.GetCapabilities(n);
                }
                catch (Exception e)
                {
                    Logger.Warn(e);
                    continue;
                }

                if (caps.ProductGuid == Guid.Empty)
                    continue;

                // reference: https://stackoverflow.com/questions/1449162/get-the-full-name-of-a-wavein-device
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    if (device.FriendlyName.StartsWith(caps.ProductName) && !deviceGuids.ContainsKey(device.FriendlyName))
                    {
                        Guid audioGuid = new Guid(device.Properties[PropertyKeys.PKEY_AudioEndpoint_GUID].Value as string);
                        deviceGuids.Add(device.FriendlyName, audioGuid);
                        break;
                    }
                }
            }

            SoundDeviceGuids = deviceGuids;
            NotifyPropertyChanged(nameof(SoundDevices));
        }

        private void InitMidiDevices()
        {
            MidiDeviceMap = new Dictionary<string, string>()
            {
                { "General MIDI", "GENERAL_MIDI" },
                { "Soundfont MIDI (Creative AWE32/AWE64)", "SOUNDFONT_MIDI" },
                { "Yamaha XG MIDI", "YAMAHA_XG" }
            };
        }

        private void InitRenderers()
        {
            RendererMap = new Dictionary<string, int>()
            {
                { "Software Renderer", 0 },
                { "Direct3D Hardware Acceleration", 1 },
                { "Custom 7H Game Driver", 3 }
            };
        }

        /// <summary>
        /// Plays audio_test_file.ogg based on launch settings for testing audio
        /// </summary>
        /// <param name="channel"> Where audio will play from: Left,Center,or Right channel</param>
        internal void TestAudio(AudioChannel channel)
        {
            string pathToTestFile = Path.Combine(Sys._7HFolder, "Resources", "audio_test_file.ogg");


            if (_audioTest == null)
            {
                // reference: https://markheath.net/post/handling-multi-channel-audio-in-naudio
                // reference: https://stackoverflow.com/questions/22248138/play-sound-on-specific-channel-with-naudio?rq=1

                // input 0 - audio test .ogg
                // input 1 - silenced wave provider to play silent audio
                NAudio.Vorbis.VorbisWaveReader waveReader = new NAudio.Vorbis.VorbisWaveReader(pathToTestFile);
                MultiplexingWaveProvider waveProvider = new MultiplexingWaveProvider(new List<IWaveProvider>() { waveReader, new SilenceWaveProvider(waveReader.WaveFormat) }, 2);

                int leftChannel = 0;
                int rightChannel = 1;

                if (IsReverseSpeakersChecked)
                {
                    leftChannel = 1;
                    rightChannel = 0;
                }

                if (channel == AudioChannel.Left)
                {
                    // note that the wave reader has 2 input channels so we must route both input channels of the reader to the output channel
                    waveProvider.ConnectInputToOutput(0, leftChannel);
                    waveProvider.ConnectInputToOutput(1, leftChannel);

                    waveProvider.ConnectInputToOutput(2, rightChannel);
                    waveProvider.ConnectInputToOutput(3, rightChannel);
                }
                else if (channel == AudioChannel.Right)
                {
                    waveProvider.ConnectInputToOutput(0, rightChannel);
                    waveProvider.ConnectInputToOutput(1, rightChannel);

                    waveProvider.ConnectInputToOutput(2, leftChannel);
                    waveProvider.ConnectInputToOutput(3, leftChannel);
                }
                else
                {
                    waveProvider.ConnectInputToOutput(0, leftChannel);
                    waveProvider.ConnectInputToOutput(1, leftChannel);

                    waveProvider.ConnectInputToOutput(0, rightChannel);
                    waveProvider.ConnectInputToOutput(1, rightChannel);
                }

                float outVolume = (float)MusicVolumeValue / (float)100.0;
                if (LastVolumeSliderChanged == VolumeSlider.Sfx)
                {
                    outVolume = (float)SfxVolumeValue / (float)100.0;
                }

                _audioTest = new WaveOut
                {
                    DeviceNumber = GetSelectedSoundDeviceNumber(),
                    Volume = outVolume
                };

                _audioTest.Init(waveProvider);

                _audioTest.PlaybackStopped += AudioTest_PlaybackStopped;
                _audioTest.Play();

                NotifyPropertyChanged(nameof(IsAudioPlaying));
                NotifyPropertyChanged(nameof(IsAudioNotPlaying));
            }
        }

        private int GetSelectedSoundDeviceNumber()
        {
            Guid selectedGuid = SoundDeviceGuids[SelectedSoundDevice];

            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);

                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    if (device.FriendlyName.StartsWith(caps.ProductName))
                    {
                        Guid soundGuid = new Guid(device.Properties[PropertyKeys.PKEY_AudioEndpoint_GUID].Value as string);

                        if (soundGuid == selectedGuid)
                        {
                            return n;
                        }
                    }
                }
            }

            return -1; // if device not found return default device which is -1
        }

        private void AudioTest_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_audioTest != null)
            {
                _audioTest.Stop();
                _audioTest.PlaybackStopped -= AudioTest_PlaybackStopped;
                _audioTest = null;

                NotifyPropertyChanged(nameof(IsAudioPlaying));
                NotifyPropertyChanged(nameof(IsAudioNotPlaying));
            }
        }

        /// <summary>
        /// Shows warning message dialog to user if <see cref="SelectedRenderer"/> is not set to "Custom 7H Game Driver"
        /// </summary>
        private void ShowWarningMessageAboutRenderer()
        {
            if (!HasLoaded)
            {
                // prevent warning from showing when loading settings
                return;
            }

            if (!SelectedRenderer.Equals("Custom 7H Game Driver", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageDialogWindow.Show(ResourceHelper.Get(StringKey.ChoosingAnyOtherOptionBesidesCustom7HGameDriver), ResourceHelper.Get(StringKey.Warning), MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void EditSelectedProgram(ProgramToRunViewModel selected)
        {
            IsProgramPopupOpen = true;
            NewProgramPathText = selected.ProgramPath;
            NewProgramArgsText = selected.ProgramArguments ?? "";
        }

        internal void AddNewProgram()
        {
            IsProgramPopupOpen = true;
        }

        /// <summary>
        /// Adds or Edits program to run and closes programs popup
        /// </summary>
        internal bool SaveProgramToRun()
        {
            if (!File.Exists(NewProgramPathText))
            {
                StatusMessage = ResourceHelper.Get(StringKey.ProgramToRunNotFound);
                return false;
            }

            if (!ProgramList.Any(s => s.ProgramPath == NewProgramPathText))
            {
                ProgramList.Add(new ProgramToRunViewModel(NewProgramPathText, NewProgramArgsText));
            }
            else
            {
                ProgramToRunViewModel toEdit = ProgramList.FirstOrDefault(s => s.ProgramPath == NewProgramPathText);
                toEdit.ProgramArguments = NewProgramArgsText;
            }

            CloseProgramPopup();
            return true;
        }

        internal void CloseProgramPopup()
        {
            IsProgramPopupOpen = false;
            NewProgramPathText = "";
            NewProgramArgsText = "";
        }

        internal void RemoveSelectedProgram(ProgramToRunViewModel selected)
        {
            ProgramList.Remove(selected);
        }

        /// <summary>
        /// Returns list of <see cref="ProgramLaunchInfo"/> objects based on the current input in <see cref="ProgramList"/>
        /// </summary>
        private List<ProgramLaunchInfo> GetUpdatedProgramsToRun()
        {
            List<ProgramLaunchInfo> updatedPrograms = new List<ProgramLaunchInfo>();

            foreach (ProgramToRunViewModel item in ProgramList.ToList())
            {
                ProgramLaunchInfo existingProg = Sys.Settings.ProgramsToLaunchPrior.FirstOrDefault(s => s.PathToProgram == item.ProgramPath);

                if (existingProg == null)
                {
                    existingProg = new ProgramLaunchInfo() { PathToProgram = item.ProgramPath, ProgramArgs = item.ProgramArguments };
                }
                else
                {
                    existingProg.ProgramArgs = item.ProgramArguments;
                }

                updatedPrograms.Add(existingProg);
            }

            return updatedPrograms;
        }

        internal void ImportMissingMovies()
        {
            string warningMessage = string.Format(ResourceHelper.Get(StringKey.ImportMissingMoviesWarningMessage), Sys.Settings.MovieFolder); 
            MessageDialogViewModel result = MessageDialogWindow.Show(warningMessage, ResourceHelper.Get(StringKey.Warning), MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result.Result == MessageBoxResult.No)
            {
                return;
            }

            ImportButtonIsEnabled = false;
            ImportProgressVisibility = Visibility.Visible;

            bool cancelProcess = false;
            int totalFiles = 0;
            int filesCopied = 0;

            Task importTask = Task.Factory.StartNew(() =>
            {
                Dictionary<string, string[]> missingMovies = GameConverter.GetMissingMovieFiles(Sys.Settings.MovieFolder);
                List<string> discsToInsert = GetDiscsToInsertForMissingMovies(missingMovies);

                totalFiles = missingMovies.Count;

                foreach (string disc in discsToInsert)
                {
                    List<string> driveLetters;

                    do
                    {
                        SetImportStatus($"{ResourceHelper.Get(StringKey.LookingFor)} {disc} ...");
                        driveLetters = GameLauncher.GetDriveLetters(disc);

                        if (driveLetters.Count == 0)
                        {
                            SetImportStatus(string.Format(ResourceHelper.Get(StringKey.InsertToContinue), disc));

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                string discNotFoundMessage = string.Format(ResourceHelper.Get(StringKey.PleaseInsertToContinueCopying), disc);
                                MessageDialogViewModel insertDiscResult = MessageDialogWindow.Show(discNotFoundMessage, ResourceHelper.Get(StringKey.InsertDisc), MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                                cancelProcess = (insertDiscResult.Result == MessageBoxResult.Cancel);
                            });
                        }

                        if (cancelProcess)
                        {
                            return;
                        }

                    } while (driveLetters.Count == 0);

                    SetImportStatus($"{string.Format(ResourceHelper.Get(StringKey.FoundDiscAt), disc)} {string.Join("  ", driveLetters)} ...");

                    // loop over missing files on the found disc and copy to data/movies destination
                    foreach (string movieFile in missingMovies.Where(kv => kv.Value.Any(s => s.Equals(disc, StringComparison.InvariantCultureIgnoreCase)))
                                                              .Select(kv => kv.Key))
                    {

                        foreach (string drive in driveLetters)
                        {
                            string fullTargetPath = Path.Combine(Sys.Settings.MovieFolder, movieFile);
                            string sourceFilePath = Path.Combine(drive, "ff7", "movies", movieFile);

                            if (File.Exists(sourceFilePath))
                            {
                                if (File.Exists(fullTargetPath))
                                {
                                    SetImportStatus($"{ResourceHelper.Get(StringKey.Overwriting)} {movieFile} ...");
                                }
                                else
                                {
                                    SetImportStatus($"{ResourceHelper.Get(StringKey.Copying)} {movieFile} ...");
                                }

                                File.Copy(sourceFilePath, fullTargetPath, true);
                                filesCopied++;
                                UpdateImportProgress(filesCopied, totalFiles);
                                break;
                            }
                            else
                            {
                                SetImportStatus(string.Format(ResourceHelper.Get(StringKey.FailedToFindAt), movieFile, sourceFilePath));
                            }
                        }
                    }
                }
            });

            importTask.ContinueWith((taskResult) =>
            {
                if (taskResult.IsFaulted)
                {
                    Logger.Error(taskResult.Exception);
                    SetImportStatus($"{ResourceHelper.Get(StringKey.AnErrorOccurredCopyingMovies)}: {taskResult.Exception.GetBaseException().Message}");
                }
                else if (cancelProcess)
                {
                    InitImportMovieOption();
                }
                else
                {
                    if (filesCopied == totalFiles)
                    {
                        SetImportStatus(ResourceHelper.Get(StringKey.SuccessfullyCopiedMovies));
                    }
                    else
                    {
                        SetImportStatus(ResourceHelper.Get(StringKey.FinishedCopyingMoviesSomeFailed));
                    }

                    ImportButtonIsEnabled = !GameConverter.AllMovieFilesExist(Sys.Settings.MovieFolder);
                    ImportProgressValue = 0;
                    ImportProgressVisibility = Visibility.Hidden;
                }
            });

        }

        private void SetImportStatus(string message)
        {
            ImportStatusMessage = message;
            Logger.Info(ImportStatusMessage);
            App.ForceUpdateUI();
        }

        private void UpdateImportProgress(int copiedCount, int totalCount)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (totalCount == 0)
                {
                    ImportProgressValue = 0;
                }
                else
                {
                    ImportProgressValue = ((double)copiedCount / (double)totalCount) * 100;
                }
                App.ForceUpdateUI();
            });
        }

        internal void ChangeProgramOrder(ProgramToRunViewModel program, int delta)
        {
            int currentIndex = ProgramList.IndexOf(program);
            int targetIndex = currentIndex + delta;

            if (targetIndex == currentIndex || targetIndex < 0 || targetIndex >= ProgramList.Count)
            {
                return;
            }

            ProgramList.Move(currentIndex, targetIndex);
        }

        internal void ResetToDefaults()
        {
            Logger.Info("Resetting game launcher settings to defaults.");
            LoadSettings(LaunchSettings.DefaultSettings());
        }

    }
}
