using _7thHeaven.Code;
using _7thWrapperLib;
using Iros._7th;
using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeaven.Windows;
using SeventhHeavenUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using XInputDotNetPure;
using static SeventhHeaven.Classes.KeyboardInputSender;

namespace SeventhHeaven.Classes
{
    public enum FF7Version
    {
        Unknown = -1,
        Steam,
        ReRelease,
        Original98
    }

    internal enum GraphicsRenderer
    {
        SoftwareRenderer = 0,
        D3DHardwareAccelerated = 1,
        CustomDriver = 3
    }

    /// <summary>
    /// Responsibile for the entire process that happens for launching the game
    /// </summary>
    public class GameLauncher
    {
        #region Data Members and Properties

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static GameLauncher _instance;

        public static GameLauncher Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameLauncher();

                return _instance;
            }
        }

        private Dictionary<string, Process> _alsoLaunchProcesses = new Dictionary<string, Process>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, _7HPlugin> _plugins = new Dictionary<string, _7HPlugin>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<_7thWrapperLib.ProgramInfo, Process> _sideLoadProcesses = new Dictionary<_7thWrapperLib.ProgramInfo, Process>();

        private ControllerInterceptor _controllerInterceptor; 

        public delegate void OnProgressChanged(string message);
        public event OnProgressChanged ProgressChanged;

        public delegate void OnLaunchCompleted(bool wasSuccessful);
        public event OnLaunchCompleted LaunchCompleted;


        public string DriveLetter { get; set; }
        public bool DidMountVirtualDisc { get; set; }
        /// <summary>
        /// Used to ensure only one thread/task is trying to mount/unmount the disc
        /// </summary>
        private static object _mountLock = new object();

        #endregion

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_FORCEMINIMIZE = 11;
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static bool LaunchGame(bool varDump, bool debug, bool launchWithNoMods = false)
        {
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingFf7IsNotRunning));
            if (IsFF7Running())
            {
                string title = ResourceHelper.Get(StringKey.Ff7IsAlreadyRunning);
                string message = ResourceHelper.Get(StringKey.Ff7IsAlreadyRunningDoYouWantToForceClose);

                Instance.RaiseProgressChanged($"\t{title}", NLog.LogLevel.Warn);

                var result = MessageDialogWindow.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result.Result == MessageBoxResult.Yes)
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ForceClosingAllInstancesFf7)}", NLog.LogLevel.Info);

                    if (!ForceKillFF7())
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCloseProcess)} {Path.GetFileName(Sys.Settings.FF7Exe)}. {ResourceHelper.Get(StringKey.Aborting)}", NLog.LogLevel.Error);
                        return false;
                    }
                }
            }

            Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.CheckingFf7ExeExistsAt)} {Sys.Settings.FF7Exe} ...");
            if (!File.Exists(Sys.Settings.FF7Exe))
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FileNotFoundAborting)}", NLog.LogLevel.Error);
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.Ff7ExeNotFoundYouMayNeedToConfigure));
                return false;
            }


            //
            // GAME CONVERTER - Make sure game is ready for mods
            // 
            GameConverter converter = new GameConverter(Path.GetDirectoryName(Sys.Settings.FF7Exe));
            converter.MessageSent += GameConverter_MessageSent;


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingInstalledGameIsCompatible));
            if (converter.IsGamePirated())
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ErrorCodeYarr), NLog.LogLevel.Error);
                Logger.Info(FileUtils.ListAllFiles(converter.InstallPath));
                return false;
            }

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingGameIsNotInstalledInProtectedFolder));
            if (converter.IsGameLocatedInSystemFolders())
            {
                string message = ResourceHelper.Get(StringKey.Ff7IsCurrentlyInstalledInASystemFolder);
                var result = MessageDialogWindow.Show(message, ResourceHelper.Get(StringKey.CannotContinue), MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result.Result == MessageBoxResult.No)
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CanNotContinueDueToFf7nstalledInProtectedFolder)}", NLog.LogLevel.Error);
                    return false;
                }


                // copy installation and update settings with new path
                string newInstallationPath = @"C:\Games\Final Fantasy VII";
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CopyingGameFilesTo)} {newInstallationPath}");
                bool didCopy = converter.CopyGame(newInstallationPath);

                if (!didCopy)
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCopyFf7To)} {newInstallationPath}. {ResourceHelper.Get(StringKey.Aborting)}", NLog.LogLevel.Error);
                    return false;
                }

                // update settings with new path
                Logger.Info("\tUpdating paths in Sys.Settings with new installation path");
                Sys.Settings.SetPathsFromInstallationPath(newInstallationPath);
                converter.InstallPath = newInstallationPath;
            }

            Instance.RaiseProgressChanged($"Verifying english game files exist ...");
            if (!converter.IsEnglishGameInstalled())
            {
                Instance.RaiseProgressChanged($"\tFound language installed: {converter.GetInstalledLanguage()}. Creating english game files...");
                converter.ConvertToEnglishInstall();
            }


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingGameIsMaxInstall));
            if (!converter.VerifyFullInstallation())
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.YourFf7InstallationFolderIsMissingCriticalFiles), NLog.LogLevel.Error);
                return false;
            }


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CreatingMissingRequiredDirectories));
            converter.CreateMissingDirectories();


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingAdditionalFilesForBattleAndKernelFoldersExist));
            if (!converter.VerifyAdditionalFilesExist())
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.FailedToVerifyCopyMissingAdditionalFiles), NLog.LogLevel.Error);
                return false;
            }

            converter.CopyMovieFilesToFolder(Sys.Settings.MovieFolder);

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingAllMovieFilesExist));
            if (!GameConverter.AllMovieFilesExist(Sys.Settings.MovieFolder))
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CouldNotFindAllMovieFilesAt)} {Sys.Settings.MovieFolder}", NLog.LogLevel.Warn);

                string otherMovieFolder = Path.Combine(converter.InstallPath, "data", "movies");
                bool pathsAreSame = Sys.Settings.MovieFolder.Equals(otherMovieFolder);
                if (!pathsAreSame && GameConverter.AllMovieFilesExist(otherMovieFolder)) // only check other location if different then what is already set in General Settings
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.AllFilesFoundAt)} {otherMovieFolder}. {ResourceHelper.Get(StringKey.UpdatingMoviePathSetting)}");
                    Sys.Settings.MovieFolder = otherMovieFolder;
                }
                else
                {
                    if (!pathsAreSame)
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CouldNotFindAllMovieFilesAt)} {otherMovieFolder}", NLog.LogLevel.Warn);
                    }

                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.AttemptingToCopyMovieFiles)}");

                    if (!converter.CopyMovieFilesToFolder(Sys.Settings.MovieFolder))
                    {
                        // skip warning if an active mod contains movie files
                        bool activeModsHasMovies = Sys.ActiveProfile.ActiveItems.Any(a => Sys.Library.GetItem(a.ModID).CachedDetails.ContainsMovies);

                        string title = ResourceHelper.Get(StringKey.MovieFilesAreMissing);
                        string message = ResourceHelper.Get(StringKey.InOrderToSeeInGameMoviesYouWillNeedMessage);
                        if (!Sys.Settings.GameLaunchSettings.HasDisplayedMovieWarning && !activeModsHasMovies)
                        {
                            Sys.Settings.GameLaunchSettings.HasDisplayedMovieWarning = true;
                            MessageDialogWindow.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        if (!activeModsHasMovies)
                        {
                            Instance.RaiseProgressChanged(title, NLog.LogLevel.Warn);
                            Instance.RaiseProgressChanged(message, NLog.LogLevel.Warn);
                        }

                    }
                }
            }

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingMusicFilesExist));
            if (!converter.AllMusicFilesExist())
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CouldNotFindAllMusicFilesAt)} {Path.Combine(converter.InstallPath, "music", "vgmstream")}", NLog.LogLevel.Warn);

                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.AttemptingToCopyMusicFiles)}");
                converter.CopyMusicFiles();

                if (!converter.AllMusicFilesExist())
                {
                    // skip warning if an active mod contains music files
                    bool activeModsHasMusic = Sys.ActiveProfile.ActiveItems.Any(a => Sys.Library.GetItem(a.ModID).CachedDetails.ContainsMusic);

                    string title = ResourceHelper.Get(StringKey.OggMusicFilesAreMissing);
                    string message = ResourceHelper.Get(StringKey.InOrderToHearHighQualityMusicYouWillNeedMessage);
                    if (!Sys.Settings.GameLaunchSettings.HasDisplayedOggMusicWarning && !activeModsHasMusic)
                    {
                        Sys.Settings.GameLaunchSettings.HasDisplayedOggMusicWarning = true;
                        MessageDialogWindow.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    if (!activeModsHasMusic)
                    {
                        Instance.RaiseProgressChanged(title, NLog.LogLevel.Warn);
                        Instance.RaiseProgressChanged(message, NLog.LogLevel.Warn);
                    }

                }
            }
            string backupFolderPath = Path.Combine(converter.InstallPath, GameConverter.BackupFolderName, DateTime.Now.ToString("yyyyMMddHHmmss"));


            converter.CheckAndCopyOldGameConverterFiles(backupFolderPath);


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingLatestGameDriverIsInstalled));
            if (!converter.InstallLatestGameDriver(backupFolderPath))
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.SomethingWentWrongTryingToDetectGameDriver), NLog.LogLevel.Error);
                return false;
            }


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingGameDriverShadersFoldersExist));
            converter.CopyMissingShaders();


            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VerifyingFf7Exe));
            if (new FileInfo(Sys.Settings.FF7Exe).Name.Equals("ff7.exe", StringComparison.InvariantCultureIgnoreCase))
            {
                // only compare exes are different if ff7.exe set in Settings (and not something like ff7_bc.exe)
                if (converter.IsExeDifferent())
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Ff7ExeDetectedToBeDifferent)}");
                    if (converter.BackupExe(backupFolderPath))
                    {
                        bool didCopy = converter.CopyFF7ExeToGame();
                        if (!didCopy)
                        {
                            Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCopyFf7Exe)}", NLog.LogLevel.Error);
                            return false;
                        }
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCreateBackupOfFf7Exe)}", NLog.LogLevel.Error);
                        return false;
                    }
                }
            }

            if (converter.IsConfigExeDifferent())
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Ff7ConfigExeDetectedToBeMissingOrDifferent)}");
                if (converter.BackupFF7ConfigExe(backupFolderPath))
                {
                    bool didCopy = converter.CopyFF7ConfigExeToGame();
                    if (!didCopy)
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCopyFf7ConfigExe)}", NLog.LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCreateBackupOfFf7ConfigExe)}", NLog.LogLevel.Error);
                    return false;
                }
            }




            //
            // GAME SHOULD BE FULLY 'CONVERTED' AND READY TO LAUNCH FOR MODS AT THIS POINT
            //
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingAProfileIsActive));
            if (Sys.ActiveProfile == null)
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ActiveProfileNotFound)}", NLog.LogLevel.Error);
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CreateAProfileFirstAndTryAgain));
                return false;
            }

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingModCompatibilityRequirements));
            if (!SanityCheckCompatibility())
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedModCompatibilityCheck)}", NLog.LogLevel.Error);
                return false;
            }

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingModConstraintsForCompatibility));
            if (!SanityCheckSettings())
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedModConstraintCheck)}", NLog.LogLevel.Error);
                return false;
            }

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingModLoadOrderRequirements));
            if (!VerifyOrdering())
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedModLoadOrderCheck)}", NLog.LogLevel.Error);
                return false;
            }


            //
            // Get Drive Letter and auto mount if needed
            //
            Instance.DidMountVirtualDisc = false;

            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.LookingForGameDisc));
            Instance.DriveLetter = GetDriveLetter();

            if (!string.IsNullOrEmpty(Instance.DriveLetter))
            {
                Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.FoundGameDiscAt)} {Instance.DriveLetter} ...");
            }
            else
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.FailedToFindGameDisc), NLog.LogLevel.Warn);

                if (!OSHasAutoMountSupport())
                {
                    Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.OsDoesNotSupportAutoMounting), NLog.LogLevel.Error);
                    return false;
                }
                else
                {
                    if (Sys.Settings.GameLaunchSettings.AutoMountGameDisc)
                    {
                        Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.AutoMountingVirtualGameDisc));
                        bool didMount = MountIso();

                        if (!didMount)
                        {
                            Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.FailedToAutoMountVirtualDiscAt)} {Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO")} ...", NLog.LogLevel.Error);
                            return false;
                        }

                        Instance.DidMountVirtualDisc = true;
                        Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.LookingForGameDiscAfterMounting));
                        Instance.DriveLetter = GetDriveLetter();

                        if (string.IsNullOrEmpty(Instance.DriveLetter))
                        {
                            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.FailedToFindGameDiscAfterAutoMounting), NLog.LogLevel.Error);
                            return false;
                        }

                        Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.FoundGameDiscAt)} {Instance.DriveLetter} ...");
                    }
                }
            }

            //
            // Update Registry with new launch settings
            // 
            Instance.SetRegistryValues();
            Instance.SetCompatibilityFlagsInRegistry();


            //
            // Determine if game will be ran as 'vanilla' with mods so don't have to inject with EasyHook
            //
            bool runAsVanilla = false;
            string vanillaMsg = "";

            if (launchWithNoMods)
            {
                vanillaMsg = ResourceHelper.Get(StringKey.UserRequestedToPlayWithNoModsLaunchingGameAsVanilla);
                runAsVanilla = true;
            }
            else if (Sys.ActiveProfile.ActiveItems.Count == 0)
            {
                vanillaMsg = ResourceHelper.Get(StringKey.NoModsActivatedLaunchingGameAsVanilla);
                runAsVanilla = true;
            }
            else if (Sys.Settings.GameLaunchSettings.SelectedRenderer != (int)GraphicsRenderer.CustomDriver)
            {
                vanillaMsg = ResourceHelper.Get(StringKey.SelectedRendererIsNotSetToCustomLaunchingGameAsVanilla);
                runAsVanilla = true;
            }



            RuntimeProfile runtimeProfile = null;

            if (!runAsVanilla)
            {
                //
                // Create Runtime Profile for Active Mods
                //
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CreatingRuntimeProfile));
                runtimeProfile = CreateRuntimeProfile();

                if (runtimeProfile == null)
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCreateRuntimeProfileForActiveMods)}", NLog.LogLevel.Error);
                    return false;
                }

                //
                // Start Turbolog for Variable Dump
                //
                if (varDump)
                {
                    Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.VariableDumpSetToTrueStartingTurboLog));
                    StartTurboLogForVariableDump(runtimeProfile);
                }

                //
                // Copy EasyHook.dll to FF7
                //
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CopyingEasyHookToFf7PathIfNotFoundOrOlder));
                CopyEasyHookDlls();
            }


            //
            // Copy input.cfg to FF7
            //
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CopyingFf7InputCfgToFf7Path));
            bool didCopyCfg = CopyKeyboardInputCfg();


            //
            // Setup log file if debugging
            //
            if (debug && runtimeProfile != null)
            {
                runtimeProfile.Options |= RuntimeOptions.DetailedLog;
                runtimeProfile.LogFile = Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "log.txt");

                Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.DebugLoggingSetToTrueDetailedLoggingWillBeWrittenTo)} {runtimeProfile.LogFile} ...");
            }

            //
            // Check/Disable Reunion Mod
            //
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.CheckingIfReunionModIsInstalled));
            Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Found)}: {IsReunionModInstalled()}");

            bool didDisableReunion = false;
            if (IsReunionModInstalled() && Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch)
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.DisablingReunionMod));
                EnableOrDisableReunionMod(doEnable: false);
                didDisableReunion = true;
            }

            //
            // Initialize the controller input interceptor and DS4Win service to recognize playstation 4 controller as xbox 360 controller (XInput support)
            //

            if (Instance._controllerInterceptor == null)
            {
                Instance._controllerInterceptor = new ControllerInterceptor();
            }


            // start FF7 proc as normal and return true when running the game as vanilla
            if (runAsVanilla)
            {
                Instance.RaiseProgressChanged(vanillaMsg);
                LaunchFF7Exe();

                if (didDisableReunion)
                {
                    Task.Factory.StartNew(() =>
                    {
                        System.Threading.Thread.Sleep(5000); // wait 5 seconds before renaming the dll so the game and gl driver can fully initialize
                        Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ReenablingReunionMod));
                        EnableOrDisableReunionMod(doEnable: true);
                    });
                }

                return true;
            }

            //
            // Attempt to Create FF7 Proc and Inject with EasyHook
            //
            int pid;
            try
            {
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.LaunchingAdditionalProgramsToRunIfAny));
                Instance.LaunchAdditionalProgramsToRunPrior();

                RuntimeParams parms = new RuntimeParams
                {
                    ProfileFile = Path.GetTempFileName()
                };

                Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.WritingTemporaryRuntimeProfileFileTo)} {parms.ProfileFile} ...");

                using (FileStream fs = new FileStream(parms.ProfileFile, FileMode.Create))
                    Util.SerializeBinary(runtimeProfile, fs);

                // attempt to launch the game a few times in the case of an ApplicationException that can be thrown by EasyHook it seems randomly at times
                // ... The error tends to go away the second time trying but we will try multiple times before failing
                // ... if we fail to inject with EasyHook then we will give the user a chance to set the compatibility flag to fix the issue
                string lib = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "7thWrapperLib.dll");

                bool didInject = false;
                int attemptCount = 0;
                int totalAttempts = 7;
                pid = -1;

                while (!didInject && attemptCount < totalAttempts)
                {
                    didInject = false;

                    try
                    {
                        Instance.RaiseProgressChanged(string.Format(ResourceHelper.Get(StringKey.AttemptingToInjectWithEasyHook), attemptCount + 1, totalAttempts));

                        // attempt to inject on background thread so we can have a timeout if the process does not return in 10 seconds
                        // a successful injection should only take ~3 seconds
                        var waitTask = Task.Factory.StartNew(() =>
                        {
                            EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);
                        }).ContinueWith((taskResult) =>
                        {
                            if (taskResult.IsFaulted)
                            {
                                // an error occurred when injecting so concatenate all errors to display in output
                                didInject = false;
                                string errors = "";
                                foreach (Exception ex in taskResult.Exception.InnerExceptions)
                                {
                                    if (ex is AggregateException)
                                    {
                                        errors += string.Join("; ", (ex as AggregateException).InnerExceptions.Select(s => s.Message));
                                    }
                                    else
                                    {
                                        errors += $"{ex.Message}; ";
                                    }
                                }

                                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ReceivedErrors)}: {errors} ...", NLog.LogLevel.Warn);
                            }
                            else
                            {
                                didInject = true;
                            }
                        }).ConfigureAwait(false);

                        // Wait 10 seconds for the injection to complete
                        DateTime startTime = DateTime.Now;
                        while (!waitTask.GetAwaiter().IsCompleted)
                        {
                            TimeSpan elapsed = DateTime.Now.Subtract(startTime);
                            if (elapsed.Seconds > 10)
                            {
                                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ReachedTimeoutWaitingForInjection)}", NLog.LogLevel.Warn);
                                didInject = false;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ReceivedUnknownError)}: {e.Message} ...", NLog.LogLevel.Warn);
                    }
                    finally
                    {
                        attemptCount++;
                    }

                    // If the CreateAndInject() process fails to inject it will attempt to kill the process it started...
                    // ... this could fail and leave the process open which causes problems for the next attempt so make sure the process is killed on failure
                    if (!didInject)
                    {
                        string procName = Path.GetFileNameWithoutExtension(Sys.Settings.FF7Exe);
                        var openProcs = Process.GetProcessesByName(procName);
                        foreach (Process proc in openProcs)
                        {
                            if (!proc.HasExited)
                                proc.Kill();
                        }
                    }
                }

                if (!didInject)
                {
                    Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.FailedToInjectAfterMaxAmountOfTries)} ({totalAttempts}) ...", NLog.LogLevel.Warn);
                    Sys.Settings.GameLaunchSettings.HasReceivedCode5Error = true; // update launch settings to notify that user has received a code 5 error in the code

                    // give user option to set compat flag and try again
                    var viewModel = MessageDialogWindow.Show(ResourceHelper.Get(StringKey.FailedToInjectWithEasyHookMessage),
                                                             ResourceHelper.Get(StringKey.ErrorFailedToStartGame),
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Warning);

                    if (viewModel.Result == MessageBoxResult.Yes)
                    {
                        Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.SettingCompatibilityFixAndTryingAgain));
                        Sys.Settings.GameLaunchSettings.Code5Fix = true;

                        Instance.SetCompatibilityFlagsInRegistry();

                        try
                        {
                            EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);
                        }
                        catch (ApplicationException aex)
                        {
                            if (aex.Message.IndexOf("Unknown error in injected assembler code", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.StillFailedToInjectWithEasyHookAborting), NLog.LogLevel.Error);
                                MessageDialogWindow.Show(ResourceHelper.Get(StringKey.FailedToInjectWithEasyHookAfterSettingFlags), ResourceHelper.Get(StringKey.ErrorFailedToStartGame), MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.UserChoseNotToSetCompatibilityFix)}");
                        return false;
                    }
                }


                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.GettingFf7Proc));
                Process ff7Proc = Process.GetProcessById(pid);

                if (ff7Proc == null)
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToGetFf7Proc)}", NLog.LogLevel.Error);
                    return false;
                }

                ff7Proc.EnableRaisingEvents = true;
                if (debug)
                {
                    Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.DebugLoggingSetToTrueWiringUpLogFile));
                    ff7Proc.Exited += (o, e) =>
                    {
                        try
                        {
                            DebugLogger.CloseLogFile();

                            ProcessStartInfo debugTxtProc = new ProcessStartInfo()
                            {
                                WorkingDirectory = Path.GetDirectoryName(runtimeProfile.LogFile),
                                FileName = runtimeProfile.LogFile
                            };
                            Process.Start(debugTxtProc);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.FailedToStartProcessForDebugLog)}: {ex.Message}", NLog.LogLevel.Error);
                        }
                    };
                }

                /// sideload programs for mods before starting FF7 because FF7 losing focus while initializing can cause the intro movies to stop playing
                /// ... Thus we load programs first so they don't steal window focus
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.StartingProgramsForMods));
                foreach (RuntimeMod mod in runtimeProfile.Mods)
                {
                    Instance.LaunchProgramsForMod(mod);
                }

                /// load plugins for mods
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.StartingPluginsForMods));
                foreach (RuntimeMod mod in runtimeProfile.Mods)
                {
                    StartPluginsForMod(mod);
                }

                // wire up process to stop plugins and side processes when proc has exited
                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.SettingUpFf7ExeToStopPluginsAndModPrograms));
                ff7Proc.Exited += (o, e) =>
                {
                    if (!IsFF7Running())
                    {
                        // stop polling for input once all ff7 procs are closed (could be multiple instances open)
                        Instance._controllerInterceptor.PollingInput = false;
                    }

                    for (int i = 0; i < Instance._plugins.Count; i++)
                    {
                        try
                        {
                            string key = Instance._plugins.ElementAt(i).Key;
                            Instance._plugins[key].Stop();
                            Instance._plugins[key] = null;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }

                    // wrapped in try/catch so an unhandled exception when exiting the game does not crash 7H
                    try
                    {
                        Instance._plugins.Clear();

                        Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.StoppingOtherProgramsForMods));
                        Instance.StopAllSideProcessesForMods();

                        if (Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc && Instance.DidMountVirtualDisc && IsVirtualIsoMounted(Instance.DriveLetter))
                        {
                            Instance.RaiseProgressChanged("Auto unmounting game disc ...");
                            UnmountIso();
                        }

                        // ensure Reunion is re-enabled when ff7 process exits in case it failed above for any reason
                        if (File.Exists(Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "Reunion.dll.bak")))
                        {
                            EnableOrDisableReunionMod(doEnable: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                };


                int secondsToWait = 120;
                Instance.RaiseProgressChanged(string.Format(ResourceHelper.Get(StringKey.WaitingForFf7ExeToRespond), secondsToWait));
                DateTime start = DateTime.Now;
                while (ff7Proc.Responding == false)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(start);
                    if (elapsed.TotalSeconds > secondsToWait)
                        break;
                }

                if (didDisableReunion)
                {
                    Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ReenablingReunionMod));
                    EnableOrDisableReunionMod(doEnable: true);
                }

                // ensure ff7 window is active at end of launching
                if (ff7Proc.MainWindowHandle != IntPtr.Zero)
                {
                    SetForegroundWindow(ff7Proc.MainWindowHandle);
                    Instance._controllerInterceptor.PollForGamepadInput();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);

                Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ExceptionOccurredWhileTryingToStart), NLog.LogLevel.Error);
                MessageDialogWindow.Show(e.ToString(), ResourceHelper.Get(StringKey.ErrorFailedToStartGame), MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
            finally
            {
                converter.MessageSent -= GameConverter_MessageSent;
            }
        }

        private static void StartPluginsForMod(RuntimeMod mod)
        {
            if (!mod.LoadPlugins.Any())
            {
                return; // no plugins to load
            }

            mod.Startup();
            foreach (string dll in mod.GetLoadPlugins())
            {
                _7HPlugin plugin;
                if (!Instance._plugins.TryGetValue(dll, out plugin))
                {
                    System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFrom(dll);

                    plugin = asm.GetType("_7thHeaven.Plugin")
                                .GetConstructor(Type.EmptyTypes)
                                .Invoke(null) as _7HPlugin;

                    Instance._plugins.Add(dll, plugin);
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.PluginAdded)}: {dll}");
                }

                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.StartingPlugin)}: {dll}");

                // invoke on app dispatcher thread since accessing main window which is a UI object
                App.Current.Dispatcher.Invoke(() =>
                {
                    mod.WpfWindowInterop = new Wpf32Window(new WindowInteropHelper(App.Current.MainWindow).EnsureHandle());
                    App.Current.Dispatcher.Invoke(() => plugin.Start(mod));
                });
            }
        }

        private static void GameConverter_MessageSent(string message, NLog.LogLevel logLevel)
        {
            Instance.RaiseProgressChanged(message, logLevel);
        }

        internal static RuntimeProfile CreateRuntimeProfile()
        {
            string ff7Folder = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToDataFolder = Path.Combine(ff7Folder, "data");
            List<RuntimeMod> runtimeMods = null;

            try
            {
                runtimeMods = Sys.ActiveProfile.ActiveItems.Select(i => i.GetRuntime(Sys._context))
                                                           .Where(i => i != null)
                                                           .ToList();
            }
            catch (VariableAliasNotFoundException aex)
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToGetListOfRuntimeMods)}: {aex.Message}", NLog.LogLevel.Error);
                return null;
            }
            catch (Exception)
            {
                throw;
            }


            RuntimeProfile runtimeProfiles = new RuntimeProfile()
            {
                MonitorPaths = new List<string>() {
                    pathToDataFolder,
                    Sys.Settings.AaliFolder,
                    Sys.Settings.MovieFolder,
                },
                ModPath = Sys.Settings.LibraryLocation,
                OpenGLConfig = Sys.ActiveProfile.OpenGLConfig,
                FF7Path = ff7Folder,
                gameFiles = Directory.GetFiles(ff7Folder, "*.*", SearchOption.AllDirectories),
                Mods = runtimeMods
            };

            Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.AddingPathsToMonitor)}");
            runtimeProfiles.MonitorPaths.AddRange(Sys.Settings.ExtraFolders.Where(s => s.Length > 0).Select(s => Path.Combine(ff7Folder, s)));
            return runtimeProfiles;
        }

        private static void CopyEasyHookDlls()
        {
            string source = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string dir = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToTargetEasyHook = Path.Combine(dir, "EasyHook.dll");
            string pathToSourceEasyHook = Path.Combine(source, "EasyHook.dll");

            // compare file versions to determine if file can be skipped
            bool hasOldVersion = false;

            if (File.Exists(pathToTargetEasyHook))
            {
                FileVersionInfo sourceVersion = FileVersionInfo.GetVersionInfo(pathToSourceEasyHook);
                FileVersionInfo targetVersion = FileVersionInfo.GetVersionInfo(pathToTargetEasyHook);

                if (new Version(targetVersion.FileVersion).CompareTo(new Version(sourceVersion.FileVersion)) < 0)
                {
                    Instance.RaiseProgressChanged($"\tEasyHook.dll {ResourceHelper.Get(StringKey.DetectedToBeOlderVersion)} ...");
                    hasOldVersion = true;
                }
            }



            if (!File.Exists(pathToTargetEasyHook) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook, pathToTargetEasyHook, true);
                Instance.RaiseProgressChanged($"\tEasyHook.dll {ResourceHelper.Get(StringKey.Copied)} ...");
            }
            else
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.SkippedCopying)} EasyHook.dll ...");

            }



            string pathToTargetEasyHook32 = Path.Combine(dir, "EasyHook32.dll");
            string pathToSourceEasyHook32 = Path.Combine(source, "EasyHook32.dll");
            hasOldVersion = false;

            if (File.Exists(pathToTargetEasyHook32))
            {
                FileVersionInfo sourceVersion = FileVersionInfo.GetVersionInfo(pathToSourceEasyHook32);
                FileVersionInfo targetVersion = FileVersionInfo.GetVersionInfo(pathToTargetEasyHook32);

                if (new Version(targetVersion.FileVersion).CompareTo(new Version(sourceVersion.FileVersion)) < 0)
                {
                    Instance.RaiseProgressChanged($"\tEasyHook32.dll {ResourceHelper.Get(StringKey.DetectedToBeOlderVersion)} ...");
                    hasOldVersion = true;
                }
            }


            if (!File.Exists(pathToTargetEasyHook32) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook32, pathToTargetEasyHook32, true);
                Instance.RaiseProgressChanged($"\tEasyHook32.dll {ResourceHelper.Get(StringKey.Copied)} ...");
            }
            else
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.SkippedCopying)} EasyHook32.dll ...");
            }
        }

        private static void StartTurboLogForVariableDump(RuntimeProfile runtimeProfiles)
        {
            string turboLogProcName = Path.Combine(Sys._7HFolder, "TurBoLog.exe");

            // remove from dictionary (and stop other turbolog exe) if exists
            if (Instance._alsoLaunchProcesses.ContainsKey(turboLogProcName))
            {
                if (!Instance._alsoLaunchProcesses[turboLogProcName].HasExited)
                {
                    Instance._alsoLaunchProcesses[turboLogProcName].Kill();
                }
                Instance._alsoLaunchProcesses.Remove(turboLogProcName);
            }

            runtimeProfiles.MonitorVars = Sys._context.VarAliases.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();

            ProcessStartInfo psi = new ProcessStartInfo(turboLogProcName)
            {
                WorkingDirectory = Path.GetDirectoryName(Sys.Settings.FF7Exe)
            };
            Process aproc = Process.Start(psi);

            Instance._alsoLaunchProcesses.Add(turboLogProcName, aproc);
            aproc.EnableRaisingEvents = true;
            aproc.Exited += (o, e) => Instance._alsoLaunchProcesses.Remove(turboLogProcName);
        }


        /// <summary>
        /// Launches FF7.exe without loading any mods.
        /// </summary>
        internal static bool LaunchFF7Exe()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(Sys.Settings.FF7Exe)
                {
                    WorkingDirectory = Path.GetDirectoryName(Sys.Settings.FF7Exe)
                };

                Process ff7Proc = Process.Start(startInfo);

                ff7Proc.EnableRaisingEvents = true;
                ff7Proc.Exited += (o, e) =>
                {
                    try
                    {
                        if (!IsFF7Running())
                        {
                            // stop polling for input once all ff7 procs are closed (could be multiple instances open)
                            Instance._controllerInterceptor.PollingInput = false;
                        }

                        if (Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc && Instance.DidMountVirtualDisc && IsVirtualIsoMounted(Instance.DriveLetter))
                        {
                            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.AutoUnmountingGameDisc));
                            UnmountIso();
                        }

                        // ensure Reunion is re-enabled when ff7 process exits in case it failed above for any reason
                        if (File.Exists(Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "Reunion.dll.bak")))
                        {
                            EnableOrDisableReunionMod(doEnable: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                };

                Instance._controllerInterceptor.PollForGamepadInput();

                return true;
            }
            catch (Exception ex)
            {
                Instance.RaiseProgressChanged($"{ResourceHelper.Get(StringKey.AnExceptionOccurredTryingToStartFf7At)} {Sys.Settings.FF7Exe} ...", NLog.LogLevel.Error);
                Logger.Error(ex);
                return false;
            }
        }

        internal static bool SanityCheckSettings()
        {
            List<string> changes = new List<string>();
            foreach (var constraint in GetConstraints())
            {
                if (!constraint.Verify(out string msg))
                {
                    Logger.Warn(msg);
                    MessageDialogWindow.Show(ResourceHelper.Get(StringKey.ThereWasAnErrorVerifyingModConstraintSeeTheFollowingDetails), msg, ResourceHelper.Get(StringKey.Warning), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (msg != null)
                {
                    changes.Add(msg);
                }
            }

            if (changes.Any())
            {
                Logger.Warn($"The following settings have been changed to make these mods compatible:\n{String.Join("\n", changes)}");
                MessageDialogWindow.Show(ResourceHelper.Get(StringKey.TheFollowingSettingsHaveBeenChangedToMakeTheseModsCompatible), String.Join("\n", changes), ResourceHelper.Get(StringKey.Warning), MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return true;
        }

        internal static bool SanityCheckCompatibility()
        {
            if (Sys.Settings.HasOption(GeneralOptions.BypassCompatibility))
            {
                return true; // user has 'ignore compatibility restrictions' set in general settings so just return true
            }

            List<InstalledItem> activeInstalledMods = Sys.ActiveProfile.ActiveItems.Select(pi => Sys.Library.GetItem(pi.ModID)).ToList();

            foreach (InstalledItem item in activeInstalledMods)
            {
                ModInfo info = item.GetModInfo();

                if (info == null)
                {
                    continue;
                }

                foreach (var req in info.Compatibility.Requires)
                {
                    var rInst = activeInstalledMods.Find(i => i.ModID.Equals(req.ModID));
                    if (rInst == null)
                    {
                        MessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ModRequiresYouToActivateAsWell), item.CachedDetails.Name, req.Description), ResourceHelper.Get(StringKey.MissingRequiredActiveMods));
                        return false;
                    }
                    else if (req.Versions.Any() && !req.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ModRequiresYouToActivateButYouDoNotHaveCompatibleVersionInstalled), item.CachedDetails.Name, rInst.CachedDetails.Name), ResourceHelper.Get(StringKey.UnsupportedModVersion));
                        return false;
                    }
                }

                foreach (var forbid in info.Compatibility.Forbids)
                {
                    var rInst = activeInstalledMods.Find(i => i.ModID.Equals(forbid.ModID));
                    if (rInst == null)
                    {
                        continue; //good!
                    }

                    if (forbid.Versions.Any() && forbid.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ModIsNotCompatibleWithTheVersionYouHaveInstalled), item.CachedDetails.Name, rInst.CachedDetails.Name), ResourceHelper.Get(StringKey.IncompatibleMod));
                        return false;
                    }
                    else
                    {
                        MessageDialogWindow.Show(string.Format(ResourceHelper.Get(StringKey.ModIsNotCompatibleWithYouWillNeedToDisableIt), item.CachedDetails.Name, rInst.CachedDetails.Name), ResourceHelper.Get(StringKey.IncompatibleMod));
                        return false;
                    }
                }
            }

            return true;
        }

        internal static bool VerifyOrdering()
        {
            var details = Sys.ActiveProfile
                             .ActiveItems
                             .Select(i => Sys.Library.GetItem(i.ModID))
                             .Select(ii => new { Mod = ii, Info = ii.GetModInfo() })
                             .ToDictionary(a => a.Mod.ModID, a => a);

            List<string> problems = new List<string>();

            foreach (int i in Enumerable.Range(0, Sys.ActiveProfile.ActiveItems.Count))
            {
                Iros._7th.Workshop.ProfileItem mod = Sys.ActiveProfile.ActiveItems[i];
                var info = details[mod.ModID].Info;

                if (info == null)
                {
                    continue;
                }

                foreach (Guid after in info.OrderAfter)
                {
                    if (Sys.ActiveProfile.ActiveItems.Skip(i).Any(pi => pi.ModID.Equals(after)))
                    {
                        problems.Add(string.Format(ResourceHelper.Get(StringKey.ModIsNotCompatibleWithYouWillNeedToDisableIt), details[mod.ModID].Mod.CachedDetails.Name, details[after].Mod.CachedDetails.Name));
                    }
                }

                foreach (Guid before in info.OrderBefore)
                {
                    if (Sys.ActiveProfile.ActiveItems.Take(i).Any(pi => pi.ModID.Equals(before)))
                    {
                        problems.Add(string.Format(ResourceHelper.Get(StringKey.ModIsNotCompatibleWithYouWillNeedToDisableIt), details[mod.ModID].Mod.CachedDetails.Name, details[before].Mod.CachedDetails.Name));
                    }
                }
            }

            if (problems.Any())
            {
                if (MessageDialogWindow.Show(ResourceHelper.Get(StringKey.TheFollowingModsWillNotWorkProperlyInTheCurrentOrder), String.Join("\n", problems), ResourceHelper.Get(StringKey.LoadOrderIncompatible), MessageBoxButton.YesNo).Result != MessageBoxResult.Yes)
                    return false;
            }

            return true;
        }

        internal static List<Constraint> GetConstraints()
        {
            List<Constraint> constraints = new List<Constraint>();
            foreach (Iros._7th.Workshop.ProfileItem pItem in Sys.ActiveProfile.ActiveItems)
            {
                InstalledItem inst = Sys.Library.GetItem(pItem.ModID);
                ModInfo info = inst.GetModInfo();

                if (info == null)
                {
                    continue;
                }

                foreach (var cSetting in info.Compatibility.Settings)
                {
                    if (!String.IsNullOrWhiteSpace(cSetting.MyID))
                    {
                        var setting = pItem.Settings.Find(s => s.ID.Equals(cSetting.MyID, StringComparison.InvariantCultureIgnoreCase));
                        if ((setting == null) || (setting.Value != cSetting.MyValue)) continue;
                    }

                    Iros._7th.Workshop.ProfileItem oItem = Sys.ActiveProfile.ActiveItems.Find(i => i.ModID.Equals(cSetting.ModID));
                    if (oItem == null) continue;

                    InstalledItem oInst = Sys.Library.GetItem(cSetting.ModID);
                    Constraint ct = constraints.Find(c => c.ModID.Equals(cSetting.ModID) && c.Setting.Equals(cSetting.TheirID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = cSetting.ModID, Setting = cSetting.TheirID };
                        constraints.Add(ct);
                    }

                    if (!ct.ParticipatingMods.ContainsKey(inst.CachedDetails.ID))
                    {
                        ct.ParticipatingMods.Add(inst.CachedDetails.ID, inst.CachedDetails.Name);
                    }

                    if (cSetting.Require.HasValue)
                    {
                        ct.Require.Add(cSetting.Require.Value);
                    }

                    foreach (var f in cSetting.Forbid)
                    {
                        ct.Forbid.Add(f);
                    }
                }

                foreach (var setting in info.Options)
                {
                    Constraint ct = constraints.Find(c => c.ModID.Equals(pItem.ModID) && c.Setting.Equals(setting.ID, StringComparison.InvariantCultureIgnoreCase));
                    if (ct == null)
                    {
                        ct = new Constraint() { ModID = pItem.ModID, Setting = setting.ID };
                        constraints.Add(ct);
                    }
                    ct.Option = setting;
                }

            }

            return constraints;
        }

        internal static bool OSHasAutoMountSupport()
        {
            Version osVersion = Environment.OSVersion.Version;
            if (osVersion.Major < 6)
            {
                return false;
            }
            else if (osVersion.Major == 6)
            {
                if (osVersion.Minor < 2)
                {
                    return false; // on an OS below Win 8
                }
            }

            return true;
        }

        public static bool IsReunionModInstalled()
        {
            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe);


            if (!Directory.Exists(installPath))
            {
                return false;
            }

            return Directory.GetFiles(installPath).Any(s => new FileInfo(s).Name.Equals("ddraw.dll", StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool EnableOrDisableReunionMod(bool doEnable)
        {
            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe);

            if (!Directory.Exists(installPath))
            {
                return true;
            }

            try
            {
                string pathToDll = Path.Combine(installPath, "ddraw.dll");
                string backupName = Path.Combine(installPath, "Reunion.dll.bak");

                // disable Reunion by renaming ddraw.dll to Reunion.dll.bak
                if (!doEnable)
                {
                    if (File.Exists(pathToDll))
                    {
                        File.Move(pathToDll, backupName);
                        return true;
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CouldNotFindDdrawDllAt)} {pathToDll}", NLog.LogLevel.Warn);
                        return false;
                    }
                }
                else
                {
                    if (File.Exists(backupName))
                    {
                        File.Move(backupName, pathToDll);
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Renamed)} {backupName} -> {pathToDll}");
                        return true;
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.CouldNotFindReunionDllBakAt)} {backupName}", NLog.LogLevel.Warn);
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Scans all drives looking for the drive labeled "FF7DISC1", "FF7DISC2", or "FF7DISC3" and returns the corresponding drive letter.
        /// If not found returns empty string. Returns the drive letter for <paramref name="labelToFind"/> if not null
        /// </summary>
        public static string GetDriveLetter(string labelToFind = null)
        {
            List<string> labels = null;
            if (string.IsNullOrWhiteSpace(labelToFind))
            {
                labels = new List<string>() { "FF7DISC1", "FF7DISC2", "FF7DISC3" };
            }
            else
            {
                labels = new List<string>() { labelToFind };
            }

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && labels.Any(s => s.Equals(drive.VolumeLabel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return drive.Name;
                }
            }

            return "";
        }

        /// <summary>
        /// Scans all drives looking for the drive labeled "FF7DISC1", "FF7DISC2", or "FF7DISC3" and returns the corresponding drive letters that have the matching label.
        /// If not found returns empty string. Returns the drive letter for <paramref name="labelToFind"/> if not null
        /// </summary>
        public static List<string> GetDriveLetters(string labelToFind = null)
        {
            List<string> labels = null;
            if (string.IsNullOrWhiteSpace(labelToFind))
            {
                labels = new List<string>() { "FF7DISC1", "FF7DISC2", "FF7DISC3" };
            }
            else
            {
                labels = new List<string>() { labelToFind };
            }

            List<string> drivesFound = new List<string>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && labels.Any(s => s.Equals(drive.VolumeLabel, StringComparison.InvariantCultureIgnoreCase)))
                {
                    drivesFound.Add(drive.Name);
                }
            }

            return drivesFound;
        }

        /// <summary>
        /// Updates Registry with new values from <see cref="Sys.Settings.GameLaunchSettings"/>
        /// </summary>
        public void SetRegistryValues()
        {
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ApplyingChangedValuesToRegistry));

            string ff7KeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII";
            string virtualStorePath = $"{RegistryHelper.GetKeyPath(FF7RegKey.VirtualStoreKeyPath)}\\Final Fantasy VII";

            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string pathToData = Path.Combine(installPath, @"data\");
            string pathToMovies = Sys.Settings.MovieFolder;

            if (installPath != null && !installPath.EndsWith(@"\"))
            {
                installPath += @"\";
            }

            if (pathToMovies != null && !pathToMovies.EndsWith(@"\"))
            {
                pathToMovies += @"\";
            }


            // Add registry key values for paths and drive letter
            SetValueIfChanged(ff7KeyPath, "AppPath", installPath);
            SetValueIfChanged(ff7KeyPath, "DataPath", pathToData);
            SetValueIfChanged(ff7KeyPath, "MoviePath", pathToMovies);


            // setting the drive letter may not happen if auto update disc path is not set
            if (Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath && !string.IsNullOrWhiteSpace(DriveLetter))
            {
                SetValueIfChanged(ff7KeyPath, "DataDrive", DriveLetter);
            }

            SetValueIfChanged(ff7KeyPath, "DiskNo", 0, RegistryValueKind.DWord);
            SetValueIfChanged(ff7KeyPath, "FullInstall", 1, RegistryValueKind.DWord);


            if (Environment.Is64BitOperatingSystem)
            {
                SetValueIfChanged(RegistryHelper.FF7AppKeyPath64Bit, "Path", installPath.TrimEnd('\\'));
                SetValueIfChanged(RegistryHelper.FF7AppKeyPath32Bit, "Path", installPath.TrimEnd('\\'));
            }
            else
            {
                SetValueIfChanged(RegistryHelper.FF7AppKeyPath32Bit, "Path", installPath.TrimEnd('\\'));
            }


            // Add registry key values for Graphics
            string graphicsKeyPath = $"{ff7KeyPath}\\1.00\\Graphics";
            string graphicsVirtualKeyPath = $"{virtualStorePath}\\1.00\\Graphics";

            SetValueIfChanged(graphicsKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);
            SetValueIfChanged(graphicsVirtualKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.SelectedRenderer == 3)
            {
                SetValueIfChanged(graphicsKeyPath, "DriverPath", "7H_GameDriver.dll");
                SetValueIfChanged(graphicsVirtualKeyPath, "DriverPath", "7H_GameDriver.dll");

                SetValueIfChanged(graphicsKeyPath, "Mode", 2, RegistryValueKind.DWord);
                SetValueIfChanged(graphicsVirtualKeyPath, "Mode", 2, RegistryValueKind.DWord);

                SetValueIfChanged(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                SetValueIfChanged(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
            }
            else
            {
                SetValueIfChanged(graphicsKeyPath, "DriverPath", "");
                SetValueIfChanged(graphicsVirtualKeyPath, "DriverPath", "");

                int mode = Sys.Settings.GameLaunchSettings.FullScreenMode ? 2 : 1;

                SetValueIfChanged(graphicsKeyPath, "Mode", mode, RegistryValueKind.DWord);
                SetValueIfChanged(graphicsVirtualKeyPath, "Mode", mode, RegistryValueKind.DWord);

                if (mode == 1)
                {
                    SetValueIfChanged(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                    SetValueIfChanged(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                }
                else if (Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption)
                {
                    SetValueIfChanged(graphicsKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);
                    SetValueIfChanged(graphicsVirtualKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);
                }
                else if (Sys.Settings.GameLaunchSettings.UseTntGraphicsOption)
                {
                    SetValueIfChanged(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                    SetValueIfChanged(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                }
                else
                {
                    SetValueIfChanged(graphicsKeyPath, "Options", 0, RegistryValueKind.DWord);
                    SetValueIfChanged(graphicsVirtualKeyPath, "Options", 0, RegistryValueKind.DWord);
                }
            }

            byte[] emptyGuidBytes = Guid.Empty.ToByteArray();

            SetValueIfChanged(graphicsKeyPath, "DD_GUID", emptyGuidBytes, RegistryValueKind.Binary);
            SetValueIfChanged(graphicsVirtualKeyPath, "DD_GUID", emptyGuidBytes, RegistryValueKind.Binary);


            // Add registry key values for MIDI
            string midiKeyPath = $"{ff7KeyPath}\\1.00\\MIDI";
            string midiVirtualKeyPath = $"{virtualStorePath}\\1.00\\MIDI";

            SetValueIfChanged(midiKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);
            SetValueIfChanged(midiVirtualKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);

            SetValueIfChanged(midiKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);
            SetValueIfChanged(midiVirtualKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);

            SetValueIfChanged(midiKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.MusicVolume, RegistryValueKind.DWord);
            SetValueIfChanged(midiVirtualKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.MusicVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl)
            {
                SetValueIfChanged(midiKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
                SetValueIfChanged(midiVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                SetValueIfChanged(midiKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
                SetValueIfChanged(midiVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }

            // Add registry key values for Sound
            string soundKeyPath = $"{ff7KeyPath}\\1.00\\Sound";
            string soundVirtualKeyPath = $"{virtualStorePath}\\1.00\\Sound";

            byte[] soundGuidBytes = Sys.Settings.GameLaunchSettings.SelectedSoundDevice.ToByteArray();

            SetValueIfChanged(soundKeyPath, "Sound_GUID", soundGuidBytes, RegistryValueKind.Binary);
            SetValueIfChanged(soundVirtualKeyPath, "Sound_GUID", soundGuidBytes, RegistryValueKind.Binary);

            SetValueIfChanged(soundKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SfxVolume, RegistryValueKind.DWord);
            SetValueIfChanged(soundVirtualKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SfxVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.ReverseSpeakers)
            {
                SetValueIfChanged(soundKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
                SetValueIfChanged(soundVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                SetValueIfChanged(soundKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
                SetValueIfChanged(soundVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Update Registry with new value if it has changed from the current value in the Registry.
        /// Logs the changed value.
        /// </summary>
        /// <param name="regKeyPath"></param>
        /// <param name="regValueName"></param>
        /// <param name="newValue"></param>
        /// <param name="valueKind"></param>
        private void SetValueIfChanged(string regKeyPath, string regValueName, object newValue, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            object currentValue = RegistryHelper.GetValue(regKeyPath, regValueName, null);
            string valueFormatted = newValue?.ToString(); // used to display the object value correctly in the log e.g. for a byte[] convert it to readable string
            bool isValuesEqual;

            if (newValue is byte[])
            {
                string currentConverted = currentValue == null ? "" : BitConverter.ToString(currentValue as byte[]);
                string newConverted = newValue == null ? "" : BitConverter.ToString(newValue as byte[]);

                isValuesEqual = currentConverted != null && currentConverted.Equals(newConverted);
                valueFormatted = newConverted;
            }
            else
            {
                isValuesEqual = currentValue != null && currentValue.Equals(newValue);
            }

            if (!isValuesEqual)
            {
                Instance.RaiseProgressChanged($"\t {regKeyPath}::{regValueName} = {valueFormatted}");
                RegistryHelper.SetValue(regKeyPath, regValueName, newValue, valueKind);
            }
        }

        /// <summary>
        /// Set/Delete compatibility flags in registry for ~ 640x480 HIGHDPIAWARE
        /// </summary>
        public void SetCompatibilityFlagsInRegistry()
        {
            Instance.RaiseProgressChanged(ResourceHelper.Get(StringKey.ApplyingCompatibilityFlagsInRegistry));

            RegistryKey ff7CompatKey;
            string keyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

            // delete compatibility flags if set in launch settings
            if (!Sys.Settings.GameLaunchSettings.Code5Fix && !Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Code5FixHighDpiFixSetToFalseDeletingFlags)}");
                ff7CompatKey = Registry.CurrentUser.OpenSubKey(keyPath, true);

                if (ff7CompatKey != null && ff7CompatKey.GetValue(Sys.Settings.FF7Exe) != null)
                {
                    Instance.RaiseProgressChanged($"\t\t{ResourceHelper.Get(StringKey.CompatibilityFlagsFoundDeleting)}");
                    ff7CompatKey.DeleteValue(Sys.Settings.FF7Exe);
                }

                return;
            }


            string compatString = "~ ";
            // Add 640x480 compatibility flag if set in settings
            if (Sys.Settings.GameLaunchSettings.Code5Fix)
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Code5FixSetToTrueApplyingCompatibilityFlag)}");
                compatString += "640x480 ";
            }

            if (Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.HighDpiFixSetToTrueApplyingCompatibilityFlag)}");
                compatString += "HIGHDPIAWARE";
            }

            Instance.RaiseProgressChanged($"\t HKEY_CURRENT_USER\\{keyPath}::{Sys.Settings.FF7Exe} = {compatString}");
            ff7CompatKey = Registry.CurrentUser.OpenSubKey(keyPath, true);
            ff7CompatKey?.SetValue(Sys.Settings.FF7Exe, compatString);
        }

        /// <summary>
        /// Mounts FF7DISC1.ISO in 'Resources' folder to virtual drive (if Win 8+)
        /// </summary>
        /// <returns></returns>
        public static bool MountIso()
        {
            Version osVersion = Environment.OSVersion.Version;
            if (osVersion.Major < 6)
            {
                return false;
            }
            else if (osVersion.Major == 6)
            {
                if (osVersion.Minor < 2)
                {
                    return false; // on an OS below Win 8
                }
            }

            try
            {
                string isoPath = Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO");

                if (!File.Exists(isoPath))
                {
                    return false;
                }

                lock (_mountLock)
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.AddCommand("Mount-DiskImage");
                        ps.AddParameter("ImagePath", isoPath);

                        Logger.Info($"\tattempting to mount iso at {isoPath}");
                        System.Collections.ObjectModel.Collection<PSObject> result = ps.Invoke();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Unmounts FF7DISC1
        /// </summary>
        /// <remarks>
        /// This assumes the iso image at Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO") is the mounted iso.
        /// ... this code does not handle unmounting other iso files.
        /// </remarks>
        /// <returns></returns>
        public static bool UnmountIso()
        {
            if (!OSHasAutoMountSupport())
            {
                return false;
            }

            try
            {
                string isoPath = Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO");

                if (!File.Exists(isoPath))
                {
                    return false;
                }

                lock (_mountLock)
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        System.Collections.ObjectModel.Collection<PSObject> result = ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// returns true if mounted FF7DISC is a virtual iso (by checking for game files on the drive)
        /// </summary>
        /// <param name="driveLetter"></param>
        public static bool IsVirtualIsoMounted(string driveLetter)
        {
            DriveInfo driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == driveLetter);

            if (driveInfo == null)
            {
                return false;
            }

            // the .iso we mount has the following attributes so check for these
            return driveInfo.DriveFormat == "UDF" && driveInfo.DriveType == DriveType.CDRom && driveInfo.TotalFreeSpace == 0;
        }

        public static bool CopyKeyboardInputCfg()
        {
            Directory.CreateDirectory(Sys.PathToControlsFolder);
            string pathToCfg = Path.Combine(Sys.PathToControlsFolder, Sys.Settings.GameLaunchSettings.InGameConfigOption);

            Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.UsingControlConfigurationFile)} {Sys.Settings.GameLaunchSettings.InGameConfigOption} ...");
            if (!File.Exists(pathToCfg))
            {
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.InputCfgFileNotFoundAt)} {pathToCfg}", NLog.LogLevel.Warn);
                return false;
            }

            try
            {
                string targetPath = Path.Combine(Path.GetDirectoryName(Sys.Settings.FF7Exe), "ff7input.cfg");

                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.Copying)} {pathToCfg} -> {targetPath} ...");
                File.Copy(pathToCfg, targetPath, true);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.FailedToCopyCfgFile)}: {e.Message}", NLog.LogLevel.Error);
                return false;
            }

            return true;
        }

        public static bool IsFF7Running()
        {
            string fileName = Path.GetFileNameWithoutExtension(Sys.Settings.FF7Exe);
            return Process.GetProcessesByName(fileName).Length > 0;
        }

        private static bool ForceKillFF7()
        {
            string fileName = Path.GetFileNameWithoutExtension(Sys.Settings.FF7Exe);

            try
            {
                foreach (Process item in Process.GetProcessesByName(fileName))
                {
                    item.Kill();
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }


        /// <summary>
        /// Kills any currently running process found in <see cref="_sideLoadProcesses"/>
        /// </summary>
        private void StopAllSideProcessesForMods()
        {
            foreach (var valuePair in _sideLoadProcesses.ToList())
            {
                _7thWrapperLib.ProgramInfo info = valuePair.Key;
                Process sideProc = valuePair.Value;
                string procName = sideProc.ProcessName;

                if (!sideProc.HasExited)
                {
                    sideProc.Kill();
                }

                // Kill all instances with same process name if necessary
                if (info.CloseAllInstances)
                {
                    foreach (Process otherProc in Process.GetProcessesByName(procName))
                    {
                        if (!otherProc.HasExited)
                            otherProc.Kill();
                    }
                }
            }
        }

        internal void LaunchProgramsForMod(RuntimeMod mod)
        {
            if (!mod.LoadPrograms.Any())
            {
                return;
            }

            mod.Startup();

            foreach (var program in mod.GetLoadPrograms())
            {
                if (!_sideLoadProcesses.ContainsKey(program))
                {
                    Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.StartingProgram)}: {program.PathToProgram}");
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(program.PathToProgram),
                        FileName = program.PathToProgram,
                        Arguments = program.ProgramArgs,
                        UseShellExecute = false,
                    };
                    Process aproc = Process.Start(psi);

                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _sideLoadProcesses.Remove(program);

                    _sideLoadProcesses.Add(program, aproc);
                    Instance.RaiseProgressChanged($"\t\t{ResourceHelper.Get(StringKey.Started)}");

                    IntPtr mainWindowHandle = IntPtr.Zero;

                    if (program.WaitForWindowToShow)
                    {
                        Instance.RaiseProgressChanged($"\t\t{ResourceHelper.Get(StringKey.WaitingForProgramToInitializeAndShow)}");

                        DateTime startTime = DateTime.Now;

                        while (aproc.MainWindowHandle == IntPtr.Zero && mainWindowHandle == IntPtr.Zero)
                        {
                            if (program.WaitTimeOutInSeconds != 0 && DateTime.Now.Subtract(startTime).TotalSeconds > program.WaitTimeOutInSeconds)
                            {
                                Instance.RaiseProgressChanged($"\t\t{string.Format(ResourceHelper.Get(StringKey.TimeoutReachedWaitingForProgramToShow), program.WaitTimeOutInSeconds)}", NLog.LogLevel.Warn);
                                break;
                            }

                            aproc.Refresh();

                            if (!string.IsNullOrWhiteSpace(program.WindowTitle))
                            {
                                // attempt to find main window handle based on the Window Title of the process
                                // ... necessary because some programs (Speed Hack) will open multiple processes and the Main Window Handle needs to be found since it is different from the original process started
                                mainWindowHandle = FindWindow(null, program.WindowTitle);
                            }

                        };
                    }

                    // force the process to become minimized
                    if (aproc.MainWindowHandle != IntPtr.Zero || mainWindowHandle != IntPtr.Zero)
                    {
                        Logger.Info($"\t\tforce minimizing program ...");
                        IntPtr handleToMinimize = aproc.MainWindowHandle != IntPtr.Zero ? aproc.MainWindowHandle : mainWindowHandle;

                        System.Threading.Thread.Sleep(1500); // add a small delay to ensure the program is showing in taskbar before force minimizing;
                        ShowWindow(handleToMinimize.ToInt32(), SW_FORCEMINIMIZE);
                    }
                }
                else
                {
                    if (!_sideLoadProcesses[program].HasExited)
                    {
                        Instance.RaiseProgressChanged($"\t{ResourceHelper.Get(StringKey.ProgramAlreadyRunning)}: {program.PathToProgram}", NLog.LogLevel.Warn);
                    }
                }
            }
        }


        /// <summary>
        /// Starts the processes with the specified arguments that are set in <see cref="Sys.Settings.ProgramsToLaunchPrior"/>.
        /// </summary>
        internal void LaunchAdditionalProgramsToRunPrior()
        {
            // launch other processes set in settings
            foreach (ProgramLaunchInfo al in Sys.Settings.ProgramsToLaunchPrior.Where(s => !String.IsNullOrWhiteSpace(s.PathToProgram)))
            {
                try
                {
                    if (!_alsoLaunchProcesses.ContainsKey(al.PathToProgram))
                    {
                        ProcessStartInfo psi = new ProcessStartInfo()
                        {
                            WorkingDirectory = Path.GetDirectoryName(al.PathToProgram),
                            FileName = al.PathToProgram,
                            Arguments = al.ProgramArgs
                        };
                        Process aproc = Process.Start(psi);

                        _alsoLaunchProcesses.Add(al.PathToProgram, aproc);
                        aproc.EnableRaisingEvents = true;
                        aproc.Exited += (_o, _e) => _alsoLaunchProcesses.Remove(al.PathToProgram);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(e);
                    Instance.RaiseProgressChanged($"\tFailed to start additional program: {al.PathToProgram}", NLog.LogLevel.Warn);
                }
            }
        }

        internal void RaiseProgressChanged(string messageToLog, NLog.LogLevel logLevel = null)
        {
            if (logLevel == null)
            {
                logLevel = NLog.LogLevel.Info;
            }

            Logger.Log(logLevel, messageToLog);
            ProgressChanged?.Invoke(messageToLog);
        }

        internal void RaiseLaunchCompleted(bool didLaunch)
        {
            LaunchCompleted?.Invoke(didLaunch);
        }



    }
}
