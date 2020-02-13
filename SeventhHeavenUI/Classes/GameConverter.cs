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
    /// An instance of <see cref="GameConverter"/> is used to perform actions necessary to make FF7 compatible with mods.
    /// </summary>
    public class GameConverter
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public delegate void OnMessageSent(string message, NLog.LogLevel logLevel);
        public event OnMessageSent MessageSent;

        public const string BackupFolderName = "7H2.0-BACKUP";

        public string InstallPath { get; set; }

        public ConversionSettings Settings { get; set; }

        public GameConverter(ConversionSettings settings)
        {
            InstallPath = settings.InstallPath;
            Settings = settings;
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
                    string destPath = Path.Combine(pathToBackup, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Move(fullPath, destPath);
                }
            }

            // copy EasyHook related files
            foreach (string file in Directory.GetFiles(InstallPath, "EasyHook*.*"))
            {
                FileInfo info = new FileInfo(file);
                File.Move(file, Path.Combine(pathToBackup, info.Name));
            }
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
            foreach (string file in FF7FileLister.GetMovieFiles().Keys)
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

            foreach (var movie in FF7FileLister.GetMovieFiles())
            {
                string fullPath = Path.Combine(rootFolder, movie.Key);

                if (!File.Exists(fullPath))
                {
                    missingMovies.Add(movie.Key, movie.Value);
                }
            }

            return missingMovies;
        }

        /// <summary>
        /// Copies ending2.avi and jenova_e.avi to movies folder if they dont exist
        /// </summary>
        internal bool CopyMovieFilesToFolder(string movieFolder)
        {
            string[] movieFiles = new string[] { "ending2.avi", "jenova_e.avi" };
            bool copiedAllFiles = true;

            foreach (string file in movieFiles)
            {
                string fullPath = Path.Combine(movieFolder, file);

                if (File.Exists(fullPath))
                {
                    continue; // no need to copy file as it exists as expected
                }

                // check if they exist at data/lang-en/movies location and copy them 
                string otherPath = Path.Combine(new string[] { InstallPath, "data", "lang-en", "movies", file });
                if (File.Exists(otherPath))
                {
                    SendMessage($"\tcopying {otherPath} to {fullPath}");
                    File.Copy(otherPath, fullPath, true);
                    continue;
                }
                else
                {
                    copiedAllFiles = false;
                }
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

            foreach (string file in FF7FileLister.GetMusicFiles())
            {
                string fullPath = Path.Combine(InstallPath, "music", "vgmstream", file);

                if (!File.Exists(fullPath))
                {
                    allFilesExist = false;
                }
            }

            return allFilesExist;
        }

        public void CopyMusicFiles()
        {
            Directory.CreateDirectory(Path.Combine(InstallPath, "music", "vgmstream")); // ensure music and music/vgmstream folders exist

            foreach (string file in FF7FileLister.GetMusicFiles())
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

        /// <summary>
        /// Checks 7H_GameDriver.dll is up to date and matches file in Resources/Game Driver/ folder.
        /// If files are different then game driver files are copied to ff7 install path
        /// </summary>
        /// <returns>returns false if error occurred</returns>
        internal bool InstallLatestGameDriver(string backupFolderPath)
        {
            string pathToCurrentFile = Path.Combine(InstallPath, "7H_GameDriver.dll");
            string pathToLatestFile = Path.Combine(Sys.PathToGameDriverFolder, "7H_GameDriver.dll");
            string pathToLatestCfg = Path.Combine(Sys.PathToGameDriverFolder, "7H_GameDriver.cfg");
            string pathToCurrentCfg = Path.Combine(InstallPath, "7H_GameDriver.cfg");

            if (!File.Exists(pathToLatestFile))
            {
                SendMessage($"cannot check if latest driver is installed due to missing file: {pathToLatestFile}", NLog.LogLevel.Warn);
                return true; // return true so it does not halt process
            }

            FileVersionInfo currentFileVersion = null;
            if (File.Exists(pathToCurrentFile))
            {
                currentFileVersion = FileVersionInfo.GetVersionInfo(pathToCurrentFile);
            }

            FileVersionInfo latestFileVersion = FileVersionInfo.GetVersionInfo(pathToLatestFile);

            if (currentFileVersion != null && latestFileVersion != null && new Version(currentFileVersion.FileVersion).CompareTo(new Version(latestFileVersion.FileVersion)) >= 0)
            {
                SendMessage("\t7H_GameDriver.dll file is up to date.");
                return true; // file exist and matches what is in /Game Driver folder
            }

            try
            {
                if (File.Exists(pathToCurrentFile))
                {
                    Directory.CreateDirectory(backupFolderPath);
                    SendMessage($"\tbacking up existing game driver to {backupFolderPath} ...");
                    File.Copy(pathToCurrentFile, Path.Combine(backupFolderPath, "7H_GameDriver.dll"), true);
                }

                if (File.Exists(pathToCurrentCfg))
                {
                    Directory.CreateDirectory(backupFolderPath);
                    SendMessage($"\tbacking up existing game driver .cfg to {backupFolderPath} ...");
                    File.Copy(pathToCurrentCfg, Path.Combine(backupFolderPath, "7H_GameDriver.cfg"), true);
                }
                else
                {
                    // copy default .cfg if it is missing
                    if (File.Exists(pathToLatestCfg))
                    {
                        SendMessage($"\t7H_GameDriver.cfg file is missing. Copying default from {Sys.PathToGameDriverFolder} ...", NLog.LogLevel.Warn);
                        File.Copy(pathToLatestCfg, pathToCurrentCfg, true);
                    }
                    else
                    {
                        SendMessage($"\tcannot create default .cfg due to missing file: {pathToLatestCfg} ...", NLog.LogLevel.Error);
                        return false;
                    }
                }

                string pathToShaders = Path.Combine(InstallPath, "shaders");
                if (Directory.Exists(pathToShaders) && Directory.GetFiles(pathToShaders).Length > 0)
                {
                    SendMessage($"\tbacking up existing shaders folder to {backupFolderPath} ...");
                    Directory.CreateDirectory(Path.Combine(backupFolderPath, "shaders"));
                    FileUtils.CopyDirectoryRecursively(Path.Combine(InstallPath, "shaders"), Path.Combine(backupFolderPath, "shaders"));
                }

                // move old game driver (*.fgp) plugins to backup folder and delete plugins folder if empty
                string pathToPlugins = Path.Combine(InstallPath, "plugins");
                if (Directory.Exists(pathToPlugins))
                {
                    string[] pluginFiles = new string[] { "ff7music.fgp", "ffmpeg_movies.fgp", "vgmstream_music.fgp" };

                    foreach (string file in pluginFiles)
                    {
                        string absolutePath = Path.Combine(pathToPlugins, file);
                        string targetPath = Path.Combine(backupFolderPath, "plugins", file);

                        if (File.Exists(absolutePath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            File.Move(absolutePath, targetPath);
                        }
                    }

                    if (Directory.GetFileSystemEntries(pathToPlugins).Length == 0)
                    {
                        // delete now empty plugins folder
                        Directory.Delete(pathToPlugins);
                    }
                }

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
        /// Creates 'mods/Textures', 'direct' and subfolders, 'music', 'music/vgmstream', 'data/kernel' 'data/battle' folders if missing
        /// </summary>
        public void CreateMissingDirectories()
        {
            string[] requiredFolders = new string[]
            {
                Sys.Settings.AaliFolder,
                Path.Combine(InstallPath, "data"),
                Path.Combine(InstallPath, "data", "kernel"),
                Path.Combine(InstallPath, "data", "battle"),
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

        internal void CopyMissingShaders()
        {
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

            // copy files from shaders folder if files are missing
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
        public string InstallPath { get; set; }
    }
}
