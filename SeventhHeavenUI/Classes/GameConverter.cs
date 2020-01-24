using _7thHeaven.Code;
using Iros._7th.Workshop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeventhHeaven.Classes
{
    /// <summary>
    /// This is holds the main logic for converting the FF7 game to work with 7th Heaven
    /// </summary>
    public class GameConverter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void OnMessageSent(string message, NLog.LogLevel logLevel);
        public event OnMessageSent MessageSent;

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

        public static string GetInstallLocation(FF7Version installedVersion)
        {
            string installPath = null;

            switch (installedVersion)
            {
                case FF7Version.Unknown:
                    return "";

                case FF7Version.Steam:

                    if (Environment.Is64BitOperatingSystem)
                    {
                        // on 64-bit OS, Steam release registry key could be at 64bit path or 32bit path so check both
                        installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath32Bit, "InstallLocation", "") as string;

                        if (string.IsNullOrEmpty(installPath))
                        {
                            installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath64Bit, "InstallLocation", "") as string;
                        }
                    }
                    else
                    {
                        installPath = RegistryHelper.GetValue(RegistryHelper.SteamKeyPath32Bit, "InstallLocation", "") as string;
                    }

                    return installPath;

                case FF7Version.ReRelease:
                    // check 32-bit then 64-bit registry if not exists
                    installPath = RegistryHelper.GetValue(RegistryHelper.RereleaseKeyPath32Bit, "InstallLocation", "") as string;

                    if (string.IsNullOrWhiteSpace(installPath))
                    {
                        installPath = RegistryHelper.GetValue(RegistryHelper.RereleaseKeyPath64Bit, "InstallLocation", "") as string;
                    }

                    return installPath;

                case FF7Version.Original98:
                    return RegistryHelper.GetValue(FF7RegKey.FF7AppKeyPath, "Path", "") as string;

                default:
                    return "";
            }
        }

        public bool IsGamePirated()
        {
            string[] foldersToExclude = new string[] { "The_Reunion", "mods", "direct" }; // folders to skip in check

            // check all folders at root of InstallPath (excluding some)
            foreach (string subfolder in Directory.GetDirectories(InstallPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(subfolder);

                if (foldersToExclude.Any(f => dirInfo.Name.Equals(f, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue; // don't check these folders for signs of pirated files
                }

                bool isPirated = DirectoryHasPirates(subfolder);
                if (isPirated)
                {
                    return true;
                }
            }

            // check files at root of InstallPath
            foreach (string file in Directory.GetFiles(InstallPath))
            {
                bool isPirated = IsFileOrFolderAPirate(file);
                if (isPirated)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks all files, folders, and sub-folders for signs of pirated files 
        /// </summary>
        /// <param name="folderPath"> folder to loop over and check </param>
        private bool DirectoryHasPirates(string folderPath)
        {

            string[] filesToAllow = new string[] { "00422 [F - Crackling fire, looped].ogg" };

            foreach (string fileEntry in Directory.GetFileSystemEntries(folderPath, "*", SearchOption.AllDirectories))
            {
                FileInfo info = new FileInfo(fileEntry);

                if (fileEntry.IndexOf("torrent", StringComparison.InvariantCultureIgnoreCase) >= 0 && fileEntry.IndexOf("Reunion", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    continue; // allow Reunion torrent files
                }

                if (filesToAllow.Any(f => info.Name.Equals(f, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue; // this file has been marked as allowed by us so we skip the file
                }

                bool isPirated = IsFileOrFolderAPirate(fileEntry);

                if (isPirated)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the given folder or file is pirated by checking against specific keywords.
        /// </summary>
        private bool IsFileOrFolderAPirate(string pathToFileOrFolder)
        {
            string[] pirateKeyWords = new string[] { "crack", "warez", "torrent", "skidrow", "goodies" }; // folders and keywords usually found in files when the game is pirated
            string[] pirateExtensions = new string[] { ".nfo" };                                          // file extensions that indicate the game could be pirated
            string[] pirateExactKeywords = new string[] { "ali213.ini", "rld.dll", "gameservices.dll" };  // files that indicate the game is pirated

            string name;
            string ext;

            if (Directory.Exists(pathToFileOrFolder))
            {
                name = new DirectoryInfo(pathToFileOrFolder).Name;
                ext = new DirectoryInfo(pathToFileOrFolder).Extension;
            }
            else
            {
                name = new FileInfo(pathToFileOrFolder).Name;
                ext = new FileInfo(pathToFileOrFolder).Extension;
            }


            if (pirateExactKeywords.Any(s => s.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            if (pirateExtensions.Any(s => s == ext))
            {
                return true;
            }

            if (pirateKeyWords.Any(s => pathToFileOrFolder.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) >= 0))
            {
                return true;
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

        internal bool BackupExe(string backupFolderPath)
        {
            Directory.CreateDirectory(backupFolderPath);

            string ff7ExePath = Path.Combine(InstallPath, "ff7.exe");

            try
            {
                if (File.Exists(ff7ExePath))
                {
                    File.Copy(ff7ExePath, Path.Combine(backupFolderPath, "ff7.exe"), true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        internal bool BackupFF7ConfigExe(string backupFolderPath)
        {
            string ff7ConfigPath = Path.Combine(InstallPath, "FF7Config.exe");

            try
            {
                if (File.Exists(ff7ConfigPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                    File.Copy(ff7ConfigPath, Path.Combine(backupFolderPath, "FF7Config.exe"), true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

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
                    Directory.Delete(fullPath, true);
                }
            }
        }

        public void MoveOriginalAppFilesToBackup(string pathToBackup)
        {
            Directory.CreateDirectory(pathToBackup);

            List<string> filesToMove = new List<string>() { "app.log", "ff7.exe", "ff7config.exe", "RunFFVIIConfig.bat", "RunFFVIIConfig.exe", "ff7_mo.exe", "ff7_nt.exe", "ff7_ss.exe", "ff7_ss_safer.exe", "ff7_bc.exe", "ff7input.cfg", "Multi_Readme.txt", "cfg.log", "Hext.log", "FF7_GC.log", "eax.dll", "Hext.dll", "multi.dll", "ff7_opengl.cfg", "ff7_opengl.pdb", "ff7_opengl.reg", "crash.dmp", "ff7_opengl.fgd", @"\plugins\ff7music.fgp", @"\plugins\ffmpeg_movies.fgp", @"\plugins\vgmstream_music.fgp" };

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
        /// Delete all cache files (S*D.P and T*D.P files) in <see cref="InstallPath"/>
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

            string ff7ExePath = Path.Combine(Sys.PathToProvidedExe, "ff7.exe");

            try
            {
                File.Copy(ff7ExePath, Path.Combine(InstallPath, "ff7.exe"), true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public bool CopyFF7ConfigExeToGame()
        {
            if (!Directory.Exists(InstallPath))
            {
                return false;
            }

            string ff7ConfigPath = Path.Combine(Sys.PathToProvidedExe, "FF7Config.exe");

            try
            {
                File.Copy(ff7ConfigPath, Path.Combine(InstallPath, "FF7Config.exe"), true);
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

        /// <summary>
        /// Verifies a FF7 install is a Full/Max install by checking if specific files are in the game dir.
        /// They will automatically be copied from discs if not found. 
        /// Returns false if failed to find/copy all files
        /// </summary>
        /// <returns> Returns true if all files found and/or copied; false otherwise </returns>
        public bool VerifyFullInstallation()
        {
            bool foundAllFiles = true;

            string[] expectedFiles = new string[]
            {
                @"wm\world_us.lgp",
                @"field\char.lgp",
                @"field\flevel.lgp",
                @"minigame\chocobo.lgp",
                @"minigame\coaster.lgp",
                @"minigame\condor.lgp",
                @"minigame\high-us.lgp",
                @"minigame\snowboard-us.lgp",
                @"minigame\sub.lgp"
            };

            string[] volumeLabels = new string[]
            {
                "ff7install",
                "ff7disc1",
                "ff7disc2",
                "ff7disc3"
            };

            foreach (string file in expectedFiles)
            {
                string fullTargetPath = Path.Combine(InstallPath, "data", file);

                if (File.Exists(fullTargetPath))
                {
                    // file already exists at install path so continue
                    continue;
                }

                SendMessage($"...\t {file} not found. Scanning disc(s) for files ...", NLog.LogLevel.Warn);

                // search all drives for the file
                bool foundFileOnDrive = false;
                foreach (string label in volumeLabels)
                {
                    string driveLetter = GameLauncher.GetDriveLetter(label);

                    if (!string.IsNullOrWhiteSpace(driveLetter))
                    {
                        string fullSourcePath = Path.Combine(driveLetter, "ff7", file);
                        if (label == "ff7install")
                        {
                            fullSourcePath = Path.Combine(driveLetter, "data", file); // ff7install disc has a different path then disc1-3
                        }


                        if (File.Exists(fullSourcePath))
                        {
                            foundFileOnDrive = true;
                            SendMessage($"... \t found file on {label} at {driveLetter}. Copying file ...");
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(fullTargetPath)); // ensure all subfolders are created
                                File.Copy(fullSourcePath, fullTargetPath, true);
                                SendMessage($"... \t\t copied!");
                            }
                            catch (Exception e)
                            {
                                Logger.Error(e);
                                SendMessage($"... \t\t failed to copy. Error has been logged", NLog.LogLevel.Warn);
                            }
                        }
                    }

                    if (foundFileOnDrive)
                    {
                        break; // done searching drives as file found/copied
                    }
                }

                // at this point if file not found/copied on any drive then failed verification
                if (!foundFileOnDrive)
                {
                    SendMessage($"... \t failed to find {file} on any disc ...", NLog.LogLevel.Warn);
                    foundAllFiles = false;
                }
            }

            return foundAllFiles;
        }

        /// <summary>
        /// Verifies specific files exist in /data/[subfolder] where [subfolder] is battle, kernel, and movies.
        /// If files not found then they are copied from /data/lang-en/[subfolder
        /// </summary>
        /// <returns></returns>
        internal bool VerifyAdditionalFilesExist()
        {
            string[] expectedFiles = new string[]
            {
                @"battle\camdat0.bin",
                @"battle\camdat1.bin",
                @"battle\camdat2.bin",
                @"battle\co.bin",
                @"battle\scene.bin",
                @"kernel\KERNEL.BIN",
                @"kernel\kernel2.bin",
                @"kernel\WINDOW.BIN"
            };

            foreach (string file in expectedFiles)
            {
                string fullTargetPath = Path.Combine(InstallPath, "data", file);

                if (File.Exists(fullTargetPath))
                {
                    continue; // file exists as expected
                }

                SendMessage($"... \t{file} file not found", NLog.LogLevel.Warn);

                string fullSourcePath = Path.Combine(InstallPath, "data", "lang-en", file);
                if (!File.Exists(fullSourcePath))
                {
                    SendMessage($"... \tcannot copy source file because it is missing at {fullSourcePath}", NLog.LogLevel.Warn);
                    return false;
                }


                try
                {
                    SendMessage($"... \tcopying file from {fullSourcePath}");
                    File.Copy(fullSourcePath, fullTargetPath, true);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    SendMessage($"... \tfailed to copy: {e.Message}", NLog.LogLevel.Warn);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if all movies exist at 
        /// </summary>
        /// <returns></returns>
        internal static bool AllMovieFilesExist(string rootFolder)
        {
            foreach (string file in GetMovieFiles().Keys)
            {
                string fullPath = Path.Combine(rootFolder, file);

                if (!File.Exists(fullPath))
                {
                    return false;
                }
            }

            return true;
        }

        internal static Dictionary<string, string[]> GetMissingMovieFiles(string rootFolder)
        {
            Dictionary<string, string[]> missingMovies = new Dictionary<string, string[]>();

            foreach (var movie in GetMovieFiles())
            {
                string fullPath = Path.Combine(rootFolder, movie.Key);

                if (!File.Exists(fullPath))
                {
                    missingMovies.Add(movie.Key, movie.Value);
                }
            }

            return missingMovies;
        }

        internal bool CopyMovieFilesToFolder(string movieFolder)
        {
            var movieFiles = GetMovieFiles();

            List<string> missingFiles = new List<string>();
            bool copiedAllFiles = true;

            foreach (string file in movieFiles.Keys)
            {
                string fullPath = Path.Combine(movieFolder, file);

                if (File.Exists(fullPath))
                {
                    continue; // no need to copy file as it exists as expected
                }

                // at this point file does not exist in movies folder so check disc(s) for file

                if (file == "ending2.avi" || file == "jenova_e.avi")
                {
                    // special exception for these two files; check if they exist at other location and copy them 
                    string otherPath = Path.Combine(new string[] { InstallPath, "data", "lang-en", "movies", file });
                    if (File.Exists(otherPath))
                    {
                        SendMessage($"\tcopying {otherPath} to {fullPath}");
                        File.Copy(otherPath, fullPath, true);
                        continue;
                    }
                }

                // check for all possible discs for file and copy if found
                bool copiedFile = false;
                foreach (string disc in movieFiles[file])
                {
                    string driveLetter = GameLauncher.GetDriveLetter(disc);

                    if (string.IsNullOrEmpty(driveLetter))
                    {
                        continue;
                    }

                    string sourceFilePath = Path.Combine(driveLetter, "ff7", "movies", file);
                    if (File.Exists(sourceFilePath))
                    {
                        SendMessage($"\tcopying from disc '{disc}': {sourceFilePath} to {fullPath}");
                        File.Copy(sourceFilePath, fullPath, true);
                        copiedFile = true;
                        break;
                    }
                }

                if (!copiedFile)
                {
                    missingFiles.Add($"\t - {file} on {string.Join(",", movieFiles[file])}");
                    copiedAllFiles = false; // fail to find/copy file from disc(s); continue to search/copy other files so all missing files can be listed
                }
            }

            if (!copiedAllFiles)
            {
                SendMessage("\tOne or more movie files were not found", NLog.LogLevel.Warn);
            }

            return copiedAllFiles;
        }

        public void SendMessage(string message, NLog.LogLevel logLevel = null)
        {
            if (logLevel == null)
            {
                logLevel = NLog.LogLevel.Info;
            }

            MessageSent?.Invoke(message, logLevel);
        }

        public bool AllMusicFilesExist()
        {
            bool allFilesExist = true;

            Directory.CreateDirectory(Path.Combine(InstallPath, "music", "vgmstream")); // ensure music and music/vgmstream folders exist

            foreach (string file in GetMusicFiles())
            {
                string fullPath = Path.Combine(InstallPath, "music", "vgmstream", file);

                if (!File.Exists(fullPath))
                {
                    allFilesExist = false;
                }
            }

            if (!allFilesExist)
            {
                SendMessage("\tOne or more music files are missing.", NLog.LogLevel.Warn);
            }

            return allFilesExist;
        }

        public void CopyMusicFiles()
        {
            Directory.CreateDirectory(Path.Combine(InstallPath, "music", "vgmstream")); // ensure music and music/vgmstream folders exist

            foreach (string file in GetMusicFiles())
            {
                string fullTargetPath = Path.Combine(InstallPath, "music", "vgmstream", file);

                if (File.Exists(fullTargetPath))
                {
                    continue;
                }

                string sourcePath = Path.Combine(InstallPath, "data", "music_ogg", file);

                if (!File.Exists(sourcePath))
                {
                    continue; // source music file so skip over copying
                }

                try
                {
                    SendMessage($"\tcopying music file {sourcePath} to {fullTargetPath}");
                    File.Copy(sourcePath, fullTargetPath, true);
                }
                catch (Exception e)
                {
                    Logger.Warn(e); // log error but don't halt copying of files
                }
            }
        }

        public string[] GetMusicFiles()
        {
            return new string[]
            {
                "aseri.ogg",
                "aseri2.ogg",
                "ayasi.ogg",
                "barret.ogg",
                "bat.ogg",
                "bee.ogg",
                "bokujo.ogg",
                "boo.ogg",
                "cannon.ogg",
                "canyon.ogg",
                "cephiros.ogg",
                "chase.ogg",
                "chu.ogg",
                "chu2.ogg",
                "cinco.ogg",
                "cintro.ogg",
                "comical.ogg",
                "condor.ogg",
                "corel.ogg",
                "corneo.ogg",
                "costa.ogg",
                "crlost.ogg",
                "crwin.ogg",
                "date.ogg",
                "dokubo.ogg",
                "dun2.ogg",
                "earis.ogg",
                "earislo.ogg",
                "elec.ogg",
                "fan2.ogg",
                "fanfare.ogg",
                "fiddle.ogg",
                "fin.ogg",
                "geki.ogg",
                "gold1.ogg",
                "guitar2.ogg",
                "gun.ogg",
                "hen.ogg",
                "hiku.ogg",
                "horror.ogg",
                "iseki.ogg",
                "jukai.ogg",
                "junon.ogg",
                "jyro.ogg",
                "ketc.ogg",
                "kita.ogg",
                "kurai.ogg",
                "lb1.ogg",
                "lb2.ogg",
                "ld.ogg",
                "makoro.ogg",
                "mati.ogg",
                "mekyu.ogg",
                "mogu.ogg",
                "mura1.ogg",
                "nointro.ogg",
                "oa.ogg",
                "ob.ogg",
                "odds.ogg",
                "over2.ogg",
                "parade.ogg",
                "pj.ogg",
                "pre.ogg",
                "red.ogg",
                "rhythm.ogg",
                "riku.ogg",
                "ro.ogg",
                "rocket.ogg",
                "roll.ogg",
                "rukei.ogg",
                "sadbar.ogg",
                "sadsid.ogg",
                "sea.ogg",
                "seto.ogg",
                "si.ogg",
                "sid2.ogg",
                "sido.ogg",
                "siera.ogg",
                "sinra.ogg",
                "sinraslo.ogg",
                "snow.ogg",
                "ta.ogg",
                "tb.ogg",
                "tender.ogg",
                "tifa.ogg",
                "tm.ogg",
                "utai.ogg",
                "vincent.ogg",
                "walz.ogg",
                "weapon.ogg",
                "yado.ogg",
                "yufi.ogg",
                "yufi2.ogg",
                "yume.ogg"
            };
        }

        public static Dictionary<string, string[]> GetMovieFiles()
        {
            string[] disc1 = new string[] { "ff7disc1" };
            string[] disc2 = new string[] { "ff7disc2" };
            string[] disc3 = new string[] { "ff7disc3" };
            string[] allDiscs = new string[] { "ff7disc1", "ff7disc2", "ff7disc3" };

            return new Dictionary<string, string[]>()
            {
                { "biglight.avi", disc2},
                { "bike.avi", disc1},
                { "biskdead.avi", disc1},
                { "boogdemo.avi", disc1},
                { "boogdown.avi", allDiscs},
                { "boogstar.avi", disc1},
                { "boogup.avi", allDiscs},
                { "brgnvl.avi", disc1},
                { "c_scene1.avi", disc2},
                { "c_scene2.avi", disc2},
                { "c_scene3.avi", disc2},
                { "canon.avi", disc2},
                { "canonh1p.avi", disc2},
                { "canonh3f.avi", disc2},
                { "canonht0.avi", disc2},
                { "canonht1.avi", disc2},
                { "canonht2.avi", disc2},
                { "canonon.avi", disc2},
                { "car_1209.avi", disc1},
                { "d_ropego.avi", allDiscs},
                { "d_ropein.avi", allDiscs},
                { "dumcrush.avi", disc2},
                { "earithdd.avi", disc1},
                { "eidoslogo.avi", allDiscs},
                { "ending1.avi", disc3},
                { "ending2.avi", disc3},
                { "ending3.avi", disc3},
                { "Explode.avi", allDiscs},
                { "fallpl.avi", disc1},
                { "fcar.avi", disc3},
                { "feelwin0.avi", disc2},
                { "feelwin1.avi", disc2},
                { "fship2.avi", allDiscs},
                { "funeral.avi", disc1},
                { "gelnica.avi", disc2},
                { "gold1.avi", disc1},
                { "gold2.avi", allDiscs},
                { "gold3.avi", allDiscs},
                { "gold4.avi", allDiscs},
                { "gold5.avi", allDiscs},
                { "gold6.avi", allDiscs},
                { "gold7.avi", disc1},
                { "gold7_2.avi", disc1},
                { "greatpit.avi", disc2},
                { "hiwind0.avi", disc1},
                { "hwindfly.avi", disc2},
                { "hwindjet.avi", disc2},
                { "jairofal.avi", disc1},
                { "jairofly.avi", disc1},
                { "jenova_e.avi", disc1},
                { "junair_d.avi", allDiscs},
                { "junair_u.avi", allDiscs},
                { "junelego.avi", allDiscs},
                { "junelein.avi", allDiscs},
                { "junin_go.avi", allDiscs},
                { "junin_in.avi", allDiscs},
                { "junon.avi", disc1},
                { "junsea.avi", disc2},
                { "last4_2.avi", disc3},
                { "last4_3.avi", disc3},
                { "last4_4.avi", disc3},
                { "lastflor.avi", disc3},
                { "lastmap.avi", disc3},
                { "loslake1.avi", disc2},
                { "lslmv.avi", disc2},
                { "mainplr.avi", disc1},
                { "meteofix.avi", disc2},
                { "meteosky.avi", disc2},
                { "mk8.avi", disc1},
                { "mkup.avi", disc1},
                { "monitor.avi", new string[] { "ff7disc1", "ff7disc2" } },
                { "mtcrl.avi", disc1},
                { "mtnvl.avi", disc1},
                { "mtnvl2.avi", disc1},
                { "nivlsfs.avi", disc1},
                { "northmk.avi", disc1},
                { "nrcrl.avi", disc2},
                { "nrcrl_b.avi", disc2},
                { "nvlmk.avi", disc1},
                { "ontrain.avi", disc1},
                { "opening.avi", disc1},
                { "parashot.avi", disc2},
                { "phoenix.avi", disc2},
                { "plrexp.avi", disc1},
                { "rckethit0.avi", disc2},
                { "rckethit1.avi", disc2},
                { "rcketoff.avi", disc2},
                { "rcktfail.avi", disc1},
                { "setogake.avi", disc1},
                { "smk.avi", disc1},
                { "southmk.avi", disc1},
                { "sqlogo.avi", allDiscs},
                { "u_ropego.avi", allDiscs},
                { "u_ropein.avi", allDiscs},
                { "weapon0.avi", disc2},
                { "weapon1.avi", disc2},
                { "weapon2.avi", disc2},
                { "weapon3.avi", disc2},
                { "weapon4.avi", disc2},
                { "weapon5.avi", disc2},
                { "wh2e2.avi", disc2},
                { "white2.avi", new string[] { "ff7disc2", "ff7disc3" } },
                { "zmind01.avi", disc2},
                { "zmind02.avi", disc2},
                { "zmind03.avi", disc2}
            };
        }

        /// <summary>
        /// Checks ff7_opengl.fgd is up to date and matches file in Resources/Game Driver/ folder.
        /// If files are different then game driver files are copied to ff7 install path
        /// </summary>
        /// <returns>returns false if error occurred</returns>
        internal bool InstallLatestGameDriver(string backupFolderPath)
        {
            string pathToCurrentFile = Path.Combine(InstallPath, "ff7_opengl.fgd");
            string pathToLatestFile = Path.Combine(Sys.PathToGameDriverFolder, "ff7_opengl.fgd");
            string pathToLatestCfg = Path.Combine(Sys.PathToGameDriverFolder, "ff7_opengl.cfg");
            string pathToCurrentCfg = Path.Combine(InstallPath, "ff7_opengl.cfg");

            // copy default .cfg if it is missing
            if (!File.Exists(pathToCurrentCfg))
            {
                if (File.Exists(pathToLatestCfg))
                {
                    SendMessage($"\tff7_opengl.cfg file is missing. Copying default from {Sys.PathToGameDriverFolder} ...", NLog.LogLevel.Warn);
                    File.Copy(pathToLatestCfg, pathToCurrentCfg, true);
                }
                else
                {
                    SendMessage($"\tcannot create default .cfg due to missing file: {pathToLatestCfg} ...", NLog.LogLevel.Error);
                    return false;
                }
            }

            if (!File.Exists(pathToLatestFile))
            {
                SendMessage($"cannot check if latest driver is installed due to missing file: {pathToLatestFile}", NLog.LogLevel.Warn);
                return true; // return true so it does not halt process
            }

            if (CompareOpenGlDriverVersions(pathToCurrentFile, pathToLatestFile) >= 0)
            {
                SendMessage("\tff7_opengl.fgd file is up to date.");
                return true; // file exist and matches what is in /Game Driver folder
            }

            try
            {
                if (File.Exists(pathToCurrentFile))
                {
                    Directory.CreateDirectory(backupFolderPath);
                    SendMessage($"\tbacking up existing game driver to {backupFolderPath} ...");
                    File.Copy(pathToCurrentFile, Path.Combine(backupFolderPath, "ff7_opengl.fgd"), true);
                }

                if (File.Exists(pathToCurrentCfg))
                {
                    SendMessage($"\tbacking up existing game driver .cfg to {backupFolderPath} ...");
                    File.Copy(pathToCurrentCfg, Path.Combine(backupFolderPath, "ff7_opengl.cfg"), true);
                }

                SendMessage($"\tbacking up existing plugins folder to {backupFolderPath} ...");
                Directory.CreateDirectory(Path.Combine(backupFolderPath, "plugins"));
                FileUtils.CopyDirectoryRecursively(Path.Combine(InstallPath, "plugins"), Path.Combine(backupFolderPath, "plugins"));

                SendMessage($"\tbacking up existing shaders folder to {backupFolderPath} ...");
                Directory.CreateDirectory(Path.Combine(backupFolderPath, "shaders"));
                FileUtils.CopyDirectoryRecursively(Path.Combine(InstallPath, "shaders"), Path.Combine(backupFolderPath, "shaders"));

                SendMessage($"\tcopying contents of {Sys.PathToGameDriverFolder} to {InstallPath} ...");
                FileUtils.CopyDirectoryRecursively(Sys.PathToGameDriverFolder, InstallPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Does a regex on the ff7_opengl.fgd file to find the opengl driver version.
        /// Returns -1 if <paramref name="fileOne"/> version is older than <paramref name="fileTwo"/> version.
        /// Returns 0 if versions are equal.
        /// Returns 1 if <paramref name="fileOne"/> version is newer than <paramref name="fileTwo"/> version.
        /// </summary>
        /// <param name="fileOne"></param>
        /// <param name="fileTwo"></param>
        /// <returns></returns>
        private int CompareOpenGlDriverVersions(string fileOne, string fileTwo)
        {
            Regex regex = new Regex(@"FF7\/FF8 OpenGL driver version [0-9].[0-9].[0-9].*\n");

            if (!File.Exists(fileOne) && File.Exists(fileTwo))
            {
                return -1;
            }
            else if (File.Exists(fileOne) && !File.Exists(fileTwo))
            {
                return 1;
            }

            try
            {
                string fileOneContents = File.ReadAllText(fileOne);
                string fileTwoContents = File.ReadAllText(fileTwo);

                Match fileOneMatch = regex.Match(fileOneContents);
                Match fileTwoMatch = regex.Match(fileTwoContents);

                string fileOneVersion = "";
                string fileTwoVersion = "";

                if (fileOneMatch.Success)
                {
                    fileOneVersion = fileOneMatch.Value.TrimEnd('\n').Split(' ').LastOrDefault();
                    fileOneVersion = Regex.Replace(fileOneVersion, "[^0-9.]", ""); // we must strip alpha characters from the version since we can't compare against that. e.g. "0.8.1b" becomes "0.8.1"

                    if (fileOneVersion.Count(c => c == '.') < 3) // we must ensure all four parts of a Version are there so "0.8.1" becomes "0.8.1.0"
                    {
                        fileOneVersion += ".0";
                    }
                }

                if (fileTwoMatch.Success)
                {
                    fileTwoVersion = fileTwoMatch.Value.TrimEnd('\n').Split(' ').LastOrDefault();
                    fileTwoVersion = Regex.Replace(fileTwoVersion, "[^0-9.]", "");

                    if (fileTwoVersion.Count(c => c == '.') < 3)
                    {
                        fileTwoVersion += ".0";
                    }
                }

                Version parsedVersionOne = new Version("0.0.0.0");
                Version parsedVersionTwo = new Version("0.0.0.0");

                if (!string.IsNullOrEmpty(fileOneVersion))
                {
                    parsedVersionOne = new Version(fileOneVersion);
                }

                if (!string.IsNullOrEmpty(fileTwoVersion))
                {
                    parsedVersionTwo = new Version(fileTwoVersion);
                }


                return parsedVersionOne.CompareTo(parsedVersionTwo);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                SendMessage("\tfailed to get game driver version from files. Error has been logged.", NLog.LogLevel.Warn);
                return 0;
            }

        }

        /// <summary>
        /// Creates 'mods/Textures', 'direct' and subfolders, 'music', 'music/vgmstream' folders if missing
        /// </summary>
        public void CreateMissingDirectories()
        {
            string[] requiredFolders = new string[]
            {
                Sys.Settings.AaliFolder,
                Path.Combine(InstallPath, "direct"),
                Path.Combine(InstallPath, "direct", "battle"),
                Path.Combine(InstallPath, "direct", "char"),
                Path.Combine(InstallPath, "direct", "chocobo"),
                Path.Combine(InstallPath, "direct", "coaster"),
                Path.Combine(InstallPath, "direct", "condor"),
                Path.Combine(InstallPath, "direct", "cr"),
                Path.Combine(InstallPath, "direct", "disc"),
                Path.Combine(InstallPath, "direct", "flevel"),
                Path.Combine(InstallPath, "direct", "high"),
                Path.Combine(InstallPath, "direct", "magic"),
                Path.Combine(InstallPath, "direct", "menu"),
                Path.Combine(InstallPath, "direct", "midi"),
                Path.Combine(InstallPath, "direct", "moviecam"),
                Path.Combine(InstallPath, "direct", "snowboard"),
                Path.Combine(InstallPath, "direct", "sub"),
                Path.Combine(InstallPath, "direct", "world"),
                Path.Combine(InstallPath, "music"),
                Path.Combine(InstallPath, "music", "vgmstream"),
                Path.Combine(InstallPath, "shaders"),
                Path.Combine(InstallPath, "shaders", "nolight"),
                Path.Combine(InstallPath, "shaders", "ComplexMultiShader_Nvidia"),
                Path.Combine(InstallPath, "plugins")
            };

            foreach (string dir in requiredFolders)
            {
                if (!Directory.Exists(dir))
                {
                    Logger.Info($"\tdirectory missing. creating {dir}");
                    Directory.CreateDirectory(dir);
                }
            }
        }

        /// <summary>
        /// Checks registry for old game converter registry values. If found then backup of original game converter files and registry is taken.
        /// The old game converter registry key is also deleted.
        /// </summary>
        /// <param name="pathToBackup"></param>
        internal void CheckAndCopyOldGameConverterFiles(string pathToBackup)
        {
            string converterKeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII\\GameConverterkeys";


            string regValue = RegistryHelper.GetValue(converterKeyPath, "Manipulated_by_GameConverter", "") as string;

            if (!string.IsNullOrWhiteSpace(regValue))
            {
                SendMessage("Old Game Converter registry keys found. Backing up old converter files and registry ...");
                MoveOriginalAppFilesToBackup(pathToBackup);
                MoveOriginalConverterFilesToBackup(pathToBackup);
                BackupRegistry(pathToBackup);
                DeleteCacheFiles();

                RegistryHelper.DeleteKey(converterKeyPath);
            }
        }

        internal void CopyMissingPluginsAndShaders()
        {
            string pathToPlugins = Path.Combine(InstallPath, "plugins");
            string pathToShaders = Path.Combine(InstallPath, "shaders");
            string pathToNoLight = Path.Combine(InstallPath, "shaders", "nolight");
            string pathToNvidia = Path.Combine(InstallPath, "shaders", "ComplexMultiShader_Nvidia");


            // create missing directories
            if (!Directory.Exists(pathToNoLight))
            {
                SendMessage("\tmissing shaders/nolight folder. Creating directory ...");
                Directory.CreateDirectory(pathToNoLight);
            }

            if (!Directory.Exists(pathToNvidia))
            {
                SendMessage("\tmissing shaders/ComplexMultiShader_Nvidia folder. Creating directory ...");
                Directory.CreateDirectory(pathToNvidia);
            }

            if (!Directory.Exists(pathToShaders))
            {
                SendMessage("\tmissing shaders folder. Copying from Resources/Game Driver/ ...");
                Directory.CreateDirectory(pathToShaders);
            }

            if (!Directory.Exists(pathToPlugins))
            {
                SendMessage("\tmissing plugins folder. Creating directory ...");
                Directory.CreateDirectory(pathToPlugins);
            }

            // copy files from plugins and/or shaders folder if files are missing
            string sourcePath = Path.Combine(Sys.PathToGameDriverFolder, "shaders");
            string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

            foreach (string shaderFile in sourceFiles)
            {
                string targetPath = shaderFile.Replace(sourcePath, pathToShaders);

                if (!File.Exists(targetPath))
                {
                    SendMessage($"\tmissing shader file: {targetPath}. Copying from {sourcePath} ...");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)); // ensure any missing sub folders are created
                    File.Copy(shaderFile, targetPath, true);
                }
            }


            sourcePath = Path.Combine(Sys.PathToGameDriverFolder, "plugins");
            sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

            foreach (string pluginFile in sourceFiles)
            {
                string targetPath = pluginFile.Replace(sourcePath, pathToPlugins);

                if (!File.Exists(targetPath))
                {
                    SendMessage($"\tmissing plugin file: {targetPath}. Copying from {sourcePath} ...");
                    File.Copy(pluginFile, targetPath, true);
                }
            }
        }

        internal bool IsExeDifferent()
        {
            string ff7ExePath = Path.Combine(Sys.PathToProvidedExe, "ff7.exe");
            return !FileUtils.AreFilesEqual(ff7ExePath, Path.Combine(InstallPath, "ff7.exe"));
        }

        internal bool IsConfigExeDifferent()
        {
            string ff7ExePath = Path.Combine(Sys.PathToProvidedExe, "FF7Config.exe");
            return !FileUtils.AreFilesEqual(ff7ExePath, Path.Combine(InstallPath, "FF7Config.exe"));
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
