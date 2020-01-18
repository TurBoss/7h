using _7thHeaven.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    /// <summary>
    /// This is holds the main logic for converting the FF7 game to work with 7th Heaven
    /// </summary>
    public class GameConverter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string BackupFolderName = "BackupGC2020";

        public string InstallPath { get; set; }

        public ConversionSettings Settings { get; set; }

        public GameConverter(ConversionSettings settings)
        {
            InstallPath = settings.InstallPath;
            Settings = settings;
        }

        public static BoolWithMessage StartConversion(ConversionSettings settings)
        {
            GameConverter converter = new GameConverter(settings);
            string installPath = converter.InstallPath;

            if (!Directory.Exists(installPath))
            {
                return BoolWithMessage.False($"Path to Install does not exist: {installPath}");
            }

            // Check if game version installed is pirated
            if (converter.IsGamePirated())
            {
                Logger.Warn("Game detected to be not legitimate ...");

                // TODO - write list of files to log
                return BoolWithMessage.False("Cannot patch the game, the copy of the game does not seem legitimate. The list of files/folders have been logged to converter.log for troubleshooting.");
            }

            // Check game is installed in a system folder like Program Files or Windows
            if (converter.IsGameLocatedInSystemFolders())
            {
                Logger.Warn("Game detected to be located in system folders ...");

                if (settings.CopyGameFolder)
                {
                    Logger.Warn("\tattempting to copy game ...");

                    bool didCopy = converter.CopyGame(settings.CopiedGameTargetPath);

                    if (!didCopy)
                    {
                        return BoolWithMessage.False($"Failed to copy the game to {settings.CopiedGameTargetPath} ... Cannot continue patching.");
                    }

                    // update install path to new copied location
                    converter.InstallPath = settings.CopiedGameTargetPath;
                    installPath = converter.InstallPath;
                }
                else
                {
                    Logger.Warn("\tskipping copy game ...");
                    return BoolWithMessage.False("Cannot patch the game as it is installed in a system folder which can potentially cause some modding errors. Install the game in a location such as C:\\Games");
                }
            }

            // Backup registry and original converter files if 'BackupGC2020' folder does not exist
            try
            {
                string backupFolderPath = Path.Combine(converter.InstallPath, BackupFolderName, $"Backup_{DateTime.Now.ToString("yyyyMMddHHmmss")}");
                Logger.Warn($"Attempting backup of files and registry to {backupFolderPath} ...");

                converter.BackupRegistry(backupFolderPath);
                converter.MoveOriginalConverterFilesToBackup(backupFolderPath);
                converter.MoveOriginalAppFilesToBackup(backupFolderPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return BoolWithMessage.False("Failed to backup files and/or registry");
            }


            // cleanup install folder by removing cache files and old reg keys
            bool deletedCache = converter.DeleteCacheFiles();

            if (!deletedCache)
            {
                return BoolWithMessage.False("Failed to delete cache files from install path");
            }

            try
            {
                converter.DeleteOriginalConverterAndAppFiles();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return BoolWithMessage.False("Failed to delete old game converter and app files");
            }


            // Move OGG Music Files to /music/vgmstream
            if (converter.Settings.Version != FF7Version.Original98)
            {
                try
                {
                    FileUtils.MoveDirectoryRecursively(Path.Combine(installPath, "data", "music_ogg"), Path.Combine(installPath, "music", "vgmstream"));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    return BoolWithMessage.False("Failed to move music_ogg to music/vgmstream");
                }
            }


            // Remove Compatibility Flags
            try
            {
                DeleteCompatibilityFlagRegKeys();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return BoolWithMessage.False("Failed to delete compatibility flags set by old game converter");
            }


            // Copy standard "ff7.exe" and "ff7config.exe" to root install folder.
            bool didCopyFf7 = converter.CopyFF7ExeToGame();

            if (!didCopyFf7)
            {
                return BoolWithMessage.False("Failed to copy ff7.exe to install path");
            }


            // OpenGL Driver Install
            bool didCopyGl = converter.CopyGLDriversToGame();

            if (!didCopyGl)
            {
                return BoolWithMessage.False("Failed to copy open gl drivers to install path");
            }

            return BoolWithMessage.True();
        }

        public static FF7Version GetInstalledVersion()
        {
            string installPath = null;

            if (Environment.Is64BitOperatingSystem)
            {
                // on 64-bit OS, Steam release registry key could be at 64bit path or 32bit path so check both
                installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath64Bit, "InstallLocation", "") as string;

                if (string.IsNullOrEmpty(installPath))
                {
                    installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath32Bit, "InstallLocation", "") as string;
                }
            }
            else
            {
                installPath = RegistryHelper.GetValue(FF7RegKey.SteamKeyPath, "InstallLocation", "") as string;
            }

            if (!string.IsNullOrEmpty(installPath))
            {
                return FF7Version.Steam;
            }

            installPath = RegistryHelper.GetValue(FF7RegKey.RereleaseKeyPath, "InstallLocation", "") as string;
            if (!string.IsNullOrEmpty(installPath))
            {
                return FF7Version.ReRelease;
            }

            installPath = RegistryHelper.GetValue(FF7RegKey.FF7AppKeyPath, "Path", "") as string;
            if (!string.IsNullOrEmpty(installPath))
            {
                return FF7Version.Original98;
            }

            return FF7Version.Unknown;
        }

        public static string GetInstallLocation()
        {
            FF7Version installedVersion = GetInstalledVersion();

            if (installedVersion == FF7Version.Unknown)
            {
                return "";
            }

            switch (installedVersion)
            {
                case FF7Version.Unknown:
                    return "";

                case FF7Version.Steam:
                    string installPath = null;

                    if (Environment.Is64BitOperatingSystem)
                    {
                        // on 64-bit OS, Steam release registry key could be at 64bit path or 32bit path so check both
                        installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath64Bit, "InstallLocation", "") as string;

                        if (string.IsNullOrEmpty(installPath))
                        {
                            installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath32Bit, "InstallLocation", "") as string;
                        }
                    }
                    else
                    {
                        installPath = RegistryHelper.GetValue(FF7RegKey.SteamKeyPath, "InstallLocation", "") as string;
                    }

                    return installPath;

                case FF7Version.ReRelease:
                    return RegistryHelper.GetValue(FF7RegKey.RereleaseKeyPath, "InstallLocation", "") as string;
                case FF7Version.Original98:
                    return RegistryHelper.GetValue(FF7RegKey.FF7AppKeyPath, "Path", "") as string;
                default:
                    return "";
            }
        }

        public bool IsGamePirated()
        {
            List<string> pirateKeyWords = new List<string>() { "crack", "warez", "torrent", "skidrow", "goodies" }; // folders and keywords usually found in files when the game is pirated
            List<string> pirateExtensions = new List<string>() { ".nfo" };                                          // file extensions that indicate the game could be pirated
            List<string> pirateExactKeywords = new List<string>() { "ali213.ini", "rld.dll", "gameservices.dll" };  // files that indicate the game is pirated

            foreach (string file in Directory.GetFiles(InstallPath, "*", SearchOption.AllDirectories))
            {
                FileInfo info = new FileInfo(file);

                if (pirateExactKeywords.Any(s => s.Equals(info.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }

                if (pirateExtensions.Any(s => s == info.Extension))
                {
                    return true;
                }

                if (pirateKeyWords.Any(s => file.Contains(s)))
                {
                    return true;
                }
            }

            foreach (string dir in Directory.GetDirectories(InstallPath, "*", SearchOption.AllDirectories))
            {
                if (pirateKeyWords.Any(s => dir.Contains(s)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsGameLocatedInSystemFolders()
        {
            if (!Directory.Exists(InstallPath))
            {
                return false;
            }

            List<string> protectedFolders = new List<string>() { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                                                 Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                                                 Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                                 Environment.GetFolderPath(Environment.SpecialFolder.Windows) };

            return protectedFolders.Any(s => InstallPath.Contains(s));
        }

        public bool CopyGame(string targetPath = @"C:\Games\Final Fantasy VII")
        {
            if (!Directory.Exists(InstallPath))
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(targetPath);
                FileUtils.CopyDirectoryRecursively(InstallPath, targetPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }

            return true;
        }

        #region Backup And Cleanup Related Methods

        /// <summary>
        /// Backup registry keys to .reg files in 'BackupGC2020' folder
        /// </summary>
        /// <param name="installPath"></param>
        public void BackupRegistry(string pathToBackup)
        {
            Directory.CreateDirectory(pathToBackup);

            if (Environment.Is64BitOperatingSystem)
            {
                // check which registry key exists for the steam release (could be in 32-bit or 64-bit area
                if (RegistryHelper.GetValue(RegistryHelper.SteamKeyPath64Bit, "InstallLocation") != null)
                {
                    RegistryHelper.ExportKey(RegistryHelper.SteamKeyPath64Bit, Path.Combine(pathToBackup, "FF7-01.reg"));
                }
                else
                {
                    RegistryHelper.ExportKey(RegistryHelper.SteamKeyPath32Bit, Path.Combine(pathToBackup, "FF7-01.reg"));
                }
            }

            RegistryHelper.ExportKey(RegistryHelper.GetKeyPath(FF7RegKey.RereleaseKeyPath), Path.Combine(pathToBackup, "FF7-02.reg"));
            RegistryHelper.ExportKey(RegistryHelper.GetKeyPath(FF7RegKey.FF7AppKeyPath), Path.Combine(pathToBackup, "FF7-03.reg"));

            string oldGameConverterKeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII\\GameConverterkeys";
            RegistryHelper.ExportKey(oldGameConverterKeyPath, Path.Combine(pathToBackup, "FF7-OldGC.reg"));
        }

        public void MoveOriginalConverterFilesToBackup(string pathToBackup)
        {
            Directory.CreateDirectory(pathToBackup);

            List<string> foldersToMove = new List<string>() { "DLL_in", "Hext_in", "LOADR", "Multi_DLL", "FF7anyCDv2", "BackupGC" };

            foreach (string folder in foldersToMove)
            {
                string fullPath = Path.Combine(InstallPath, folder);
                if (Directory.Exists(fullPath))
                {
                    FileUtils.MoveDirectoryRecursively(fullPath, Path.Combine(pathToBackup, folder));
                }
            }
        }

        public void MoveOriginalAppFilesToBackup(string pathToBackup)
        {
            Directory.CreateDirectory(pathToBackup);

            List<string> filesToMove = new List<string>() { "app.log", "ff7.exe", "ff7config.exe", "RunFFVIIConfig.bat", "RunFFVIIConfig.exe", "ff7_mo.exe", "ff7_nt.exe", "ff7_ss.exe", "ff7_ss_safer.exe", "ff7_bc.exe", "ff7input.cfg", "Multi_Readme.txt", "cfg.log", "Hext.log", "FF7_GC.log", "eax.dll", "Hext.dll", "multi.dll", "ff7_opengl.cfg", "ff7_opengl.fgd", @"\plugins\ff7music.fgp", @"\plugins\ffmpeg_movies.fgp", @"\plugins\vgmstream_music.fgp" };

            foreach (string file in filesToMove)
            {
                string fullPath = Path.Combine(InstallPath, file);
                if (File.Exists(fullPath))
                {
                    File.Move(fullPath, Path.Combine(pathToBackup, file));
                }
            }

            // copy EasyHook related files
            foreach (string file in Directory.GetFiles(InstallPath, "EasyHook*.*"))
            {
                FileInfo info = new FileInfo(file);
                File.Move(file, Path.Combine(pathToBackup, info.Name));
            }
        }

        public void DeleteOriginalConverterAndAppFiles()
        {
            if (!Directory.Exists(InstallPath))
            {
                return;
            }

            List<string> filesToDelete = new List<string>() { "app.log", "ff7.exe", "ff7config.exe", "RunFFVIIConfig.bat", "RunFFVIIConfig.exe", "ff7_mo.exe", "ff7_nt.exe", "ff7_ss.exe", "ff7_ss_safer.exe", "ff7_bc.exe", "ff7input.cfg", "Multi_Readme.txt", "cfg.log", "Hext.log", "FF7_GC.log", "eax.dll", "Hext.dll", "multi.dll", "ff7_opengl.cfg", "ff7_opengl.fgd", @"\plugins\ff7music.fgp", @"\plugins\ffmpeg_movies.fgp", @"\plugins\vgmstream_music.fgp" };
            List<string> foldersToDelete = new List<string>() { "DLL_in", "Hext_in", "LOADR", "Multi_DLL", "FF7anyCDv2", "BackupGC" };

            foreach (string file in filesToDelete)
            {
                string fullPath = Path.Combine(InstallPath, file);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            foreach (string folder in foldersToDelete)
            {
                string fullPath = Path.Combine(InstallPath, folder);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                }
            }

            // delete EasyHook related files
            foreach (string file in Directory.GetFiles(InstallPath, "EasyHook*.*"))
            {
                File.Delete(file);
            }

            // delete Old GameConverter reg keys
            string oldGameConverterKeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII\\GameConverterkeys";
            RegistryHelper.DeleteKey(oldGameConverterKeyPath);
        }

        /// <summary>
        /// Delete all cache files (S*D.P and T*D.P files) in given folder
        /// </summary>
        public bool DeleteCacheFiles()
        {
            if (!Directory.Exists(InstallPath))
            {
                return true;
            }

            try
            {
                foreach (string file in Directory.GetFiles(InstallPath, "S*D.P"))
                {
                    File.Delete(file);
                }

                foreach (string file in Directory.GetFiles(InstallPath, "T*D.P"))
                {
                    File.Delete(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }


        }

        #endregion

        public static void DeleteCompatibilityFlagRegKeys()
        {
            string keyPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

            List<string> keyValuesToDelete = new List<string>() { "ff7.exe", "ff7config.exe", "ff7music.exe" };

            foreach (string valueName in RegistryHelper.GetValueNamesFromKey(keyPath))
            {
                if (keyValuesToDelete.Any(s => valueName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0))
                {
                    RegistryHelper.DeleteValueFromKey(keyPath, valueName);
                }
            }
        }

        public bool CopyFF7ExeToGame()
        {
            if (!Directory.Exists(InstallPath))
            {
                return false;
            }

            string ff7ExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ff7_1.02_eng", "ff7.exe");
            string ff7ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ff7_1.02_eng", "FF7Config.exe");

            try
            {
                File.Copy(ff7ExePath, Path.Combine(InstallPath, "ff7.exe"));
                File.Copy(ff7ConfigPath, Path.Combine(InstallPath, "FF7Config.exe"));
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public bool CopyGLDriversToGame()
        {
            string pathToPlugins = Path.Combine(InstallPath, "plugins");
            string pathToShaders = Path.Combine(InstallPath, "shaders");
            string sourceOpenGlFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenGL");

            if (!Directory.Exists(sourceOpenGlFolder) || !Directory.Exists(InstallPath))
            {
                return false;
            }

            try
            {
                // Create "plugins" folder in root install folder and copy all files under OpenGL driver\plugins folder to install folder.
                Directory.CreateDirectory(pathToPlugins);
                Directory.CreateDirectory(pathToShaders);

                FileUtils.CopyDirectoryRecursively(Path.Combine(sourceOpenGlFolder, "plugins"), pathToPlugins);
                FileUtils.CopyDirectoryRecursively(Path.Combine(sourceOpenGlFolder, "shaders"), pathToShaders);


                // Copy all ff7_opengl.* files to root install folder.
                foreach (string file in Directory.GetFiles(sourceOpenGlFolder, "ff7_opengl.*"))
                {
                    FileInfo info = new FileInfo(file);
                    File.Copy(file, Path.Combine(InstallPath, info.Name));
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }


    }

    public class ConversionSettings
    {
        public FF7Version Version { get; set; }

        public string InstallPath { get; set; }

        public bool DoBackup { get; set; }

        public bool DeleteReunionIfFound { get; set; }

        public Guid AudioDeviceGuid { get; set; }

        public string DriveLetter { get; set; }

        public bool CopyGameFolder { get; set; }

        public string CopiedGameTargetPath { get; set; }

        public bool UseLaptopKeyboardCfg { get; set; }
    }
}
