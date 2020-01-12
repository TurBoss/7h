using _7thHeaven.Code;
using Iros._7th.Workshop;
using NAudio.Wave;
using SeventhHeaven.Classes;
using SeventhHeavenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.ViewModels
{
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
        private bool _isAutoMountSupported;
        private bool _isReverseSpeakersChecked;
        private bool _isLogVolumeChecked;
        private bool _isNvidiaOptionChecked;
        private bool _isNvidiaOptionEnabled;
        private bool _isRivaOptionChecked;
        private bool _isRivaOptionEnabled;
        private bool _isTntOptionEnabled;
        private bool _isTntOptionChecked;
        private string _selectedSoundDevice;
        private string _selectedMidiDevice;
        private string _selectedRenderer;
        private int _volumeValue;

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

            AutoUpdatePathChecked = Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath;

            Code5FixChecked = Sys.Settings.GameLaunchSettings.Code5Fix;
            HighDpiFixChecked = Sys.Settings.GameLaunchSettings.HighDpiFix;
            DisableReunionChecked = Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch;

            SelectedSoundDevice = SoundDeviceGuids.Where(s => s.Value == Sys.Settings.GameLaunchSettings.SelectedSoundDevice)
                                                  .Select(s => s.Key)
                                                  .FirstOrDefault();

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

        internal void SaveSettings()
        {
            Sys.Settings.GameLaunchSettings.AutoMountGameDisc = AutoMountChecked;
            Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc = AutoUnmountChecked;
            Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath = AutoUpdatePathChecked;

            Sys.Settings.GameLaunchSettings.Code5Fix = Code5FixChecked;
            Sys.Settings.GameLaunchSettings.HighDpiFix = HighDpiFixChecked;
            Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch = DisableReunionChecked;

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
    }
}
