using _7thHeaven.Code;
using Iros._7th.Workshop;
using NAudio.Wave;
using SeventhHeaven.Classes;
using SeventhHeaven.Windows;
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
    internal enum AudioChannel
    {
        Left,
        Center,
        Right
    }

    class GameLaunchSettingsViewModel : ViewModelBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _statusMessage;
        private bool _autoMountChecked;
        private bool _autoUnmountChecked;
        private bool _autoUpdatePathChecked;
        private bool _code5FixChecked;
        private bool _highDpiFixChecked;
        private bool _disableReunionChecked;
        private bool _isReunionInstalled;
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

        public bool Code5FixChecked
        {
            get
            {
                return _code5FixChecked;
            }
            set
            {
                _code5FixChecked = value;
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

        public bool DisableReunionChecked
        {
            get
            {
                return _disableReunionChecked;
            }
            set
            {
                _disableReunionChecked = value;
                NotifyPropertyChanged();
                ShowWarningMessageAbouReunion();
            }
        }

        public bool IsReunionInstalled
        {
            get
            {
                return _isReunionInstalled;
            }
            set
            {
                _isReunionInstalled = value;
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

        public bool IsRivaOptionEnabled
        {
            get
            {
                return SelectedRenderer == "Direct3D Hardware Acceleration";
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
                NotifyPropertyChanged(nameof(IsRivaOptionEnabled));
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

        public int VolumeValue
        {
            get
            {
                return _volumeValue;
            }
            set
            {
                _volumeValue = value;
                NotifyPropertyChanged();
            }
        }

        public GameLaunchSettingsViewModel()
        {
            StatusMessage = "";
            _audioTest = null;

            InitSoundDevices();
            InitRenderers();
            InitMidiDevices();
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (Sys.Settings.GameLaunchSettings == null)
            {
                Logger.Warn("No game launch settings found, initializing to defaults.");
                Sys.Settings.GameLaunchSettings = LaunchSettings.DefaultSettings();
            }

            AutoUpdatePathChecked = Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath;
            Code5FixChecked = Sys.Settings.GameLaunchSettings.Code5Fix;
            HighDpiFixChecked = Sys.Settings.GameLaunchSettings.HighDpiFix;

            // disable options to auto-mount if user OS does not support it
            IsAutoMountSupported = GameLauncher.OSHasAutoMountSupport();

            if (IsAutoMountSupported)
            {
                AutoMountChecked = Sys.Settings.GameLaunchSettings.AutoMountGameDisc;
                AutoUnmountChecked = Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc;
            }
            else
            {
                AutoMountChecked = false;
                AutoUnmountChecked = false;
            }

            // Have option look unchecked and disabled when user does not have The Reunion installed
            IsReunionInstalled = GameLauncher.IsReunionModInstalled();
            if (IsReunionInstalled)
            {
                DisableReunionChecked = Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch;
            }
            else
            {
                DisableReunionChecked = false;
                IsReunionInstalled = false;
            }


            SelectedSoundDevice = SoundDeviceGuids.Where(s => s.Value == Sys.Settings.GameLaunchSettings.SelectedSoundDevice)
                                                  .Select(s => s.Key)
                                                  .FirstOrDefault();

            // switch back to 'Auto' if device not found
            if (SelectedSoundDevice == null)
            {
                SelectedSoundDevice = SoundDeviceGuids.Where(s => s.Value == Guid.Empty)
                                                      .Select(s => s.Key)
                                                      .FirstOrDefault();
            }

            SelectedMidiDevice = MidiDeviceMap.Where(s => s.Value == Sys.Settings.GameLaunchSettings.SelectedMidiDevice)
                                              .Select(s => s.Key)
                                              .FirstOrDefault();

            VolumeValue = Sys.Settings.GameLaunchSettings.SoundVolume;
            IsReverseSpeakersChecked = Sys.Settings.GameLaunchSettings.ReverseSpeakers;
            IsLogVolumeChecked = Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl;

            SelectedRenderer = RendererMap.Where(s => s.Value == Sys.Settings.GameLaunchSettings.SelectedRenderer)
                                          .Select(s => s.Key)
                                          .FirstOrDefault();

            IsRivaOptionChecked = Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption;
            IsTntOptionChecked = Sys.Settings.GameLaunchSettings.UseTntGraphicsOption;
        }

        internal bool SaveSettings()
        {
            try
            {
                Sys.Settings.GameLaunchSettings.AutoMountGameDisc = AutoMountChecked;
                Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc = AutoUnmountChecked;
                Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath = AutoUpdatePathChecked;

                Sys.Settings.GameLaunchSettings.Code5Fix = Code5FixChecked;
                Sys.Settings.GameLaunchSettings.HighDpiFix = HighDpiFixChecked;


                IsReunionInstalled = GameLauncher.IsReunionModInstalled();
                if (IsReunionInstalled)
                {
                    Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch = DisableReunionChecked;
                }
                else
                {
                    Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch = true; // always have this set to true when Reunion is not installed in case user later installs it
                }


                Sys.Settings.GameLaunchSettings.SelectedSoundDevice = SoundDeviceGuids[SelectedSoundDevice];
                Sys.Settings.GameLaunchSettings.SelectedMidiDevice = MidiDeviceMap[SelectedMidiDevice];
                Sys.Settings.GameLaunchSettings.SoundVolume = VolumeValue;
                Sys.Settings.GameLaunchSettings.ReverseSpeakers = IsReverseSpeakersChecked;
                Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl = IsLogVolumeChecked;

                Sys.Settings.GameLaunchSettings.SelectedRenderer = RendererMap[SelectedRenderer];
                Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption = IsRivaOptionChecked;
                Sys.Settings.GameLaunchSettings.UseTntGraphicsOption = IsTntOptionChecked;

                Sys.Save();

                Sys.Message(new WMessage("Game Launch settings updated!", true));
                return true;
            }
            catch (Exception e)
            {
                StatusMessage = $"Failed to save launch settings: {e.Message}";
                Logger.Error(e);
                return false;
            }
        }

        private void InitSoundDevices()
        {
            var deviceGuids = new Dictionary<string, Guid>();

            deviceGuids.Add("Auto-Switch (Windows Default)", Guid.Empty);

            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);

                if (caps.ProductGuid == Guid.Empty)
                    continue;

                deviceGuids.Add(caps.ProductName, caps.ProductGuid);
            }

            SoundDeviceGuids = deviceGuids;
        }

        private void InitMidiDevices()
        {
            MidiDeviceMap = new Dictionary<string, string>()
            {
                { "General MIDI", "GENERAL_MIDI" },
                { "Yamaha XG MIDI", "YAMAHA_XG" },
                { "Soundfont MIDI (Creative AWE32/AWE64)", "SOUNDFONT_MIDI" }
            };
        }

        private void InitRenderers()
        {
            RendererMap = new Dictionary<string, int>()
            {
                { "Software Renderer", 0 },
                { "Direct3D Hardware Acceleration", 1 },
                { "Custom Driver", 3 }
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



                _audioTest = new WaveOut
                {
                    DeviceNumber = GetSelectedSoundDeviceNumber(),
                    Volume = (float)VolumeValue / (float)100.0
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

                if (caps.ProductGuid == selectedGuid)
                    return n;
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
        /// Shows warning message dialog to user if <see cref="SelectedRenderer"/> is not set to "Custom Driver"
        /// </summary>
        private void ShowWarningMessageAboutRenderer()
        {
            if (!SelectedRenderer.Equals("Custom Driver", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageDialogWindow.Show("Choosing any other option beside 'Custom Driver' will cause mods installed with 7H not to work anymore.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ShowWarningMessageAbouReunion()
        {
            if (GameLauncher.IsReunionModInstalled() && !DisableReunionChecked)
            {
                string warningMsg = "Reunion R06 and newer, even when disabled in Options.ini, forces a custom game driver to load when you run FF7. This conflicts with 7th Heaven's game driver, breaks your graphics settings, and you will experience problems!\n\nIf you wish to play Reunion, do so using a compatible modded version built for 7th Heaven.\n\nAre you sure?";
                var result = MessageDialogWindow.Show(warningMsg, "You should leave this setting ON!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result.Result == MessageBoxResult.No)
                {
                    // re-enable option as user selected 'No' in warning message so reverting option
                    DisableReunionChecked = true;
                }
            }
        }

    }

    /// <summary>
    /// A wave provider that plays no sound (used to test audio channels)
    /// </summary>
    public class SilenceWaveProvider : IWaveProvider
    {
        private WaveFormat _waveFormat;

        public SilenceWaveProvider(WaveFormat waveFormat)
        {
            this._waveFormat = waveFormat;
        }

        public WaveFormat WaveFormat => _waveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            // the silenced wave provider will return 0 which indicates playback stopped 
            // ... because PlaybackStopped will be fired once ALL input audio sources are done streaming [which is indicated when Read() returns 0]
            return 0; 
        }
    }
}
