/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;
//using SharpCompress.Archive;
//using SharpCompress.Reader;
using TurBoLog.UI;
using System.Threading;

namespace Iros._7th.Workshop {
    public partial class fLibrary : Form {
        public fLibrary() {
            InitializeComponent();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            this.Text = String.Format("7thHeaven Version {0}", version);
        }

        private string _catFile;
        private Dictionary<Guid, pMod> _lMods, _cMods;
        private _7thWrapperLib.LoaderContext _context;
        private fDownloads dl;

        private void fLibrary_Load(object sender, EventArgs e) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            /*
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            using (var fs = new System.IO.FileStream(@"C:\Iros\temp\Iros.7z", System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                using (var archive = ArchiveFactory.Open(fs)) {
                    using (var reader = archive.ExtractAllEntries()) {
                        while (reader.MoveToNextEntry()) {
                            if (!reader.Entry.IsDirectory) {
                                string path = System.IO.Path.Combine(@"C:\iros\temp", reader.Entry.FilePath);
                                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                                reader.WriteEntryTo(path);
                                Console.WriteLine(reader.Entry.FilePath);
                            }
                        }
                    }
                    //archive.WriteToDirectory(@"C:\games\ff7\7thWorkshop\test", SharpCompress.Common.ExtractOptions.Overwrite | SharpCompress.Common.ExtractOptions.ExtractFullPath);
                }
            }
            sw.Stop();
            */
            Log.Write("7thHeaven started: " + Sys.Version.ToString());
            
            Mega.MegaIros.Logger = Log.Write;

            this.dl = new fDownloads();
            Sys.Downloads = this.dl;
            //this.dl.Show();
            try {
                
                string src = System.IO.Path.Combine(Sys._7HFolder, "SharpCompressU.cpy");
                string dst = System.IO.Path.Combine(Sys._7HFolder, "SharpCompressU.dll");
                if (System.IO.File.Exists(dst))
                {
                    System.IO.File.Delete(dst);
                    System.IO.File.Copy(src, dst);
                }

                src = System.IO.Path.Combine(Sys._7HFolder, "Updater.cpy");
                dst = System.IO.Path.Combine(Sys._7HFolder, "Updater.exe");
                if (System.IO.File.Exists(dst))
                {
                    System.IO.File.Delete(dst);
                    System.IO.File.Copy(src, dst);
                }
            } catch (System.IO.IOException) {

            } catch (System.UnauthorizedAccessException) {

            }

            _catFile = System.IO.Path.Combine(Sys.SysFolder, "catalog.xml");
            try {
                Sys.Catalog = Util.Deserialize<Catalog>(_catFile);
            } catch {
                Sys.Catalog = new Catalog();
            }

            _context = new _7thWrapperLib.LoaderContext() { VarAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) }; 
            string vfile = System.IO.Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".var");
            if (System.IO.File.Exists(vfile))
                foreach (string line in System.IO.File.ReadAllLines(vfile)) {
                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length == 2) _context.VarAliases[parts[0]] = parts[1];
                }

            Sys.GotoLink += new EventHandler<LinkEventArgs>(Sys_GotoLink);
            Sys.MessageReceived += DoAddMessage;

            //Sys.Downloads.DownloadString(Sys.Settings.UpdateUrl, "Checking for updated catalog...", new Install.InstallProcedureCallback(CatUpdate));

            Sys.Message(new WMessage() { Text = String.Format("7thHeaven v{0} started", Sys.Version) });

            _lMods = _cMods = new Dictionary<Guid, pMod>();
            pTagsC.Init();
            pTagsL.Init();

            Sys.StatusChanged += new EventHandler<ModStatusEventArgs>(Sys_StatusChanged);

            if (Sys.Settings.MainWindow != null) {
                Size = new Size(Sys.Settings.MainWindow.W, Sys.Settings.MainWindow.H);
                var loc = new Point(Sys.Settings.MainWindow.X, Sys.Settings.MainWindow.Y);
                if (Screen.AllScreens.Any(s => s.Bounds.Contains(loc)))
                    Location = loc;
                WindowState = FormWindowState.Normal;
            }
            cbCompact.Checked = (Sys.Settings.IntOptions & InterfaceOptions.ProfileCollapse) != 0;

            Sys.ActiveProfile = null;
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Sys.SysFolder, "profiles"));
            if (!String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile) && System.IO.File.Exists(ProfileFile)) {
                try {
                    Sys.ActiveProfile = Util.Deserialize<Profile>(ProfileFile);
                    RefreshProfile();
                } catch {
                    Sys.Settings.CurrentProfile = null;
                }
            }

            bool showSettings = false;
            if (Sys.Settings.VersionUpgradeCompleted < Sys.Version)
                showSettings = true;
            else {
                var errors = Sys.Settings.VerifySettings();
                if (errors.Any()) {
                    string msg = "The following errors were found in your configuration:\n" +
                        String.Join("\n", errors) + "\n" +
                        "The settings window will now be displayed so you can fix them.";
                    MessageBox.Show(this, msg, "Config error");
                    showSettings = true;
                }
            }

            if (showSettings)
                new fSettings().ShowDialog(this);

            if (Sys.Settings.Options.HasFlag(GeneralOptions.AutoImportMods) && System.IO.Directory.Exists(Sys.Settings.LibraryLocation)) {
                foreach (string folder in System.IO.Directory.GetDirectories(Sys.Settings.LibraryLocation)) {
                    string name = System.IO.Path.GetFileName(folder);
                    if (!name.EndsWith("temp", StringComparison.InvariantCultureIgnoreCase) && !Sys.Library.PendingDelete.Contains(name, StringComparer.InvariantCultureIgnoreCase)) {
                        if (!Sys.Library.Items.SelectMany(ii => ii.Versions).Any(v => v.InstalledLocation.Equals(name, StringComparison.InvariantCultureIgnoreCase))) {
                            Log.Write("Trying to auto-import file " + folder);
                            try {
                                fImportMod.ImportMod(folder, System.IO.Path.GetFileNameWithoutExtension(folder), false, true);
                            } catch (Exception ex) {
                                Sys.Message(new WMessage() { Text = "Mod " + name + " failed to import: " + ex.ToString() });
                                continue;
                            }
                            Sys.Message(new WMessage() { Text = "Auto imported mod " + name });
                        }
                    }
                }
                foreach (string iro in System.IO.Directory.GetFiles(Sys.Settings.LibraryLocation, "*.iro")) {
                    string name = System.IO.Path.GetFileName(iro);
                    if (!name.EndsWith("temp", StringComparison.InvariantCultureIgnoreCase) && !Sys.Library.PendingDelete.Contains(name, StringComparer.InvariantCultureIgnoreCase)) {
                        if (!Sys.Library.Items.SelectMany(ii => ii.Versions).Any(v => v.InstalledLocation.Equals(name, StringComparison.InvariantCultureIgnoreCase))) {
                            Log.Write("Trying to auto-import file " + iro);
                            try {
                                fImportMod.ImportMod(iro, System.IO.Path.GetFileNameWithoutExtension(iro), true, true);
                            } catch (_7thWrapperLib.IrosArcException) {
                                Sys.Message(new WMessage() { Text = "Could not import .iro mod " + System.IO.Path.GetFileNameWithoutExtension(iro) + ", file is corrupt" });
                                continue;
                            }
                            Sys.Message(new WMessage() { Text = "Auto imported mod " + name });
                        }
                    }
                }
            }

            foreach (var mod in Sys.Library.Items.ToArray()) {
                string fn = System.IO.Path.Combine(Sys.Settings.LibraryLocation, mod.LatestInstalled.InstalledLocation);
                if (!System.IO.File.Exists(fn) && !System.IO.Directory.Exists(fn)) {
                    Sys.Library.Items.Remove(mod);
                    var details = mod.CachedDetails ?? new Mod();
                    Sys.Message(new WMessage { Text = String.Format("Could not find mod {0} - has it been deleted? Removed.", details.Name) });
                }
            }
            
            Sys.Library.AttemptDeletions();

            if (Sys.ActiveProfile == null) {
                Sys.ActiveProfile = new Profile();
                Sys.Settings.CurrentProfile = "Default";
            }
            Sys.Save();

            System.Threading.ThreadPool.QueueUserWorkItem(BackgroundCatCheck, new CatCheckOptions());
            if (Sys.Settings.Options.HasFlag(GeneralOptions.CheckForUpdates))
                System.Threading.ThreadPool.QueueUserWorkItem(UpdateCheck, Sys.Settings.AutoUpdateSource);

            foreach (string parm in Environment.GetCommandLineArgs())
                if (parm.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                    ProcessIrosLink(parm);
                else if (parm.StartsWith("/OPENIRO:", StringComparison.InvariantCultureIgnoreCase))
                {
                    string irofile = parm.Substring(9);
                    string irofilenoext = System.IO.Path.GetFileNameWithoutExtension(irofile);
                    Log.Write("Importing IRO from Windows " + irofile);
                    try
                    {
                        fImportMod.ImportMod(irofile, irofilenoext, true, false);
                    }
                    catch (Exception ex)
                    {
                        Sys.Message(new WMessage() { Text = "Mod " + irofilenoext + " failed to import: " + ex.ToString() });
                        continue;
                    }
                    Sys.Message(new WMessage() { Text = "Auto imported mod " + irofilenoext });
                    MessageBox.Show("The mod " + irofilenoext + " has been added to your Library.","Import Mod from Windows");
                    //TODO: Add an IRO "Unpack" option here
                } else if (parm.StartsWith("/PROFILE:", StringComparison.InvariantCultureIgnoreCase)) {
                    Sys.Settings.CurrentProfile = parm.Substring(9);
                    Sys.ActiveProfile = Util.Deserialize<Profile>(ProfileFile);
                    RefreshProfile();
                } else if (parm.Equals("/LAUNCH", StringComparison.InvariantCultureIgnoreCase))
                    bLaunch.PerformClick();
                else if (parm.Equals("/QUIT", StringComparison.InvariantCultureIgnoreCase))
                    Application.Exit();
        }

        void Sys_GotoLink(object sender, LinkEventArgs e) {
            if (e.Link.StartsWith("iros://", StringComparison.InvariantCultureIgnoreCase))
                ProcessIrosLink(e.Link);
        }
        // iros://{MODID} - just jump to mod
        // iros://Subscribe/[type]/[link] - add subscription

        private void ProcessIrosLink(string link) {
            string[] parts = link.Substring(7).Split(new[] { '/' }, 2);
            if (parts.Length > 0) {
                Guid g;
                if (parts[0].Equals("Subscribe", StringComparison.InvariantCultureIgnoreCase) && parts.Length > 1) {
                    if (MessageBox.Show("Are you sure you want to subscribe to this catalog?\r\n" + parts[1], "Add Subscription", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                        Sys.Settings.SubscribedUrls.Add("iros://" + parts[1]);
                    }
                } else if (parts.Length == 1 && Guid.TryParse(parts[0], out g)) {
                    GotoMod(g);
                }
            }
        }

        private void ApplyUpdate(object state) {
            string local = System.IO.Path.Combine(Sys.SysFolder, "AutoUpdate.zip");
            try {
                new System.Net.WebClient().DownloadFile(state.ToString(), local);
                Process.Start(System.IO.Path.Combine(Sys._7HFolder, "Updater.exe"));
                Application.Exit();
            } catch(Exception e) {
                Sys.Message(new WMessage { Text = "Download of update failed - " + e.ToString() });
            }
        }

        private void NotifyUpdate(decimal ver, string url) {
            fNewVer form = new fNewVer();
            form.Init(ver);
            switch (form.ShowDialog(this)) {
                case System.Windows.Forms.DialogResult.No:
                    Sys.Settings.AutoUpdateOffered = ver;
                    Sys.Save();
                    return;
                case System.Windows.Forms.DialogResult.Yes:
                    System.Threading.ThreadPool.QueueUserWorkItem(ApplyUpdate, url);
                    return;
            }
        }

        private const string _updateUrl = @"http://ovaremake.mooo.com/7thHeaven/version.txt";

        private void UpdateCheck(object link) {
            //Sys.Downloads.Download()
            try {
                string[] data = new System.Net.WebClient().DownloadString(_updateUrl).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                decimal ver;
                if (decimal.TryParse(data[0], out ver)) {

                }

                Log.Write("Auto update check: found version " + ver + ", prev. offered " + Sys.Settings.AutoUpdateOffered);
                if (ver > Sys.Version && ver > Sys.Settings.AutoUpdateOffered) {
                    Action a = () => NotifyUpdate(ver, data[1]);
                    this.Invoke((Delegate)a);
                }
            } catch(Exception e) {
                Sys.Message(new WMessage { Text = "Check for updates failed - " + e.ToString() });
            }
        }

        private class CatCheckOptions {
            public bool ForceCheck { get; set; }
        }

        private void BackgroundCatCheck(object state) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            bool changed = false;
            List<Guid> pingIDs = null;
            var options = (CatCheckOptions)state;
            System.Threading.ManualResetEvent ev = new System.Threading.ManualResetEvent(false);
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Sys.SysFolder, "temp"));
            foreach (string subscribe in Sys.Settings.SubscribedUrls.ToArray()) {
                Subscription sub = Sys.Settings.Subscriptions.Find(s => s.Url.Equals(subscribe, StringComparison.InvariantCultureIgnoreCase));
                if (sub == null) {
                    sub = new Subscription() { Url = subscribe, FailureCount = 0, LastSuccessfulCheck = DateTime.MinValue };
                    Sys.Settings.Subscriptions.Add(sub);
                }
                if ((sub.LastSuccessfulCheck < DateTime.Now.AddDays(-1)) || options.ForceCheck) {
                    Log.Write("Checking subscription " + sub.Url);
                    string path = System.IO.Path.Combine(Sys.SysFolder, "temp", "cattemp.xml");
                    bool success = false;
                    ev.Reset();
                    this.Invoke((Delegate)(Action)(() =>
                        Sys.Downloads.Download(subscribe, path, "Checking catalog " + subscribe, new Install.InstallProcedureCallback(e => {
                            success = (e.Error == null && e.Cancelled == false);
                            ev.Set();
                    }), null)));
                    ev.WaitOne();
                    if (success) {
                        try {
                            Catalog c = Util.Deserialize<Catalog>(path);
                            Sys.Catalog = Catalog.Merge(Sys.Catalog, c, out pingIDs);
                            Sys.Message(new WMessage() { Text = "Updated catalog from " + subscribe });
                            using (var fs = new System.IO.FileStream(_catFile, System.IO.FileMode.Create))
                                Util.Serialize(Sys.Catalog, fs);
                            sub.LastSuccessfulCheck = DateTime.Now;
                            sub.FailureCount = 0;
                            changed = true;
                        } catch (Exception ex) {
                            sub.FailureCount++;
                            Sys.Message(new WMessage() { Text = "Failed to load subscription " + subscribe + ": " + ex.ToString() });
                        }
                    } else
                        sub.FailureCount = sub.FailureCount + 1;
                }
            }
            if (changed)
                this.Invoke((Delegate)(Action)(() => {
                    pTagsC.Init();
                    pTagsL.Init();
                    foreach (var id in pingIDs) Sys.Ping(id);
                }));
            ScanUpdates();
        }

        void GotoMod(Guid id) {
            var inst = Sys.Library.GetItem(id);
            pMod pm;
            if (inst != null) {
                if (_lMods.TryGetValue(id, out pm))
                    pSearchResultsL.ScrollControlIntoView(pm);
                else
                    DoSearch(pSearchResultsL, new[] { inst.CachedDetails }, String.Empty, new HashSet<string>(), lSearchL, ref _lMods, mMod, pMod.ModBarState.Activate, true, false);
                TC.SelectedIndex = 1;
            } else {
                if (_cMods.TryGetValue(id, out pm))
                    pSearchResultsC.ScrollControlIntoView(pm);
                else
                    DoSearch(pSearchResultsC, new[] { Sys.Catalog.GetMod(id) }, String.Empty, new HashSet<string>(), lSearchC, ref _cMods, null, pMod.ModBarState.None, false, true);
                TC.SelectedIndex = 2;
            }
        }

        void Sys_StatusChanged(object sender, ModStatusEventArgs e) {
            pMod pm;
            if (_lMods.TryGetValue(e.ModID, out pm)) pm.UpdateDetails();
            if (_cMods.TryGetValue(e.ModID, out pm)) pm.UpdateDetails();
            pm = pProfile.Controls.OfType<pMod>().FirstOrDefault(p => p.ModID.Equals(e.ModID));
            if (pm != null) pm.UpdateDetails();

            if (e.Status == ModStatus.Installed) {
                var mod = Sys.Library.GetItem(e.ModID);
                string mfile = mod.LatestInstalled.InstalledLocation;
                _infoCache.Remove(mfile);
            }

            if (e.Status == ModStatus.Installed && e.OldStatus != ModStatus.Installed && Sys.Settings.Options.HasFlag(GeneralOptions.AutoActiveNewMods))
                ToggleActivateMod(e.ModID);
            if (e.OldStatus == ModStatus.Installed && e.Status == ModStatus.NotInstalled && Sys.ActiveProfile.Items.Any(i => i.ModID.Equals(e.ModID)))
                ToggleActivateMod(e.ModID);
        }

        /*
        private void CatUpdate(AsyncCompletedEventArgs e) {
            if (e.Error == null) {
                string[] data = ((System.Net.DownloadStringCompletedEventArgs)e).Result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                DateTime ver;
                if (DateTime.TryParse(data.ElementAtOrDefault(0), out ver) && ver > Sys.Catalog.UpdatedOn) {
                    Sys.Downloads.DownloadString(data.ElementAtOrDefault(1), "Downloading new catalog", new Install.InstallProcedureCallback(NewCat));
                }
                decimal d;
                if (data.Length > 4 && decimal.TryParse(data[2], out d) && d > Sys.Version) {
                    Sys.Message(new WMessage() { Link = data[3], Text = data[4] });
                }
            }
        }
         */

        private void ScanUpdates() {
            foreach (var inst in Sys.Library.Items) {
                var cat = Sys.Catalog.GetMod(inst.ModID);
                if (cat != null && cat.LatestVersion.Version > inst.Versions.Max(v => v.VersionDetails.Version)) {
                    switch (inst.UpdateType) {
                        case UpdateType.Notify:
                            Sys.Message(new WMessage() { Text = "New version of " + cat.Name + " available", Link = "iros://" + cat.ID.ToString() });
                            Action a = () => Sys.Ping(inst.ModID);
                            this.Invoke((Delegate)a);
                            break;
                        case UpdateType.Install:
                            Install.DownloadAndInstall(cat);
                            break;
                    }
                }
            }
        }

        private void NewCat(AsyncCompletedEventArgs e) {
            if (e.Error == null) {
                string data = ((System.Net.DownloadStringCompletedEventArgs)e).Result;
                Catalog c;
                try {
                    c = Util.DeserializeString<Catalog>(data);
                } catch {
                    return;
                }
                Sys.Catalog = c;
                using (var fs = new System.IO.FileStream(_catFile, System.IO.FileMode.Create))
                    Util.Serialize(c, fs);
                ScanUpdates();
                pTagsC.Init();
                pTagsL.Init();
                Sys.Message(new WMessage() { Text = "Updated catalog has been downloaded!" });
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private enum ScrollBarDirection {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }


        private void Remove_PM(object sender, EventArgs e) {
            var pm = sender as pMessage;
            pMessages.Controls.Remove(pm);
            foreach (pMessage other in pMessages.Controls.OfType<pMessage>().Where(p => p.Top > pm.Top)) {
                Transitions.Transition.run(other, "Top", other.Top - pm.Height, new Transitions.TransitionType_Linear(200));
            }
        }

        private void DoAddMessage(object sender, MessageEventArgs e) {
            this.Invoke((Delegate)(Action)(() => {
                pMessage pm = new pMessage();
                int top = pMessages.Controls.Count == 0 ? 0 : pMessages.Controls.OfType<Control>().Select(c => c.Top + c.Height).Max();
                pm.Init(e.Message, pMessages.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - 2);
                pMessages.Controls.Add(pm);
                pm.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                pm.Location = new Point(-pMessages.Width, top);
                pm.Close += Remove_PM;
                ShowScrollBar(pMessages.Handle, (int)ScrollBarDirection.SB_HORZ, false);
                //ShowScrollBar(pMessages.Handle, (int)ScrollBarDirection.SB_VERT, true);
                Transitions.Transition.run(pm, "Left", 0, new Transitions.TransitionType_Linear(200));
            }));
        }

        private void AddModToPanel(pMod pm, Panel p, int top, ContextMenuStrip menu, bool collapsed) {
            pm.DoAction = DoModBarAction;
            p.Controls.Add(pm);
            pm.SetBounds(0, top, p.Width - 2, collapsed ? pMod.COLLAPSED_HEIGHT : 200);
            pm.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pm.ContextMenuStrip = menu;
        }

        private void DoSearch(Panel pResults, IEnumerable<Mod> mods, string text, HashSet<string> tags, Label lbl, ref Dictionary<Guid, pMod> list, ContextMenuStrip menu, pMod.ModBarState showProfile, bool showInstall, bool showDownloadSize) {
            IEnumerable<Mod> results = mods.Select(m => new { Mod = m, Relevance = m.SearchRelevance(text) })
                .Where(a => a.Relevance > 0)
                .OrderByDescending(a => a.Relevance)
                .Select(a => a.Mod);
            if (tags.Any()) results = results.Where(m => m.Tags.Intersect(tags).Any());

            StringBuilder sb = new StringBuilder();
            sb.Append("Search ");
            if (!String.IsNullOrEmpty(text)) sb.Append("for " + text + " ");
            if (tags.Any()) sb.Append("in " + String.Join(", ", tags));           

            if (!results.Any()) sb.Append(" [no results]");
            
            lbl.Text = sb.ToString();

            pResults.SuspendLayout();
            pResults.Controls.Clear();
            pResults.VerticalScroll.Value = 0;
            Panel pcontainer = new Panel();
            pResults.Controls.Add(pcontainer);
            pcontainer.Location = new Point();
            pcontainer.Width = pResults.Width;
            pcontainer.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            int top = 0;
            foreach (var mod in results) {
                pMod pm = new pMod();
                pm.Init(mod, showProfile, showInstall, showDownloadSize);
                AddModToPanel(pm, pcontainer, top, menu, false);
                top += pm.Height;
            }
            pcontainer.Height = top;
            pResults.ResumeLayout();
            //ShowScrollBar(pResults.Handle, (int)ScrollBarDirection.SB_HORZ, false);
            //ShowScrollBar(pResults.Handle, (int)ScrollBarDirection.SB_VERT, true);
            list = pcontainer.Controls.OfType<pMod>().ToDictionary(pm => pm.ModID, pm => pm);
            pResults.Focus();
        }
        
        private bool VerifyOrdering() {
            var details = Sys.ActiveProfile
                .Items
                .Select(i => Sys.Library.GetItem(i.ModID))
                .Select(ii => new { Mod = ii, Info = GetModInfo(ii) })
                .ToDictionary(a => a.Mod.ModID, a => a);

            List<string> problems = new List<string>();

            foreach (int i in Enumerable.Range(0, Sys.ActiveProfile.Items.Count)) {
                var mod = Sys.ActiveProfile.Items[i];
                var info = details[mod.ModID].Info;
                if (info != null) {
                    foreach(Guid after in info.OrderAfter)
                        if (Sys.ActiveProfile.Items.Skip(i).Any(pi => pi.ModID.Equals(after))) {
                            problems.Add(String.Format("Mod {0} is meant to come BELOW mod {1} in the load order", details[mod.ModID].Mod.CachedDetails.Name, details[after].Mod.CachedDetails.Name));
                        }
                    foreach (Guid before in info.OrderBefore)
                        if (Sys.ActiveProfile.Items.Take(i).Any(pi => pi.ModID.Equals(before))) {
                            problems.Add(String.Format("Mod {0} is meant to come ABOVE mod {1} in the load order", details[mod.ModID].Mod.CachedDetails.Name, details[before].Mod.CachedDetails.Name));
                        }
                }
            }

            if (problems.Any()) {
                if (MessageBox.Show(String.Format("The following mods will not work properly in the current order:\n{0}\nDo you want to continue anyway?", String.Join("\n", problems)), "Load Order Incompatible", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
                    return false;
            }
            return true;
        }

        private void ChangeProfileItem(pMod pm, int change) {
            int index = Sys.ActiveProfile.Items.IndexOf(pm.Tag as ProfileItem);
            int newindex = index + change;
            if (newindex < 0 || newindex >= Sys.ActiveProfile.Items.Count) return;
            pMod other = pProfile.Controls.OfType<pMod>().First(p => p.Tag == Sys.ActiveProfile.Items[newindex]);

            var newItems = Sys.ActiveProfile.Items.ToList();
            var pitem = newItems[index];
            newItems[index] = newItems[newindex];
            newItems[newindex] = pitem;

            Sys.ActiveProfile.Items = newItems;
            Transitions.Transition.run(pm, "Top", pm.Top + (pm.Height * change), new Transitions.TransitionType_Linear(1));
            Transitions.Transition.run(other, "Top", other.Top + (pm.Height * -change), new Transitions.TransitionType_Linear(1));

        }

        private bool CheckAllowedActivate(Guid modID) {
            if (Sys.Library.CodeAllowed.Contains(modID)) return true;
            var mod = Sys.Library.GetItem(modID);
            var inst = mod.LatestInstalled;
            string mfile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);
            bool hasCode;
            var minfo = GetModInfo(mod);
            if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase)) {
                using (var arc = new _7thWrapperLib.IrosArc(mfile)) {
                    hasCode = arc.AllFolderNames().Any(s => s.EndsWith("hext", StringComparison.InvariantCultureIgnoreCase));
                }
            } else if (System.IO.Directory.Exists(mfile)) {
                hasCode = System.IO.Directory.GetDirectories(mfile, "*", System.IO.SearchOption.AllDirectories)
                    .Any(s => s.EndsWith("hext", StringComparison.InvariantCultureIgnoreCase));
            } else
                hasCode = false;

            if (minfo != null) {
                hasCode |= minfo.LoadPlugins.Any();
                hasCode |= minfo.LoadLibraries.Any();
                hasCode |= minfo.LoadAssemblies.Any();
            }

            if (!hasCode) return true;

            string msg = "This mod ({0}) contains code/patches that could change FF7.exe. Are you sure you want to activate and run this mod?\n" +
                "Only choose YES if you trust the author of this mod to run code/programs on your computer!";
            msg = String.Format(msg, mod.CachedDetails.Name);

            if (MessageBox.Show(this, msg, "Allow mod to run?", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) {
                return false;
            } else {
                Sys.Library.CodeAllowed.Add(modID);
                Sys.Save();
                return true;
            }
        }

        private const string _msgReqMissing =
            @"This mod requires the following mods to also be active, but I could not find them:
{0}
It may not work correctly unless you install them.";
        private const string _msgBadVer =
            @"This mod requires the following mods, but you do not have a supported version:
{0}
You may need to update these mods.";
        private const string _msgRequired =
            @"This mod requires that you activate the following mods:
{0}
They will be automatically turned on.";
        private const string _msgRemove =
            @"This mod requires that you deactivate the following mods:
{0}
They will be automatically turned off.";

        private const string _forbidMain =
            @"You cannot activate this mod, because it is incompatible with {0}. You will have to deactivate {0} before you can enable this mod.";
        private const string _forbidDependent =
            @"You cannot activate this mod, because it requires {0} to be active, but {0} is incompatible with {1}. You will have to deactivate {1} before you can enable this mod.";

        private void DoActivate(Guid modID) {
            if (!CheckAllowedActivate(modID)) return;

            var item = new ProfileItem() { ModID = modID, Settings = new List<ProfileSetting>() };
            Sys.ActiveProfile.Items.Add(item);
            var ppm = new pMod();
            ppm.Init(Sys.Library.GetItem(modID).CachedDetails, pMod.ModBarState.Full, true, false);
            AddModToPanel(ppm, pProfile, pProfile.Controls.OfType<pMod>().Select(p => p.Top + p.Height).OrderByDescending(i => i).FirstOrDefault(), null, cbCompact.Checked);
            ppm.Tag = item;
            pProfile.Controls.Add(ppm);
        }

        private void DoDeactivate(ProfileItem item) {
            var ppm = pProfile.Controls.OfType<pMod>().First(p => p.Tag == item);
            ppm.SendToBack();
            foreach (var other in pProfile.Controls.OfType<pMod>().Where(p => p.Top > ppm.Top))
                Transitions.Transition.run(other, "Top", other.Top - ppm.Height, new Transitions.TransitionType_Linear(200));
            Sys.ActiveProfile.Items.Remove(item);
            var tr = new Transitions.Transition(new Transitions.TransitionType_Linear(200));
            tr.add(ppm, "Left", -pProfile.Width);
            tr.TransitionCompletedEvent += (o, e) => {
                this.Invoke((Delegate)(Action)(() => pProfile.Controls.Remove(ppm)));
            };
            tr.run();
        }

        internal class Constraint {
            public Guid ModID { get; set; }
            public string Setting { get; set; }
            public List<int> Require { get; set; }
            public HashSet<int> Forbid { get; set; }
            public HashSet<string> ParticipatingMods { get; set; }
            public _7thWrapperLib.ConfigOption Option { get; set; }

            public Constraint() {
                Forbid = new HashSet<int>();
                Require = new List<int>();
                ParticipatingMods = new HashSet<string>();
            }

            public bool Verify(out string message) {
                message = null;
                if (Option == null) return true; //setting no longer exists, constraints are irrelevant?
                var pItem = Sys.ActiveProfile.Items.Find(pi => pi.ModID.Equals(ModID));
                var inst = Sys.Library.GetItem(pItem.ModID);
                var setting = pItem.Settings.Find(s => s.ID.Equals(Setting, StringComparison.InvariantCultureIgnoreCase));
                if (setting == null) {
                    setting = new ProfileSetting() { ID = Setting, Value = Option.Default };
                    pItem.Settings.Add(setting);
                }
                if (Require.Any() && (Require.Min() != Require.Max())) {
                    message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                    return false;
                }

                if (Require.Any() && Forbid.Contains(Require[0])) {
                    message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                    return false;
                }
                if (Option.Values.All(o => Forbid.Contains(o.Value))) {
                    message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                    return false;
                }
                if (Require.Any() && (setting.Value != Require[0])) {
                    var opt = Option.Values.Find(v => v.Value == Require[0]);
                    if (opt == null) {
                        message = String.Format("Mod {0}, setting {1} - no compatible option can be found. The following mods all restrict how it can be configured: {2}", inst.CachedDetails.Name, Option.Name, String.Join(",", ParticipatingMods));
                        return false;
                    }
                    setting.Value = Require[0];
                    message = String.Format("Mod {0} - changed setting {1} to {2}", inst.CachedDetails.Name, Option.Name, opt.Name);
                } else if (Forbid.Contains(setting.Value)) {
                    setting.Value = Option.Values.First(v => !Forbid.Contains(v.Value)).Value;
                    var opt = Option.Values.Find(v => v.Value == setting.Value);
                    message = String.Format("Mod {0} - changed setting {1} to {2}", inst.CachedDetails.Name, Option.Name, opt.Name);
                }
                return true;
            }
        }

        private List<Constraint> GetConstraints() {
            List<Constraint> constraints = new List<Constraint>();
            foreach (var pItem in Sys.ActiveProfile.Items) {
                var inst = Sys.Library.GetItem(pItem.ModID);
                var info = GetModInfo(inst);
                if (info != null) {
                    foreach (var cSetting in info.Compatibility.Settings) {
                        if (!String.IsNullOrWhiteSpace(cSetting.MyID)) {
                            var setting = pItem.Settings.Find(s => s.ID.Equals(cSetting.MyID, StringComparison.InvariantCultureIgnoreCase));
                            if ((setting == null) || (setting.Value != cSetting.MyValue)) continue;
                        }
                        var oItem = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(cSetting.ModID));
                        if (oItem == null) continue;
                        var oInst = Sys.Library.GetItem(cSetting.ModID);
                        Constraint ct = constraints.Find(c => c.ModID.Equals(cSetting.ModID) && c.Setting.Equals(cSetting.TheirID, StringComparison.InvariantCultureIgnoreCase));
                        if (ct == null) {
                            ct = new Constraint() { ModID = cSetting.ModID, Setting = cSetting.TheirID };
                            constraints.Add(ct);
                        }
                        ct.ParticipatingMods.Add(inst.CachedDetails.Name);
                        if (cSetting.Require.HasValue) {
                            ct.Require.Add(cSetting.Require.Value);
                        }
                        foreach (var f in cSetting.Forbid)
                            ct.Forbid.Add(f);
                    }
                    foreach (var setting in info.Options) {
                        Constraint ct = constraints.Find(c => c.ModID.Equals(pItem.ModID) && c.Setting.Equals(setting.ID, StringComparison.InvariantCultureIgnoreCase));
                        if (ct == null) {
                            ct = new Constraint() { ModID = pItem.ModID, Setting = setting.ID };
                            constraints.Add(ct);
                        }
                        ct.Option = setting;
                    }
                }
            }
            return constraints;
        }

        private bool SanityCheckSettings() {
            List<string> changes = new List<string>();
            foreach (var constraint in GetConstraints()) {
                string msg;
                if (!constraint.Verify(out msg)) {
                    MessageBox.Show(msg);
                    return false;
                } else
                    if (msg != null) changes.Add(msg);
            }

            if (changes.Any()) {
                MessageBox.Show(String.Format("The following settings have been changed to make these mods compatible:\n{0}", String.Join("\n", changes)));
            }

            return true;
        }

        private bool SanityCheckCompat() {
            var profInst = Sys.ActiveProfile.Items.Select(pi => Sys.Library.GetItem(pi.ModID)).ToList();
            foreach (var item in profInst) {
                var info = GetModInfo(item);
                if (info != null) {
                    foreach (var req in info.Compatibility.Requires) {
                        var rInst = profInst.Find(i => i.ModID.Equals(req.ModID));
                        if (rInst == null) {
                            MessageBox.Show(String.Format("Mod {0} requires you to activate {1} as well.", item.CachedDetails.Name, req.Description));
                            return false;
                        } else if (req.Versions.Any() && !req.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version)) {
                            MessageBox.Show(String.Format("Mod {0} requires you to activate {1}, but you do not have a compatible version installed. Try updating it?", item.CachedDetails.Name, rInst.CachedDetails.Name));
                            return false;
                        } 
                    }
                    foreach (var forbid in info.Compatibility.Forbids) {
                        var rInst = profInst.Find(i => i.ModID.Equals(forbid.ModID));
                        if (rInst == null) {
                            //good!
                        } else if (forbid.Versions.Any() && forbid.Versions.Contains(rInst.LatestInstalled.VersionDetails.Version)) {
                            MessageBox.Show(String.Format("Mod {0} is not compatible with the version of {1} you have installed. Try updating it?", item.CachedDetails.Name, rInst.CachedDetails.Name));
                            return false;
                        } else {
                            MessageBox.Show(String.Format("Mod {0} is not compatible with {1}. You will need to disable it.", item.CachedDetails.Name, rInst.CachedDetails.Name));
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void ToggleActivateMod(Guid modID) {
            if (String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile)) return;
            var item = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(modID));
            if (item == null) {

                HashSet<Guid> examined = new HashSet<Guid>();
                List<InstalledItem> pulledIn = new List<InstalledItem>();
                List<ProfileItem> remove = new List<ProfileItem>();
                List<string> missing = new List<string>();
                List<InstalledItem> badVersion = new List<InstalledItem>();
                Stack<InstalledItem> toExamine = new Stack<InstalledItem>();
                toExamine.Push(Sys.Library.GetItem(modID));
                while (toExamine.Any()) {
                    examined.Add(toExamine.Peek().ModID);
                    var info = GetModInfo(toExamine.Pop());
                    if (info != null) {
                        foreach(var req in info.Compatibility.Requires)
                            if (!examined.Contains(req.ModID)) {
                                var inst = Sys.Library.GetItem(req.ModID);
                                if (inst == null)
                                    missing.Add(req.Description);
                                else if (req.Versions.Any() && !req.Versions.Contains(inst.Versions.Last().VersionDetails.Version))
                                    badVersion.Add(inst);
                                else {
                                    if (Sys.ActiveProfile.Items.Find(pi => pi.ModID.Equals(req.ModID)) == null)
                                        pulledIn.Add(inst);
                                    toExamine.Push(inst);
                                }
                            }
                        foreach(var forbid in info.Compatibility.Forbids)
                            if (!examined.Contains(forbid.ModID)) {
                                examined.Add(forbid.ModID);
                                var pItem = Sys.ActiveProfile.Items.Find(i => i.ModID.Equals(forbid.ModID));
                                if (pItem != null) {
                                    if (!forbid.Versions.Any() || forbid.Versions.Contains(Sys.Library.GetItem(pItem.ModID).LatestInstalled.VersionDetails.Version)) {
                                        remove.Add(pItem);
                                    }
                                }
                            }
                    }
                }

                foreach (var active in Sys.ActiveProfile.Items.Except(remove)) {
                    var info = GetModInfo(Sys.Library.GetItem(active.ModID));
                    if (info != null) {
                        foreach (var mID in pulledIn.Select(ii => ii.ModID).Concat(new[] { modID })) {
                            var forbid = info.Compatibility.Forbids.Find(f => f.ModID.Equals(mID));
                            if (forbid != null)
                                if (!forbid.Versions.Any() || forbid.Versions.Contains(Sys.Library.GetItem(mID).LatestInstalled.VersionDetails.Version)) {
                                    if (mID.Equals(modID))
                                        MessageBox.Show(String.Format(_forbidMain, Sys.Library.GetItem(active.ModID).CachedDetails.Name));
                                    else
                                        MessageBox.Show(String.Format(_forbidDependent, Sys.Library.GetItem(mID).CachedDetails.Name, Sys.Library.GetItem(active.ModID).CachedDetails.Name));
                                    return;
                                }
                        }
                    }
                }

                if (missing.Any()) {
                    MessageBox.Show(String.Format(_msgReqMissing, String.Join("\n", missing)));
                }
                if (badVersion.Any()) {
                    MessageBox.Show(String.Format(_msgBadVer, String.Join("\n", badVersion.Select(ii => ii.CachedDetails.Name))));
                }
                if (pulledIn.Any()) {
                    MessageBox.Show(String.Format(_msgRequired, String.Join("\n", pulledIn.Select(ii => ii.CachedDetails.Name))));
                }
                if (remove.Any()) {
                    MessageBox.Show(String.Format(_msgRemove, String.Join("\n", remove.Select(pi => Sys.Library.GetItem(pi.ModID).CachedDetails.Name))));
                }

                DoActivate(modID);
                foreach (var req in pulledIn) {
                    DoActivate(req.ModID);
                    Sys.Ping(req.ModID);
                }
                foreach (var pi in remove) {
                    DoDeactivate(pi);
                    Sys.Ping(pi.ModID);
                }

                SanityCheckSettings();

            } else {
                DoDeactivate(item);
            }
            pProfile.Height = pProfile.Controls.Count == 0 ? 0 : pProfile.Controls.OfType<Control>().Select(c => c.Top + c.Height).Max();
            Sys.Ping(modID);
        }

        private Dictionary<string, _7thWrapperLib.ModInfo> _infoCache = new Dictionary<string, _7thWrapperLib.ModInfo>(StringComparer.InvariantCultureIgnoreCase);

        private _7thWrapperLib.ModInfo GetModInfo(InstalledItem ii) {
            var inst = ii.Versions.OrderBy(v => v.VersionDetails.Version).Last();
            string mfile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);
            _7thWrapperLib.ModInfo info;
            if (!_infoCache.TryGetValue(mfile, out info)) {
                if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase)) {
                    using (var arc = new _7thWrapperLib.IrosArc(mfile))
                        if (arc.HasFile("mod.xml")) {
                            var doc = new System.Xml.XmlDocument();
                            doc.Load(arc.GetData("mod.xml"));
                            info = new _7thWrapperLib.ModInfo(doc, _context);
                        }
                } else {
                    string file = System.IO.Path.Combine(mfile, "mod.xml");
                    if (System.IO.File.Exists(file))
                        info = new _7thWrapperLib.ModInfo(file, _context);
                }
                _infoCache.Add(mfile, info);
            }
            return info;
        }

        private void DoModBarAction(pMod pm, ModBarAction a) {
            switch (a) {
                case ModBarAction.Up:
                    ChangeProfileItem(pm, -1);
                    break;
                case ModBarAction.Down:
                    ChangeProfileItem(pm, +1);
                    break;
                case ModBarAction.Activate:
                    if (String.IsNullOrWhiteSpace(Sys.Settings.CurrentProfile)) {
                        MessageBox.Show("Please create a profile first.");
                        return;
                    }
                    ToggleActivateMod(pm.ModID);
                    break;
                case ModBarAction.Readme:
                    var inst = Sys.Library.GetItem(pm.ModID).LatestInstalled;
                    if (inst != null) {
                        using (var s = inst.GetData("readme.md"))
                        {
                            if (s != null)
                            {
                                using (var sr = new System.IO.StreamReader(s, true))
                                {
                                    var result = CommonMark.CommonMarkConverter.Convert(sr.ReadToEnd());
                                    fReadme.Display(result);
                                }
                            }
                        }
                    }
                    break;
                case ModBarAction.Configure:
                    inst = Sys.Library.GetItem(pm.ModID).Versions.OrderBy(v => v.VersionDetails.Version).Last();
                    ProfileItem pi = pm.Tag as ProfileItem;
                    var form = new fModConfig();
                    _7thWrapperLib.ModInfo info = null;
                    Func<string, Bitmap> imageReader;
                    Func<string, System.IO.Stream> audioReader;

                    string mfile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, inst.InstalledLocation);
                    IDisposable dispose = null;
                    if (mfile.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase)) {
                        var arc = new _7thWrapperLib.IrosArc(mfile);
                        if (arc.HasFile("mod.xml")) {
                            var doc = new System.Xml.XmlDocument();
                            doc.Load(arc.GetData("mod.xml"));
                            info = new _7thWrapperLib.ModInfo(doc, _context);
                        }
                        imageReader = s => {
                            if (arc.HasFile(s))
                                return new Bitmap(arc.GetData(s));
                            return null;
                        };
                        audioReader = s => {
                            if (arc.HasFile(s))
                                arc.GetData(s);
                            return null;
                        };
                        dispose = arc;
                    } else {
                        string file = System.IO.Path.Combine(mfile, "mod.xml");
                        if (System.IO.File.Exists(file))
                            info = new _7thWrapperLib.ModInfo(file, _context);
                        imageReader = s => {
                            string ifile = System.IO.Path.Combine(mfile, s);
                            if (System.IO.File.Exists(ifile))
                                return new Bitmap(ifile);
                            return null;
                        };
                        audioReader = s => {
                            string ifile = System.IO.Path.Combine(mfile, s);
                            if (System.IO.File.Exists(ifile))
                                return new System.IO.FileStream(ifile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                            return null;
                        };
                    }

                    using (dispose) {
                        info = info ?? new _7thWrapperLib.ModInfo();
                        if (info.Options.Count == 0) {
                            MessageBox.Show("No options for this mod");
                        } else {
                            form.Init(info, imageReader, audioReader, pi, GetConstraints().Where(c => c.ModID.Equals(pm.ModID)), mfile);
                            if (form.ShowDialog() == DialogResult.OK) {
                                pi.Settings = form.GetSettings();
                                SanityCheckSettings();
                            }
                        }
                    }
                    break;
                    
            }
        }

        private void fLibrary_FormClosed(object sender, FormClosedEventArgs e) {
            Sys.Settings.MainWindow = new SavedWindow() {
                X = Location.X, Y = Location.Y,
                W = Size.Width, H = Size.Height,
                //State = WindowState
            };

            SaveProfile();
            Sys.Save();
        }

        private void bSearchC_Click(object sender, EventArgs e) {
            DoSearch(pSearchResultsC, Sys.Catalog.Mods, txtSearchC.Text, new HashSet<string>(pTagsC.Selected), lSearchC, ref _cMods, null, pMod.ModBarState.None, false, true);
        }

        private void bRefreshC_Click(object sender, EventArgs e)
        {
            DoSearch(pSearchResultsC, Sys.Catalog.Mods, "", new HashSet<string>(pTagsC.Selected), lSearchC, ref _cMods, null, pMod.ModBarState.None, false, true);
        }

        private void bSearchL_Click(object sender, EventArgs e) {
            DoSearch(pSearchResultsL, Sys.Library.Items.Select(i => i.CachedDetails), txtSearchL.Text, new HashSet<string>(pTagsL.Selected), lSearchL, ref _lMods, mMod, pMod.ModBarState.Activate, true, false);
        }

        private void bRefreshL_Click(object sender, EventArgs e)
        {
            DoSearch(pSearchResultsL, Sys.Library.Items.Select(i => i.CachedDetails), "", new HashSet<string>(pTagsL.Selected), lSearchL, ref _lMods, mMod, pMod.ModBarState.Activate, true, false);
        }

        private void uninstallToolStripMenuItem_Click(object sender, EventArgs e) {
            pMod mod = mMod.SourceControl as pMod;
            var inst = Sys.Library.GetItem(mod.ModID);
            if (inst != null) {
                mod.SendToBack();
                Install.Uninstall(inst);
                Transitions.Transition.run(mod, "Left", -mod.Width, new Transitions.TransitionType_Linear(200));
                _lMods.Remove(mod.ModID);
                foreach (pMod other in mod.Parent.Controls.OfType<pMod>().Where(m => m.Top > mod.Top)) {
                    Debug.WriteLine("Moving mod from {0} to {1}", other.Top, other.Top - mod.Height);
                    Transitions.Transition.run(other, "Top", other.Top - mod.Height, new Transitions.TransitionType_Linear(200));
                }
            }
        }

        private void pTagsL_SelectionChanged(object sender, EventArgs e) {
            bSearchL.PerformClick();
        }

        private void pTagsC_SelectionChanged(object sender, EventArgs e) {
            bSearchC.PerformClick();
        }

        private void bTagsC_Click(object sender, EventArgs e) {
            pTagsC.Visible = !pTagsC.Visible;
        }

        private void bTagsL_Click(object sender, EventArgs e) {
            pTagsL.Visible = !pTagsL.Visible;
        }

        private pMod _mMod_Opened; //workaround for Winforms bug... :/

        private void mMod_Opening(object sender, CancelEventArgs e) {
            _mMod_Opened = mMod.SourceControl as pMod;
            var inst = Sys.Library.GetItem(_mMod_Opened.ModID);
            if (inst != null) {
                mUpdateType.Enabled = true;
                mUpdateNotify.Checked = (inst.UpdateType == UpdateType.Notify);
                mUpdateAuto.Checked = (inst.UpdateType == UpdateType.Install);
                mUpdateIgnore.Checked = (inst.UpdateType == UpdateType.Ignore);
            } else
                mUpdateType.Enabled = false;
        }

        private void mUpdateNotify_Click(object sender, EventArgs e) {
            var inst = Sys.Library.GetItem(_mMod_Opened.ModID);
            if (inst != null) {
                inst.UpdateType = (UpdateType)int.Parse((sender as ToolStripMenuItem).Tag.ToString());
            }
        }

        private void RefreshProfile() {
            pProfile.Controls.Clear();
            if (Sys.ActiveProfile == null) return;
            lProfile.Text = "Profile: " + Sys.Settings.CurrentProfile;
            Sys.ActiveProfile.Items.RemoveAll(i => Sys.Library.GetItem(i.ModID) == null);
            int top = 0;
            foreach (var item in Sys.ActiveProfile.Items) {
                var mod = Sys.Library.GetItem(item.ModID);
                if (mod != null) {
                    pMod pm = new pMod();
                    pm.Init(mod.CachedDetails, pMod.ModBarState.Full, true, false);
                    AddModToPanel(pm, pProfile, top, null, cbCompact.Checked);
                    pm.Tag = item;
                    pm.DoAction = DoModBarAction;
                    top += pm.Height;
                }
            }
            pProfile.Height = pProfile.Controls.Count == 0 ? 0 : pProfile.Controls.OfType<Control>().Select(c => c.Top + c.Height).Max();
        }

        private void bNewProfile_Click(object sender, EventArgs e) {
            SaveProfile();
            string name = Microsoft.VisualBasic.Interaction.InputBox("Profile name:", "Enter new Profile name", "New Profile");
            Sys.ActiveProfile = new Profile();
            Sys.Settings.CurrentProfile = name;
            RefreshProfile();
            SaveProfile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void showDownloadsWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            this.dl.Show();
            this.dl.BringToFront();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            new fSettings().ShowDialog(this);
        }

        private string ProfileFile { get { return System.IO.Path.Combine(Sys.SysFolder, "profiles", Sys.Settings.CurrentProfile) + ".xml"; } }

        private void bDeleteProfile_Click(object sender, EventArgs e) {
            if (System.IO.File.Exists(ProfileFile)) {
                System.IO.File.Delete(ProfileFile);
                bNewProfile.PerformClick();
            }
        }

        private void SaveProfile() {
            if (Sys.ActiveProfile != null)
                using (var fs = new System.IO.FileStream(ProfileFile, System.IO.FileMode.Create))
                    Util.Serialize(Sys.ActiveProfile, fs);
        }


        private void bOpenProfile_Click(object sender, EventArgs e) {
            fProfiles form = new fProfiles();
            if (form.ShowDialog() == DialogResult.OK)
                if (!String.IsNullOrWhiteSpace(form.SelectedProfile)) {
                    SaveProfile();
                    Sys.Settings.CurrentProfile = form.SelectedProfile;
                    Sys.ActiveProfile = Util.Deserialize<Profile>(ProfileFile);
                    RefreshProfile();
                }
        }

        private Dictionary<string, Process> _also = new Dictionary<string, Process>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, _7HPlugin> _plugins = new Dictionary<string, _7HPlugin>(StringComparer.InvariantCultureIgnoreCase);

        private void bLaunch_Click(object sender, EventArgs e) {
            Launch(false, false);
        }

        private void bImport_Click(object sender, EventArgs e) {
            new fImportMod().ShowDialog(this);
            bSearchL.PerformClick();
        }

        private void modGeneratorAssistantToolStripMenuItem_Click(object sender, EventArgs e) {
            new fMakeMod().ShowDialog(this);
        }

        private void bClearAllMsg_Click(object sender, EventArgs e) {
            foreach (var pm in pMessages.Controls.OfType<pMessage>().ToArray())
                pMessages.Controls.Remove(pm);
        }

        private void bActivateAll_Click(object sender, EventArgs e) {
            HashSet<Guid> activate = new HashSet<Guid>(Sys.ActiveProfile.Items.Select(i => i.ModID));
            foreach (var ii in Sys.Library.Items)
                if (!activate.Contains(ii.ModID))
                    ToggleActivateMod(ii.ModID);
        }

        private void checkSubscriptionsNowToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Threading.ThreadPool.QueueUserWorkItem(BackgroundCatCheck, new CatCheckOptions() { ForceCheck = true });
        }

        private void packUnpackiroArchivesToolStripMenuItem_Click(object sender, EventArgs e) {
            new fPack().ShowDialog(this);
        }

        private void cbCompact_CheckedChanged(object sender, EventArgs e) {
            RefreshProfile();
            Sys.Settings.IntOptions = Sys.Settings.IntOptions & ~InterfaceOptions.ProfileCollapse;
            if (cbCompact.Checked) Sys.Settings.IntOptions |= InterfaceOptions.ProfileCollapse;
        }

        private void launchWithVariableDumpToolStripMenuItem_Click(object sender, EventArgs e) {

            Launch(true, false);

        }

        private void Launch(bool varDump, bool debug) {

            if (!SanityCheckCompat()) return;
            if (!SanityCheckSettings()) return;
            if (!VerifyOrdering()) return;
            string lib = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "7thWrapperLib.dll");
            if (Sys.ActiveProfile == null) {
                MessageBox.Show("Create a profile first");
                return;
            }
            if (Sys.ActiveProfile.Items.Count == 0) {
                MessageBox.Show("No mods have been activated.");
                //return;
            }
            if (!System.IO.File.Exists(Sys.Settings.FF7Exe)) {
                MessageBox.Show("FF7.exe not found. You may need to configure 7H using the Workshop/Settings menu.");
                return;
            }
            string ff7folder = System.IO.Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string data = System.IO.Path.Combine(ff7folder, "data");
            _7thWrapperLib.RuntimeProfile rp = new _7thWrapperLib.RuntimeProfile() {
                MonitorPaths = new List<string>() {
                    data,
                    Sys.Settings.AaliFolder,
                    Sys.Settings.MovieFolder,
                },
                ModPath = Sys.Settings.LibraryLocation,
                OpenGLConfig = Sys.ActiveProfile.OpenGLConfig,
                FF7Path = ff7folder,
                Mods = Sys.ActiveProfile.Items
                    .Select(i => i.GetRuntime(_context))
                    .Where(i => i != null)
                    .ToList()
            };

            rp.MonitorPaths.AddRange(Sys.Settings.ExtraFolders.Where(s => s.Length > 0).Select(s => System.IO.Path.Combine(ff7folder, s)));


            if (varDump) {
                rp.MonitorVars = _context.VarAliases.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();

                string tbl = "TurBoLog.exe";
                var psi = new ProcessStartInfo(tbl);
                psi.WorkingDirectory = System.IO.Path.GetDirectoryName(tbl);
                var aproc = Process.Start(psi);
                _also.Add(tbl, aproc);
                aproc.EnableRaisingEvents = true;
                aproc.Exited += (_o, _e) => _also.Remove(tbl);

            }
            foreach (string al in Sys.Settings.AlsoLaunch.Where(s => !String.IsNullOrWhiteSpace(s))) {
                if (!_also.ContainsKey(al)) {
                    string lal = al;
                    var psi = new ProcessStartInfo(lal);
                    psi.WorkingDirectory = System.IO.Path.GetDirectoryName(lal);
                    var aproc = Process.Start(psi);
                    _also.Add(lal, aproc);
                    aproc.EnableRaisingEvents = true;
                    aproc.Exited += (_o, _e) => _also.Remove(lal);
                }
            }

            string dir = System.IO.Path.GetDirectoryName(Sys.Settings.FF7Exe);
            string source = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string f1 = System.IO.Path.Combine(dir, "EasyHook.dll");
            if (!System.IO.File.Exists(f1))
                System.IO.File.Copy(System.IO.Path.Combine(source, "EasyHook.dll"), f1);
            string f2 = System.IO.Path.Combine(dir, "EasyHook32.dll");
            if (!System.IO.File.Exists(f2))
                System.IO.File.Copy(System.IO.Path.Combine(source, "EasyHook32.dll"), f2);

            if (debug) {
                rp.Options |= _7thWrapperLib.RuntimeOptions.DetailedLog;
                rp.LogFile = System.IO.Path.Combine(Sys.SysFolder, "log.txt");
            }

            /*
            /// 
            foreach (var mod in rp.Mods) mod.Startup();
            using (var fs = new System.IO.FileStream(System.IO.Path.Combine(data, "minigame", "chocobo.lgp"), System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                var lw = new _7thWrapperLib.LGPWrapper(fs.Handle, "chocobo.lgp", rp);
            }
            return;
            ////
             */

            int pid;
            try {
                _7thWrapperLib.RuntimeParams parms = new _7thWrapperLib.RuntimeParams {
                    ProfileFile = System.IO.Path.GetTempFileName()
                };
                using (var fs = new System.IO.FileStream(parms.ProfileFile, System.IO.FileMode.Create))
                    Util.SerializeBinary(rp, fs);

                // Add 640x480 and High DPI compatibility flags if set in settings
                if (Sys.Settings.Options.HasFlag(GeneralOptions.SetEXECompatFlags))
                {
                    RegistryKey ff7CompatKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                    ff7CompatKey.SetValue(Sys.Settings.FF7Exe, "~ 640X480 HIGHDPIAWARE");
                }
                EasyHook.RemoteHooking.CreateAndInject(Sys.Settings.FF7Exe, String.Empty, 0, lib, null, out pid, parms);

                var proc = System.Diagnostics.Process.GetProcessById(pid);
                if (proc != null) {
                    proc.EnableRaisingEvents = true;
                    if (debug) {
                        proc.Exited += (o, e) => {
                            System.Diagnostics.Process.Start(rp.LogFile);
                        };
                    }
                }
                foreach (var mod in rp.Mods) {
                    if (mod.LoadPlugins.Any()) {
                        mod.Startup();
                        foreach (string dll in mod.GetLoadPlugins()) {
                            _7HPlugin plugin;
                            if (!_plugins.TryGetValue(dll, out plugin)) {
                                var asm = System.Reflection.Assembly.LoadFrom(dll);
                                plugin = asm.GetType("_7thHeaven.Plugin")
                                    .GetConstructor(Type.EmptyTypes)
                                    .Invoke(null) as _7HPlugin;
                                _plugins.Add(dll, plugin);
                            }
                            plugin.Start(mod);
                        }
                    }
                }

                proc.Exited += (o, e) => {
                    foreach (var plugin in _plugins.Values)
                        plugin.Stop();
                };
            } catch (Exception e) {
                
                MessageBox.Show(e.ToString(), "Error starting FF7");

                return;
            }
        }

        private void chunkToolToolStripMenuItem_Click(object sender, EventArgs e) {
            new fChunks().ShowDialog(this);
        }

        private void launchWithDebugLogToolStripMenuItem_Click(object sender, EventArgs e) {
            Launch(false, true);
        }

        private void bGLConfig_Click(object sender, EventArgs e) {
            string config = (Sys.ActiveProfile.OpenGLConfig ?? String.Empty).Replace("\n", "\r\n");
            if (fTextEdit.Edit("Custom FF7_OpenGL.cfg", ref config))
                Sys.ActiveProfile.OpenGLConfig = config.Replace("\r\n", "\n");
        }

        private void openGLDriverConfigurationToolStripMenuItem_Click(object sender, EventArgs e) {
            string spec = System.IO.Path.Combine(Sys._7HFolder, "ConfigSpec-FF7OpenGL.xml");
            string cfg = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Sys.Settings.FF7Exe), "ff7_opengl.cfg");
            if (System.IO.File.Exists(spec) && System.IO.File.Exists(cfg)) {
                var cs = new fConfigSettings();
                cs.Init(spec, cfg);
                cs.ShowDialog(this);
            }
        }

        private void txtSearchC_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)13) {
                bSearchC.PerformClick();
            }
        }

        private void txtSearchL_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)13) 
                bSearchL.PerformClick();
        }

        private void TC_SelectedIndexChanged(object sender, EventArgs e) {
            switch (TC.SelectedIndex) {
                case 0:
                    pProfileOuter.Focus();
                    break;
                case 1:
                    pSearchResultsL.Focus();
                    bRefreshL_Click(sender, e);
                    break;

                case 2:
                    pSearchResultsC.Focus();
                    bRefreshC_Click(sender, e);
                    break;
            }
        }

        private void bProfileDetails_Click(object sender, EventArgs e) {
            string details = String.Join("\r\n", Sys.ActiveProfile.GetDetails());
            var result = CommonMark.CommonMarkConverter.Convert(details);

            fReadme.Display(result, "Profile Details");
        }

    }
}
