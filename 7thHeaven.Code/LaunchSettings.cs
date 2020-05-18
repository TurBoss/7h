using System;

namespace _7thHeaven.Code
{
    public enum MountDiscOption
    {
        Unknown = -1,
        MountWithPowerShell = 0,
        MountWithWinCDEmu = 1
    }

    [Serializable]
    public class LaunchSettings
    {
        public bool AutoMountGameDisc { get; set; }
        public bool AutoUnmountGameDisc { get; set; }
        public bool AutoUpdateDiscPath { get; set; }
        public MountDiscOption MountingOption { get; set; }

        public bool HighDpiFix { get; set; }
        public bool DisableReunionOnLaunch { get; set; }

        public bool ReverseSpeakers { get; set; }
        public bool LogarithmicVolumeControl { get; set; }
        public Guid SelectedSoundDevice { get; set; }
        public string SelectedMidiDevice { get; set; }

        public int SelectedRenderer { get; set; }
        public bool UseRiva128GraphicsOption { get; set; }
        public bool UseTntGraphicsOption { get; set; }
        public bool QuarterScreenMode { get; set; }
        public bool FullScreenMode { get; set; }
        public bool ShowLauncherWindow { get; set; }

        public bool HasDisplayedOggMusicWarning { get; set; }
        public bool HasDisplayedMovieWarning { get; set; }

        public bool EnablePs4ControllerService { get; set; }

        /// <summary>
        /// True means that the launcher will poll for input from a gamepad to intercept trigger/dpad presses
        /// </summary>
        public bool EnableGamepadPolling { get; set; }



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
                HighDpiFix = false,
                DisableReunionOnLaunch = true,
                SelectedSoundDevice = Guid.Empty,
                ReverseSpeakers = false,
                LogarithmicVolumeControl = true,
                SelectedMidiDevice = "GENERAL_MIDI",
                SelectedRenderer = 3,
                UseRiva128GraphicsOption = false,
                UseTntGraphicsOption = false,
                QuarterScreenMode = false,
                FullScreenMode = false,
                ShowLauncherWindow = true,
                InGameConfigOption = "[Default] Playstation+Steam KB.cfg",
                HasDisplayedOggMusicWarning = false,
                HasDisplayedMovieWarning = false,
                EnablePs4ControllerService = false,
                MountingOption = MountDiscOption.MountWithPowerShell,
                EnableGamepadPolling = true
            };
        }
    }
}
