using System;

namespace _7thHeaven.Code
{
    [Serializable]
    public class LaunchSettings
    {
        public bool AutoMountGameDisc { get; set; }
        public bool AutoUnmountGameDisc { get; set; }
        public bool AutoUpdateDiscPath { get; set; }

        public bool Code5Fix { get; set; }
        public bool HighDpiFix { get; set; }
        public bool DisableReunionOnLaunch { get; set; }

        public bool ReverseSpeakers { get; set; }
        public bool LogarithmicVolumeControl { get; set; }
        public Guid SelectedSoundDevice { get; set; }
        public string SelectedMidiDevice { get; set; }
        public int MusicVolume { get; set; }
        public int SfxVolume { get; set; }

        public int SelectedRenderer { get; set; }
        public bool UseRiva128GraphicsOption { get; set; }
        public bool UseTntGraphicsOption { get; set; }
        public bool QuarterScreenMode { get; set; }
        public bool FullScreenMode { get; set; }
        public bool HasReceivedCode5Error { get; set; }
        public bool ShowLauncherWindow { get; set; }

        /// <summary>
        /// File name of the ff7input.cfg file to copy to ff7 game dir 
        /// e.g. "stock game.cfg" or "custom.cfg"
        /// </summary>
        public string InGameConfigOption { get; set; }

        public static LaunchSettings DefaultSettings()
        {
            return new LaunchSettings()
            {
                AutoMountGameDisc = true,
                AutoUnmountGameDisc = true,
                AutoUpdateDiscPath = true,
                Code5Fix = false,
                HighDpiFix = false,
                DisableReunionOnLaunch = true,
                SelectedSoundDevice = Guid.Empty,
                ReverseSpeakers = false,
                LogarithmicVolumeControl = true,
                SelectedMidiDevice = "GENERAL_MIDI",
                SelectedRenderer = 3,
                MusicVolume = 100,
                SfxVolume = 100,
                UseRiva128GraphicsOption = false,
                UseTntGraphicsOption = false,
                QuarterScreenMode = false,
                FullScreenMode = false,
                HasReceivedCode5Error = false,
                ShowLauncherWindow = true,
                InGameConfigOption = "stock game.cfg"
            };
        }
    }
}
