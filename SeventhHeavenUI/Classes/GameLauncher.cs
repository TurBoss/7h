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
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeventhHeaven.Classes
{
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

            Instance.RaiseProgressChanged($"Launching additional programs to run (if any) ...");
            Instance.LaunchAdditionalProgramsToRunPrior();


            if (Sys.ActiveProfile.ActiveItems.Count == 0)
            {
                string vanillaMsg = "No mods have been activated. The game will now launch as 'vanilla'";
                Instance.RaiseProgressChanged(vanillaMsg);
                Sys.Message(new WMessage(vanillaMsg, true));

                LaunchFF7Exe();
                return true;
            }


            Instance.RaiseProgressChanged("Creating Runtime Profile ...");
            RuntimeProfile runtimeProfiles = CreateRuntimeProfile();


            if (varDump)
            {
                Instance.RaiseProgressChanged("Variable Dump set to true. Starting TurBoLog ...");
                StartTurboLogForVariableDump(runtimeProfiles);
            }

            // copy EasyHook.dll to FF7
            Instance.RaiseProgressChanged("Copying EasyHook.dll to FF7 path (if not found) ...");
            CopyEasyHookDlls();


            // setup log file if debugging
            if (debug)
            {
                runtimeProfiles.Options |= _7thWrapperLib.RuntimeOptions.DetailedLog;
                runtimeProfiles.LogFile = Path.Combine(Sys.SysFolder, "log.txt");

                Instance.RaiseProgressChanged($"Debug Logging set to true. Detailed logging will be written to {runtimeProfiles.LogFile} ...");
            }

            int pid;
            try
            {
                RuntimeParams parms = new RuntimeParams
                {
                    ProfileFile = Path.GetTempFileName()
                };

                Instance.RaiseProgressChanged($"Writing tempory runtime profile file to {parms.ProfileFile} ...");

                using (FileStream fs = new FileStream(parms.ProfileFile, FileMode.Create))
                    Util.SerializeBinary(runtimeProfiles, fs);


                // Add 640x480 compatibility flag if set in settings
                if (Sys.Settings.HasOption(GeneralOptions.SetEXECompatFlags))
                {
                    Instance.RaiseProgressChanged("Exe compat flag set, applying 640x480 flag in registry");
                    RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                    ff7CompatKey?.SetValue(Sys.Settings.FF7Exe, "~ 640X480");
                }

                // attempt to launch the game a few times in the case of an ApplicationException that can be thrown by EasyHook it seems randomly at times
                // ... The error tends to go away the second time trying but we will try multiple times before failing
                // ... if we fail to inject with EasyHook then we will give the user a chance to set the compatibility flag to fix the issue
                string lib = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "7thWrapperLib.dll");

                bool didInject = false;
                int attemptCount = 0;
                int totalAttempts = 20;
                pid = -1;

                while (!didInject && attemptCount < totalAttempts)
                {
                    try
                    {
                        Instance.RaiseProgressChanged($"Attempting to inject with EasyHook: try # {attemptCount+1} ...");

                        EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);
                        didInject = true;
                    }
                    catch (ApplicationException aex)
                    {
                        if (aex.Message.IndexOf("Unknown error in injected assembler code", StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            didInject = false;
                            attemptCount++;
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

                        RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                        ff7CompatKey?.SetValue(Sys.Settings.FF7Exe, "~ 640X480");

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
                            Process.Start(runtimeProfiles.LogFile);
                        };
                    }
                }

                /// load plugins and sideload other programs for mods
                Instance.RaiseProgressChanged("Starting plugins and programs for mods ...");
                foreach (RuntimeMod mod in runtimeProfiles.Mods)
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
                };

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
                Mods = Sys.ActiveProfile.ActiveItems.Select(i => i.GetRuntime(Sys._context))
                                                    .Where(i => i != null)
                                                    .ToList()
            };

            Instance.RaiseProgressChanged("Adding paths to monitor ...");
            runtimeProfiles.MonitorPaths.AddRange(Sys.Settings.ExtraFolders.Where(s => s.Length > 0).Select(s => Path.Combine(ff7Folder, s)));
            return runtimeProfiles;
        }

        private static void CopyEasyHookDlls()
        {
            string dir = Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string source = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string pathToEasyHook = Path.Combine(dir, "EasyHook.dll");
            if (!File.Exists(pathToEasyHook))
            {
                File.Copy(Path.Combine(source, "EasyHook.dll"), pathToEasyHook);
                Instance.RaiseProgressChanged("\tEasyHook.dll copied ...");
            }
            else
            {
                Instance.RaiseProgressChanged("\tskipped copying EasyHook.dll ...");

            }

            string pathToEasyHook32 = Path.Combine(dir, "EasyHook32.dll");
            if (!File.Exists(pathToEasyHook32))
            {
                File.Copy(Path.Combine(source, "EasyHook32.dll"), pathToEasyHook32);
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
                _7thWrapperLib.ModInfo info = GetModInfo(inst);

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

            _7thWrapperLib.ModInfo info;

            if (!_infoCache.TryGetValue(mfile, out info))
            {
                if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var arc = new _7thWrapperLib.IrosArc(mfile))
                        if (arc.HasFile("mod.xml"))
                        {
                            var doc = new System.Xml.XmlDocument();
                            doc.Load(arc.GetData("mod.xml"));
                            info = new _7thWrapperLib.ModInfo(doc, Sys._context);
                        }
                }
                else
                {
                    string file = Path.Combine(mfile, "mod.xml");
                    if (File.Exists(file))
                        info = new _7thWrapperLib.ModInfo(file, Sys._context);
                }
                _infoCache.Add(mfile, info);
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
