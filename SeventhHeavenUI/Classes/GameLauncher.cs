using _7thHeaven.Code;
using _7thWrapperLib;
using Iros._7th;
using Iros._7th.Workshop;
using Microsoft.Win32;
using SeventhHeaven.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.Classes
{
    public enum FF7Version
    {
        Unknown = -1,
        Steam,
        ReRelease,
        Original98
    }

    /// <summary>
    /// Responsibile for the entire process that happens for launching the game
    /// </summary>
    public class GameLauncher
    {
        #region Data Members and Properties

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static Dictionary<string, _7thWrapperLib.ModInfo> _infoCache = new Dictionary<string, _7thWrapperLib.ModInfo>(StringComparer.InvariantCultureIgnoreCase);
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

        public delegate void OnProgressChanged(string message);
        public event OnProgressChanged ProgressChanged;

        public FF7Version InstallVersion { get; set; }
        public string DriveLetter { get; set; }

        #endregion

        public static bool LaunchGame(bool varDump, bool debug)
        {
            Instance.RaiseProgressChanged("Checking mod compatibility requirements ...");
            if (!SanityCheckCompatibility())
            {
                Instance.RaiseProgressChanged("\tfailed mod compatibility check. aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged("Checking mod constraints for compatibility ...");
            if (!SanityCheckSettings())
            {
                Instance.RaiseProgressChanged("\tfailed mod constraint check. aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged("Checking mod load order requirements ...");
            if (!VerifyOrdering())
            {
                Instance.RaiseProgressChanged("\tfailed mod load order check. aborting ...");
                return false;
            }

            Instance.RaiseProgressChanged("Checking a profile is active ...");
            if (Sys.ActiveProfile == null)
            {
                Instance.RaiseProgressChanged("\tactive profile not found. aborting ...");
                MessageDialogWindow.Show("Create a profile first", "Missing Profile");
                return false;
            }


            Instance.RaiseProgressChanged($"Checking FF7 .exe exists at {Sys.Settings.FF7Exe} ...");
            if (!File.Exists(Sys.Settings.FF7Exe))
            {
                Instance.RaiseProgressChanged("\tfile not found. aborting ...");
                MessageDialogWindow.Show("FF7.exe not found. You may need to configure 7H using the Workshop/Settings menu.", "FF7.exe Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            Instance.RaiseProgressChanged("Checking if Reunion mod is installed ...");
            Instance.RaiseProgressChanged($"\tfound: {IsReunionModInstalled()}");


            if (IsReunionModInstalled() && Sys.Settings.GameLaunchSettings.DisableReunionOnLaunch)
            {
                Instance.RaiseProgressChanged("Disabling Reunion mod by renaming ddraw.dll to Reunion.dll.bak ...");
                DisableReunionMod();
            }

            Instance.RaiseProgressChanged("Looking for game disc ...");
            Instance.DriveLetter = GetDriveLetter();

            if (!string.IsNullOrEmpty(Instance.DriveLetter))
            {
                Instance.RaiseProgressChanged($"Found game disc at {Instance.DriveLetter} ...");
            }
            else
            {
                Instance.RaiseProgressChanged($"Failed to find game disc ...");

                if (!OSHasAutoMountSupport())
                {
                    Instance.RaiseProgressChanged($"OS does not support auto mounting virtual disc. You must mount a virtual disk named FF7DISC.ISO, insert a disk/usb named FF7DISC1, or rename a HDD to FF7DISC1 ...");
                    return false;
                }
                else
                {
                    if (Sys.Settings.GameLaunchSettings.AutoMountGameDisc)
                    {
                        Instance.RaiseProgressChanged($"Auto mounting virtual game disc ...");
                        bool didMount = MountIso();

                        if (!didMount)
                        {
                            Instance.RaiseProgressChanged($"Failed to auto mount virtual disc at {Path.Combine(Sys._7HFolder, "Resources", "FF7DISC1.ISO")} ...");
                            return false;
                        }

                        Instance.RaiseProgressChanged("Looking for game disc after mounting ...");
                        Instance.DriveLetter = GetDriveLetter();

                        if (string.IsNullOrEmpty(Instance.DriveLetter))
                        {
                            Instance.RaiseProgressChanged($"Failed to find game disc after auto mounting ...");
                            return false;
                        }

                        Instance.RaiseProgressChanged($"Found game disc at {Instance.DriveLetter} ...");
                    }
                }
            }

            Instance.SetRegistryValues();
            Instance.SetCompatibilityFlagsInRegistry();

            if (Sys.ActiveProfile.ActiveItems.Count == 0)
            {
                string vanillaMsg = "No mods have been activated. The game will now launch as 'vanilla'";
                Instance.RaiseProgressChanged(vanillaMsg);
                Sys.Message(new WMessage(vanillaMsg, true));

                LaunchFF7Exe();
                return true;
            }


            Instance.RaiseProgressChanged("Creating Runtime Profile ...");
            RuntimeProfile runtimeProfile = CreateRuntimeProfile();

            if (runtimeProfile == null)
            {
                Instance.RaiseProgressChanged("\tfailed to create Runtime Profile for active mods ...");
                return false;
            }


            if (varDump)
            {
                Instance.RaiseProgressChanged("Variable Dump set to true. Starting TurBoLog ...");
                StartTurboLogForVariableDump(runtimeProfile);
            }

            // copy EasyHook.dll to FF7
            Instance.RaiseProgressChanged("Copying EasyHook.dll to FF7 path (if not found or older version detected) ...");
            CopyEasyHookDlls();


            // setup log file if debugging
            if (debug)
            {
                runtimeProfile.Options |= RuntimeOptions.DetailedLog;
                runtimeProfile.LogFile = Path.Combine(Sys.SysFolder, "log.txt");

                Instance.RaiseProgressChanged($"Debug Logging set to true. Detailed logging will be written to {runtimeProfile.LogFile} ...");
            }

            int pid;
            try
            {
                Instance.RaiseProgressChanged($"Launching additional programs to run (if any) ...");
                Instance.LaunchAdditionalProgramsToRunPrior();

                RuntimeParams parms = new RuntimeParams
                {
                    ProfileFile = Path.GetTempFileName()
                };

                Instance.RaiseProgressChanged($"Writing temporary runtime profile file to {parms.ProfileFile} ...");

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
                        Instance.RaiseProgressChanged($"Attempting to inject with EasyHook: try # {attemptCount + 1} ...");

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

                                Instance.RaiseProgressChanged($"\treceived errors: {errors} ...");
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
                                Instance.RaiseProgressChanged($"\treached timeout waiting for injection ...");
                                didInject = false;
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Instance.RaiseProgressChanged($"\treceived unknown error: {e.Message} ...");
                    }
                    finally
                    {
                        attemptCount++;
                    }

                    // If the CreateAndInject() process fails to inject it will attempt to kill the process it started...
                    // ... this could fail and leave the process open which causes problems for the next attempt so make sure the process is killed on failure
                    if (!didInject)
                    {
                        var openProcs = Process.GetProcessesByName("ff7");
                        foreach (Process proc in openProcs)
                        {
                            if (!proc.HasExited)
                                proc.Kill();
                        }
                    }
                }

                if (!didInject)
                {
                    Instance.RaiseProgressChanged($"Failed to inject after max amount of tries ({totalAttempts}) ...");

                    // give user option to set compat flag and try again
                    var viewModel = MessageDialogWindow.Show("Failed to inject with EasyHook after trying multiple times. This is usually fixed by setting the 'Code 5 Fix' to 'On' in the Game Launcher Settings.\n\nDo you want to apply the setting and try again?",
                                                             "Error - Failed To Start Game",
                                                             MessageBoxButton.YesNo,
                                                             MessageBoxImage.Warning);

                    if (viewModel.Result == MessageBoxResult.Yes)
                    {
                        Instance.RaiseProgressChanged($"Setting compatibility fix and trying again ...");
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
                                Instance.RaiseProgressChanged($"Still failed to inject with EasyHook after setting compatibility fix. aborting ...");
                                MessageDialogWindow.Show("Failed inject with EasyHook even after setting the compatibility flags", "Failed To Start Game", MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Instance.RaiseProgressChanged($"\tuser chose not to set compatibility fix. aborting ...");
                        return false;
                    }
                }


                Instance.RaiseProgressChanged("Getting ff7 proc ...");
                var ff7Proc = Process.GetProcessById(pid);
                if (ff7Proc != null)
                {
                    ff7Proc.EnableRaisingEvents = true;
                    if (debug)
                    {
                        Instance.RaiseProgressChanged("debug logging set to true. wiring up turbolog file to open after game exit ...");
                        ff7Proc.Exited += (o, e) =>
                        {
                            Process.Start(runtimeProfile.LogFile);
                        };
                    }
                }

                /// load plugins and sideload other programs for mods
                Instance.RaiseProgressChanged("Starting plugins and programs for mods ...");
                foreach (RuntimeMod mod in runtimeProfile.Mods)
                {
                    if (mod.LoadPlugins.Any())
                    {
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
                                Instance.RaiseProgressChanged($"\tplugin added: {dll}");
                            }

                            Instance.RaiseProgressChanged($"\tstarting plugin: {dll}");
                            plugin.Start(mod);
                        }
                    }

                    Instance.LaunchProgramsForMod(mod);
                }

                // wire up process to stop plugins and side processes when proc has exited
                Instance.RaiseProgressChanged("Setting up FF7 .exe to stop plugins and mod programs after exiting ...");
                ff7Proc.Exited += (o, e) =>
                {
                    foreach (var plugin in Instance._plugins.Values)
                        plugin.Stop();

                    Instance.StopAllSideProcessesForMods();

                    if (Sys.Settings.GameLaunchSettings.AutoUnmountGameDisc)
                    {
                        UnmountIso();
                    }
                };


                Instance.RaiseProgressChanged("Waiting for FF7 .exe to respond ...");
                DateTime start = DateTime.Now;
                int secondsToWait = 10;
                while (ff7Proc.Responding == false)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(start);
                    if (elapsed.Seconds > secondsToWait)
                        break;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);


                Instance.RaiseProgressChanged("Exception occurred while trying to start FF7 ...");
                MessageDialogWindow.Show(e.ToString(), "Error starting FF7", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
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
                Instance.RaiseProgressChanged($"\tfailed to get list of Runtime Mods due to a missing variable for a mod: {aex.Message}");
                return null;
            }
            catch (Exception e)
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

            Instance.RaiseProgressChanged("\tadding paths to monitor ...");
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
                    Instance.RaiseProgressChanged("\tEasyHook.dll detected to be older version ...");
                    hasOldVersion = true;
                }
            }



            if (!File.Exists(pathToTargetEasyHook) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook, pathToTargetEasyHook, true);
                Instance.RaiseProgressChanged("\tEasyHook.dll copied ...");
            }
            else
            {
                Instance.RaiseProgressChanged("\tskipped copying EasyHook.dll ...");

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
                    Instance.RaiseProgressChanged("\tEasyHook32.dll detected to be older version ...");
                    hasOldVersion = true;
                }
            }


            if (!File.Exists(pathToTargetEasyHook32) || hasOldVersion)
            {
                File.Copy(pathToSourceEasyHook32, pathToTargetEasyHook32, true);
                Instance.RaiseProgressChanged("\tEasyHook32.dll copied ...");
            }
            else
            {
                Instance.RaiseProgressChanged("\tskipped copying EasyHook32.dll ...");
            }
        }

        private static void StartTurboLogForVariableDump(RuntimeProfile runtimeProfiles)
        {
            string turboLogProcName = "TurBoLog.exe";
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
                WorkingDirectory = Path.GetDirectoryName(turboLogProcName)
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
            // remove the flag for 640x480 when playing vanilla since Easy Hook is not being used
            if (Sys.Settings.HasOption(GeneralOptions.SetEXECompatFlags))
            {
                RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);

                try
                {
                    ff7CompatKey?.DeleteValue(Sys.Settings.FF7Exe);
                }
                catch (Exception e)
                {
                    // will fail if already deleted
                }
            }

            try
            {
                Process.Start(Sys.Settings.FF7Exe);
                return true;
            }
            catch (Exception ex)
            {
                Instance.RaiseProgressChanged($"An exception occurred while trying to start FF7 at {Sys.Settings.FF7Exe} ...");
                Logger.Error(ex);
                return false;
            }
        }

        internal static void RemoveFromInfoCache(string mfile)
        {
            if (_infoCache.ContainsKey(mfile))
            {
                _infoCache.Remove(mfile);
            }
        }

        internal static bool SanityCheckSettings()
        {
            List<string> changes = new List<string>();
            foreach (var constraint in GetConstraints())
            {
                if (!constraint.Verify(out string msg))
                {
                    MessageDialogWindow.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (msg != null)
                {
                    changes.Add(msg);
                }
            }

            if (changes.Any())
            {
                MessageDialogWindow.Show($"The following settings have been changed to make these mods compatible:\n{String.Join("\n", changes)}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return true;
        }

        internal static bool SanityCheckCompatibility()
        {
            List<InstalledItem> profInst = Sys.ActiveProfile.ActiveItems.Select(pi => Sys.Library.GetItem(pi.ModID)).ToList();

            foreach (InstalledItem item in profInst)
            {
                var info = GetModInfo(item);

                if (info == null)
                {
                    continue;
                }

                foreach (var req in info.Compatibility.Requires)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(req.ModID));
                    if (rInst == null)
                    {
                        MessageDialogWindow.Show(String.Format("Mod {0} requires you to activate {1} as well.", item.CachedDetails.Name, req.Description), "Missing Required Activation");
                        return false;
                    }
                    else if (req.Versions.Any() && !req.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show(String.Format("Mod {0} requires you to activate {1}, but you do not have a compatible version installed. Try updating it?", item.CachedDetails.Name, rInst.CachedDetails.Name), "Unsupported Mod Version");
                        return false;
                    }
                }

                foreach (var forbid in info.Compatibility.Forbids)
                {
                    var rInst = profInst.Find(i => i.ModID.Equals(forbid.ModID));
                    if (rInst == null)
                    {
                        continue; //good!
                    }

                    if (forbid.Versions.Any() && forbid.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version))
                    {
                        MessageDialogWindow.Show($"Mod {item.CachedDetails.Name} is not compatible with the version of {rInst.CachedDetails.Name} you have installed. Try updating it?", "Incompatible Mod");
                        return false;
                    }
                    else
                    {
                        MessageDialogWindow.Show($"Mod {item.CachedDetails.Name} is not compatible with {rInst.CachedDetails.Name}. You will need to disable it.", "Incompatible Mod");
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
                             .Select(ii => new { Mod = ii, Info = GetModInfo(ii) })
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
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come BELOW mod {details[after].Mod.CachedDetails.Name} in the load order");
                    }
                }

                foreach (Guid before in info.OrderBefore)
                {
                    if (Sys.ActiveProfile.ActiveItems.Take(i).Any(pi => pi.ModID.Equals(before)))
                    {
                        problems.Add($"Mod {details[mod.ModID].Mod.CachedDetails.Name} is meant to come ABOVE mod {details[before].Mod.CachedDetails.Name} in the load order");
                    }
                }
            }

            if (problems.Any())
            {
                if (MessageDialogWindow.Show($"The following mods will not work properly in the current order:\n{String.Join("\n", problems)}\nDo you want to continue anyway?", "Load Order Incompatible", MessageBoxButton.YesNo).Result != MessageBoxResult.Yes)
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
                ModInfo info = GetModInfo(inst);

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

                    ct.ParticipatingMods.Add(inst.CachedDetails.Name);
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

        internal static ModInfo GetModInfo(InstalledItem ii)
        {
            InstalledVersion inst = ii.LatestInstalled;
            string mfile = Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);

            ModInfo info = null;

            try
            {
                if (!_infoCache.TryGetValue(mfile, out info))
                {
                    if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (var arc = new IrosArc(mfile))
                            if (arc.HasFile("mod.xml"))
                            {
                                var doc = new System.Xml.XmlDocument();
                                doc.Load(arc.GetData("mod.xml"));
                                info = new ModInfo(doc, Sys._context);
                            }
                    }
                    else
                    {
                        string file = Path.Combine(mfile, "mod.xml");
                        if (File.Exists(file))
                            info = new ModInfo(file, Sys._context);
                    }
                    _infoCache.Add(mfile, info);
                }
            }
            catch (VariableAliasNotFoundException aex)
            {
                // this exception occurrs when the variable alias is not found in .var file which causes ModInfo not to be captured completely. warn user and return null
                Sys.Message(new WMessage($"WARNING: failed to get mod info due to a missing variable which can cause problems: {aex.Message}", true));
                return null;
            }

            return info;
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

            return Directory.GetFiles(installPath).Any(s => s.Contains("ddraw.dll"));
        }

        public static bool DisableReunionMod()
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

                if (File.Exists(pathToDll))
                {
                    File.Move(pathToDll, backupName);
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
        /// Scans all drives looking for the drive labeled "FF7DISC1" and returns the corresponding drive letter.
        /// If not found returns empty string.
        /// </summary>
        public static string GetDriveLetter()
        {
            List<string> labels = new List<string>() { "FF7DISC1" };

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && labels.Any(s => s == drive.VolumeLabel))
                {
                    return drive.Name;
                }
            }

            return "";
        }

        /// <summary>
        /// Updates Registry with new values from <see cref="Sys.Settings.GameLaunchSettings"/>
        /// </summary>
        public void SetRegistryValues()
        {
            Instance.RaiseProgressChanged("Applying values to registry ...");

            string ff7KeyPath = $"{RegistryHelper.GetKeyPath(FF7RegKey.SquareSoftKeyPath)}\\Final Fantasy VII";
            string virtualStorePath = $"{RegistryHelper.GetKeyPath(FF7RegKey.VirtualStoreKeyPath)}\\Final Fantasy VII";

            string installPath = Path.GetDirectoryName(Sys.Settings.FF7Exe) + @"\";

            // Add registry key values for paths and drive letter
            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::AppPath = {installPath}");
            RegistryHelper.SetValue(ff7KeyPath, "AppPath", installPath);
            RegistryHelper.SetValue(virtualStorePath, "AppPath", installPath);

            string pathToData = Path.Combine(installPath, @"data\");
            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DataPath = {pathToData}");

            RegistryHelper.SetValue(ff7KeyPath, "DataPath", pathToData);
            RegistryHelper.SetValue(virtualStorePath, "DataPath", pathToData);

            string pathToMovies = Path.Combine(installPath, "data", @"movies\");
            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::MoviePath = {pathToMovies}");

            RegistryHelper.SetValue(ff7KeyPath, "MoviePath", pathToMovies);
            RegistryHelper.SetValue(virtualStorePath, "MoviePath", pathToMovies);

            // setting the drive letter may not happen if auto update disc path is not set
            if (Sys.Settings.GameLaunchSettings.AutoUpdateDiscPath && !string.IsNullOrWhiteSpace(DriveLetter))
            {
                Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DataDrive = {DriveLetter}");
                RegistryHelper.SetValue(ff7KeyPath, "DataDrive", DriveLetter);
                RegistryHelper.SetValue(virtualStorePath, "DataDrive", DriveLetter);
            }

            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::DiskNo = 0");
            RegistryHelper.SetValue(ff7KeyPath, "DiskNo", 0, RegistryValueKind.DWord);
            RegistryHelper.SetValue(virtualStorePath, "DiskNo", 0, RegistryValueKind.DWord);

            Instance.RaiseProgressChanged($"\t {ff7KeyPath}::FullInstall = 1");
            RegistryHelper.SetValue(ff7KeyPath, "FullInstall", 1, RegistryValueKind.DWord);
            RegistryHelper.SetValue(virtualStorePath, "FullInstall", 1, RegistryValueKind.DWord);


            Instance.RaiseProgressChanged($"\t {RegistryHelper.GetKeyPath(FF7RegKey.FF7AppKeyPath)}::Path = {installPath}");
            RegistryHelper.SetValue(RegistryHelper.GetKeyPath(FF7RegKey.FF7AppKeyPath), "Path", installPath);


            // Add registry key values for Graphics
            string graphicsKeyPath = $"{ff7KeyPath}\\1.00\\Graphics";
            string graphicsVirtualKeyPath = $"{virtualStorePath}\\1.00\\Graphics";

            Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Driver = {Sys.Settings.GameLaunchSettings.SelectedRenderer}");
            RegistryHelper.SetValue(graphicsKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);
            RegistryHelper.SetValue(graphicsVirtualKeyPath, "Driver", Sys.Settings.GameLaunchSettings.SelectedRenderer, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.SelectedRenderer == 3)
            {
                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DriverPath = ff7_opengl.fgd");
                RegistryHelper.SetValue(graphicsKeyPath, "DriverPath", "ff7_opengl.fgd");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "DriverPath", "ff7_opengl.fgd");

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Mode = 2");
                RegistryHelper.SetValue(graphicsKeyPath, "Mode", 2, RegistryValueKind.DWord);
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Mode", 2, RegistryValueKind.DWord);

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x12");
                RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DriverPath = ");
                RegistryHelper.SetValue(graphicsKeyPath, "DriverPath", "");
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "DriverPath", "");

                Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Mode = 1");
                RegistryHelper.SetValue(graphicsKeyPath, "Mode", 1, RegistryValueKind.DWord);
                RegistryHelper.SetValue(graphicsVirtualKeyPath, "Mode", 1, RegistryValueKind.DWord);

                if (Sys.Settings.GameLaunchSettings.UseRiva128GraphicsOption)
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x0000000a");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x0000000a, RegistryValueKind.DWord);
                }
                else if (Sys.Settings.GameLaunchSettings.UseTntGraphicsOption)
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0x12");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0x12, RegistryValueKind.DWord);
                }
                else
                {
                    Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::Options = 0");
                    RegistryHelper.SetValue(graphicsKeyPath, "Options", 0, RegistryValueKind.DWord);
                    RegistryHelper.SetValue(graphicsVirtualKeyPath, "Options", 0, RegistryValueKind.DWord);
                }
            }

            Instance.RaiseProgressChanged($"\t {graphicsKeyPath}::DD_GUID = {Guid.Empty}");
            RegistryHelper.SetValue(graphicsKeyPath, "DD_GUID", Guid.Empty.ToByteArray(), RegistryValueKind.Binary);
            RegistryHelper.SetValue(graphicsVirtualKeyPath, "DD_GUID", Guid.Empty.ToByteArray(), RegistryValueKind.Binary);


            // Add registry key values for MIDI
            string midiKeyPath = $"{ff7KeyPath}\\1.00\\MIDI";
            string midiVirtualKeyPath = $"{virtualStorePath}\\1.00\\MIDI";

            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MIDI_DeviceID = 0");
            RegistryHelper.SetValue(midiKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);
            RegistryHelper.SetValue(midiVirtualKeyPath, "MIDI_DeviceID", 0x00000000, RegistryValueKind.DWord);


            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MIDI_data = {Sys.Settings.GameLaunchSettings.SelectedMidiDevice}");
            RegistryHelper.SetValue(midiKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);
            RegistryHelper.SetValue(midiVirtualKeyPath, "MIDI_data", Sys.Settings.GameLaunchSettings.SelectedMidiDevice);

            Instance.RaiseProgressChanged($"\t {midiKeyPath}::MusicVolume = {Sys.Settings.GameLaunchSettings.SoundVolume}");
            RegistryHelper.SetValue(midiKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.SoundVolume, RegistryValueKind.DWord);
            RegistryHelper.SetValue(midiVirtualKeyPath, "MusicVolume", Sys.Settings.GameLaunchSettings.SoundVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.LogarithmicVolumeControl)
            {
                Instance.RaiseProgressChanged($"\t {midiKeyPath}::Options = 1");
                RegistryHelper.SetValue(midiKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
                RegistryHelper.SetValue(midiVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {midiKeyPath}::Options = 0");
                RegistryHelper.SetValue(midiKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
                RegistryHelper.SetValue(midiVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }

            // Add registry key values for Sound
            string soundKeyPath = $"{ff7KeyPath}\\1.00\\Sound";
            string soundVirtualKeyPath = $"{virtualStorePath}\\1.00\\Sound";

            Instance.RaiseProgressChanged($"\t {soundKeyPath}::Sound_GUID = {Sys.Settings.GameLaunchSettings.SelectedSoundDevice}");
            RegistryHelper.SetValue(soundKeyPath, "Sound_GUID", Sys.Settings.GameLaunchSettings.SelectedSoundDevice.ToByteArray(), RegistryValueKind.Binary);
            RegistryHelper.SetValue(soundVirtualKeyPath, "Sound_GUID", Sys.Settings.GameLaunchSettings.SelectedSoundDevice.ToByteArray(), RegistryValueKind.Binary);

            Instance.RaiseProgressChanged($"\t {soundKeyPath}::SFXVolume = {Sys.Settings.GameLaunchSettings.SoundVolume}");
            RegistryHelper.SetValue(soundKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SoundVolume, RegistryValueKind.DWord);
            RegistryHelper.SetValue(soundVirtualKeyPath, "SFXVolume", Sys.Settings.GameLaunchSettings.SoundVolume, RegistryValueKind.DWord);

            if (Sys.Settings.GameLaunchSettings.ReverseSpeakers)
            {
                Instance.RaiseProgressChanged($"\t {soundKeyPath}::Options = 1");
                RegistryHelper.SetValue(soundKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
                RegistryHelper.SetValue(soundVirtualKeyPath, "Options", 0x00000001, RegistryValueKind.DWord);
            }
            else
            {
                Instance.RaiseProgressChanged($"\t {soundKeyPath}::Options = 0");
                RegistryHelper.SetValue(soundKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
                RegistryHelper.SetValue(soundVirtualKeyPath, "Options", 0x00000000, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// Set/Delete compatibility flags in registry for ~ 640x480 HIGHDPIAWARE
        /// </summary>
        public void SetCompatibilityFlagsInRegistry()
        {
            Instance.RaiseProgressChanged($"Applying compatibility flags in registry (if any) ...");

            RegistryKey ff7CompatKey;
            string keyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

            // delete compatibility flags if set in launch settings
            if (!Sys.Settings.GameLaunchSettings.Code5Fix && !Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged("\t Code 5 fix / High DPI fix set to false - deleting flags if exist ...");
                ff7CompatKey = Registry.CurrentUser.OpenSubKey(keyPath, true);
                if (ff7CompatKey.GetValue(Sys.Settings.FF7Exe) != null)
                {
                    Instance.RaiseProgressChanged("\t\t compatibility flags found - deleting");
                    ff7CompatKey.DeleteValue(Sys.Settings.FF7Exe);
                }

                return;
            }


            string compatString = "~ ";
            // Add 640x480 compatibility flag if set in settings
            if (Sys.Settings.GameLaunchSettings.Code5Fix)
            {
                Instance.RaiseProgressChanged("\t Code 5 fix set to true - applying 640x480 compatibility flag in registry");
                compatString += "640x480 ";
            }

            if (Sys.Settings.GameLaunchSettings.HighDpiFix)
            {
                Instance.RaiseProgressChanged("\t High DPI Fix set to true - applying HIGHDPIAWARE compatibility flag in registry");
                compatString += "HIGHDPIAWARE";
            }

            Instance.RaiseProgressChanged($"\t {keyPath}::{Sys.Settings.FF7Exe} = {compatString}");
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

                using (PowerShell ps = PowerShell.Create())
                {
                    var result = ps.AddCommand("Mount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
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

                using (PowerShell ps = PowerShell.Create())
                {
                    var result = ps.AddCommand("Dismount-DiskImage").AddParameter("ImagePath", isoPath).Invoke();
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
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        WorkingDirectory = Path.GetDirectoryName(program.PathToProgram),
                        FileName = program.PathToProgram,
                        Arguments = program.ProgramArgs,
                        UseShellExecute = false
                    };
                    Process aproc = Process.Start(psi);

                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _sideLoadProcesses.Remove(program);

                    _sideLoadProcesses.Add(program, aproc);
                    Instance.RaiseProgressChanged($"\tstarted program: {program.PathToProgram}");
                }
                else
                {
                    if (!_sideLoadProcesses[program].HasExited)
                    {
                        Instance.RaiseProgressChanged($"\tprogram already running: {program.PathToProgram}");
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
        }

        internal void RaiseProgressChanged(string messageToLog)
        {
            Logger.Info(messageToLog);
            ProgressChanged?.Invoke(messageToLog);
        }
    }
}
