/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using _7thHeaven.Code;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iros._7th.Workshop
{
    public static class Install
    {
        public static string SafeStr(string s)
        {
            return new string(s.Select(c => Char.IsLetterOrDigit(c) ? c : '_').ToArray());
        }

        private enum DeleteStatus
        {
            DidntStart,
            Partial,
            Complete,
        }

        private static DeleteStatus CheckDeleteAll(IEnumerable<string> files, IEnumerable<string> dirs, out string err)
        {
            foreach (string f in files)
            {
                try
                {
                    using (var fs = new System.IO.FileStream(f, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite)) { }
                }
                catch (System.IO.IOException ex)
                {
                    err = String.Format("Could not access file {0} - is it in use? {1}", f, ex.ToString());
                    return DeleteStatus.DidntStart;
                }
            }
            bool partial = false;
            foreach (string f in files)
            {
                try
                {
                    System.IO.File.Delete(f);
                }
                catch (System.IO.IOException)
                {
                    partial = true;
                    continue;
                }
            }
            foreach (string d in dirs)
            {
                try
                {
                    System.IO.Directory.Delete(d);
                }
                catch (System.IO.IOException)
                {
                    partial = true;
                    continue;
                }
            }
            err = null;
            return partial ? DeleteStatus.Partial : DeleteStatus.Complete;
        }

        public static void Uninstall(InstalledItem ii)
        {
            /*
            List<string> files = new List<string>(), dirs = new List<string>();
            foreach (var ver in ii.Versions) {
                string file = System.IO.Path.Combine(Sys.Settings.LibraryLocation, ver.InstalledLocation);
                if (System.IO.File.Exists(file))
                    files.Add(ver.InstalledLocation);
                else if (System.IO.Directory.Exists(file))
                    dirs.Add(ver.InstalledLocation);
            }

            string err;
            switch (CheckDeleteAll(files, dirs, out err)) {
                case DeleteStatus.DidntStart:
                    Sys.Message(new WMessage() { Text = err });
                    return;
                case DeleteStatus.Partial:
                    Sys.Message(new WMessage() { Text = "Some files could not be deleted, were they in use?" });
                    break;
            }
             */

            Sys.Library.QueuePendingDelete(ii.Versions.Select(v => v.InstalledLocation));
            Sys.Library.RemoveInstall(ii);
            Sys.Message(new WMessage() { Text = "Uninstalled " + ii.CachedDetails.Name });
            Sys.Library.AttemptDeletions();
        }

        public static void DownloadAndInstall(Mod m)
        {
            Action onCancel = () => Sys.RevertStatus(m.ID);
            Action<Exception> onError = ex =>
            {
                Sys.Message(new WMessage($"Error ocurred downloading/installing {m.Name} - {ex.Message}"));
                Sys.RevertStatus(m.ID);
            };


            string temppath = System.IO.Path.Combine(Sys.Settings.LibraryLocation, "temp");
            System.IO.Directory.CreateDirectory(temppath);

            var install = Sys.Library.GetItem(m.ID);
            if (install != null)
            {
                var patches = m.GetPatchesFromTo(install.LatestInstalled.VersionDetails.Version, m.LatestVersion.Version);
                if (patches.Any())
                {
                    string pfile = String.Format("{0}_{1}_{2}.irop", m.ID, SafeStr(m.Name), m.LatestVersion.Version);
                    Sys.SetStatus(m.ID, ModStatus.Downloading);
                    Sys.Downloads.Download(patches.Select(p => p.Link),
                        System.IO.Path.Combine(temppath, pfile),
                        "Downloading " + m.Name,
                        new PatchModProcedure()
                        {
                            File = pfile,
                            Install = install,
                            Mod = m,
                            Error = onError
                        },
                        onCancel
                    );
                    return;
                }
            }

            string file = String.Format("{0}_{1}_{2}.iro", m.ID, SafeStr(m.Name), m.LatestVersion.Version);
            Sys.SetStatus(m.ID, ModStatus.Downloading);

            if (m.LatestVersion.PatchLinks.Any())
            {
                PatchController pc = new PatchController(m.LatestVersion.PatchLinks.Count);

                Sys.Downloads.Download(m.LatestVersion.Links,
                    System.IO.Path.Combine(temppath, file),
                    "Downloading " + m.Name,
                    new DownloadThenPatchProcedure()
                    {
                        File = file,
                        Mod = m,
                        Controller = pc,
                        Error = onError
                    },
                    onCancel
                );
                int pCount = 0;
                foreach (string p in m.LatestVersion.PatchLinks)
                {
                    string pfile = String.Format("{0}_{1}_{2}_patch{3}.iro", m.ID, SafeStr(m.Name), m.LatestVersion.Version, pCount);
                    pc.PatchFiles.Add(pfile);
                    Sys.Downloads.Download(p,
                        System.IO.Path.Combine(temppath, pfile),
                        "Downloading " + m.Name + " patch " + pCount,
                        new DownloadPatchProcedure()
                        {
                            File = pfile,
                            Controller = pc,
                            Error = ex =>
                            {
                                pc.PatchFailed();
                                Sys.RevertStatus(m.ID);
                            }
                        },
                        () =>
                        {
                            pc.PatchFailed();
                            Sys.RevertStatus(m.ID);
                        }
                    );

                    pCount++;
                }

            }
            else
            {
                Sys.Downloads.Download(m.LatestVersion.Links,
                    System.IO.Path.Combine(temppath, file),
                    "Downloading " + m.Name,
                    new InstallModProcedure()
                    {
                        File = file,
                        Mod = m,
                        ExtractSubFolder = m.LatestVersion.ExtractSubFolder,
                        ExtractInto = m.LatestVersion.ExtractInto,
                        Error = onError
                    },
                    onCancel
                );
            }
        }

        public abstract class InstallProcedure
        {
            public Action<int> SetPCComplete;
            public Action Complete;
            public Action<Exception> Error;

            public abstract void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e); //Called after Complete() has run and should tidy everything up, notify user, etc.
            public abstract void Schedule(); //Needs to process the downloaded file - possibly on a background thread - and afterwards, call Complete() (or Error())
        }

        public class InstallProcedureCallback : InstallProcedure
        {
            private Action<System.ComponentModel.AsyncCompletedEventArgs> _callback;

            public InstallProcedureCallback(Action<System.ComponentModel.AsyncCompletedEventArgs> callback)
            {
                try
                {
                    _callback = callback;
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }

            public override void Schedule()
            {
                SetPCComplete(100);
                Complete();
            }

            public override void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e)
            {
                _callback(e);
            }
        }

        private class PatchController
        {
            private System.Threading.AutoResetEvent _event;
            private int _pending;

            public List<string> PatchFiles { get; private set; }
            public PatchController(int numPatches)
            {
                _event = new System.Threading.AutoResetEvent(false);
                _pending = numPatches;
                PatchFiles = new List<string>();
            }

            public void PatchReady()
            {
                int remaining = System.Threading.Interlocked.Decrement(ref _pending);
                _event.Set();
            }
            public void PatchFailed()
            {
                _pending = -1;
                _event.Set();
            }

            public bool WaitForPatches()
            {
                while (_pending > 0)
                {
                    _event.WaitOne();
                }
                return _pending == 0;
            }
        }

        private class DownloadThenPatchProcedure : InstallProcedure
        {
            private string _dest;
            public string File { get; set; }
            public Mod Mod { get; set; }
            public PatchController Controller { get; set; }

            private void ProcessDownload(object state)
            {
                try
                {
                    string source = System.IO.Path.Combine(Sys.Settings.LibraryLocation, "temp", File);
                    _dest = System.IO.Path.Combine(Sys.Settings.LibraryLocation, File);
                    byte[] sig = new byte[4];
                    using (var iro = new _7thWrapperLib.IrosArc(source, true))
                    {
                        if (!iro.CheckValid())
                            throw new Exception("IRO archive appears to be invalid: corrupted download?");
                        if (!Controller.WaitForPatches())
                            throw new Exception("Failed to acquire patches");

                        int numPatch = Controller.PatchFiles.Count;
                        int pDone = 0;
                        foreach (string pfile in Controller.PatchFiles)
                        {
                            string patchfile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, "temp", pfile);
                            using (var patch = new _7thWrapperLib.IrosArc(patchfile))
                            {
                                iro.ApplyPatch(patch, (d, _) => SetPCComplete((int)((100 / numPatch) * pDone + 100 * d / numPatch)));
                            }
                            pDone++;
                        }
                    }
                    System.IO.File.Move(source, _dest);
                }
                catch (Exception e)
                {
                    Error(e);
                    return;
                }
                SetPCComplete(100);
                Complete();
            }

            public override void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Sys.Message(new WMessage() { Text = "Error downloading " + Mod.Name + "\r\n" + e.Error.ToString() });
                    Sys.RevertStatus(Mod.ID);
                }
                else
                {
                    //ProcessDownload(file);
                    var inst = Sys.Library.GetItem(Mod.ID);
                    if (inst == null)
                    {
                        Mod = new ModImporter().ParseModXmlFromSource(_dest, Mod);

                        Sys.Library.AddInstall(new InstalledItem()
                        {
                            ModID = Mod.ID,
                            CachedDetails = Mod,
                            UpdateType = Sys.Library.DefaultUpdate,
                            Versions = new List<InstalledVersion>() { new InstalledVersion() { InstalledLocation = System.IO.Path.GetFileName(_dest), VersionDetails = Mod.LatestVersion } }
                        });
                        Sys.Message(new WMessage() { Text = "Installed " + Mod.Name, Link = "iros://" + Mod.ID.ToString() });
                        Sys.SetStatus(Mod.ID, ModStatus.Installed);
                    }
                    else
                    {
                        inst.CachedDetails = new ModImporter().ParseModXmlFromSource(_dest, Mod);
                        if (!Sys.Settings.HasOption(GeneralOptions.KeepOldVersions))
                        {
                            foreach (string ivfile in inst.Versions.Select(v => v.InstalledLocation))
                            {
                                string ifile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, ivfile);
                                if (System.IO.File.Exists(ifile))
                                    System.IO.File.Delete(ifile);
                                else if (System.IO.Directory.Exists(ifile))
                                    System.IO.Directory.Delete(ifile, true);
                            }
                            inst.Versions.Clear();
                        }
                        inst.Versions.Add(new InstalledVersion() { InstalledLocation = System.IO.Path.GetFileName(_dest), VersionDetails = Mod.LatestVersion });
                        Sys.Message(new WMessage() { Text = "Updated " + Mod.Name, Link = "iros://" + Mod.ID.ToString() });
                        Sys.SetStatus(Mod.ID, ModStatus.Installed);
                    }
                    Sys.Save();
                }
            }

            public override void Schedule()
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ProcessDownload);
            }
        }

        private class DownloadPatchProcedure : InstallProcedure
        {
            public string File { get; set; }
            public PatchController Controller { get; set; }

            private void ProcessDownload(object state)
            {
                Controller.PatchReady();
                SetPCComplete(100);
                Complete();
            }
            public override void Schedule()
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ProcessDownload);
            }
            public override void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e)
            {
            }
        }

        private class PatchModProcedure : InstallProcedure
        {
            public string File { get; set; }
            public Mod Mod { get; set; }
            public InstalledItem Install { get; set; }

            private void ProcessDownload(object state)
            {
                string patchfile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, "temp", File);
                try
                {
                    string source = System.IO.Path.Combine(Sys.Settings.LibraryLocation, Install.LatestInstalled.InstalledLocation);
                    using (var iro = new _7thWrapperLib.IrosArc(source, true))
                    {
                        using (var patch = new _7thWrapperLib.IrosArc(patchfile))
                        {
                            iro.ApplyPatch(patch, (d, _) => SetPCComplete((int)(100 * d)));
                        }
                    }
                }
                catch (Exception e)
                {
                    Error(e);
                    return;
                }
                Sys.Library.QueuePendingDelete(new[] { patchfile });
                SetPCComplete(100);
                Complete();
            }

            public override void Schedule()
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ProcessDownload);
            }
            public override void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Sys.Message(new WMessage() { Text = "Error updating " + Mod.Name + "\r\n" + e.Error.ToString() });
                    Sys.RevertStatus(Mod.ID);
                }
                else
                {
                    Install.CachedDetails = Mod;
                    Install.LatestInstalled.VersionDetails = Mod.LatestVersion;
                    Sys.Message(new WMessage() { Text = "Updated " + Mod.Name, Link = "iros://" + Mod.ID.ToString() });
                    Sys.SetStatus(Mod.ID, ModStatus.Installed);
                }
                Sys.Save();
            }
        }

        private class InstallModProcedure : InstallProcedure
        {
            public string File { get; set; }
            public Mod Mod { get; set; }
            public string ExtractSubFolder { get; set; }
            public string ExtractInto { get; set; }
            private string _dest;

            private void ProcessDownload(object state)
            {
                try
                {
                    string source = System.IO.Path.Combine(Sys.Settings.LibraryLocation, "temp", File);
                    _dest = System.IO.Path.Combine(Sys.Settings.LibraryLocation, File);
                    byte[] sig = new byte[4];
                    using (var fs = new System.IO.FileStream(source, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        fs.Read(sig, 0, 4);
                        int isig = BitConverter.ToInt32(sig, 0);
                        if (isig == _7thWrapperLib.IrosArc.SIG)
                        { //plain IRO file, just move into place
                            fs.Close();
                            using (var iro = new _7thWrapperLib.IrosArc(source))
                                if (!iro.CheckValid())
                                    throw new Exception("IRO archive appears to be invalid: corrupted download?");
                            System.IO.File.Move(source, _dest);
                        }
                        else
                        {
                            var archive = ArchiveFactory.Open(fs);
                            var iroEnt = archive.Entries.FirstOrDefault(e => System.IO.Path.GetExtension(e.Key).Equals(".iro", StringComparison.InvariantCultureIgnoreCase));
                            if (iroEnt != null)
                            {
                                iroEnt.WriteToFile(_dest);
                            }
                            else
                            { //extract entire archive...
                                if (_dest.EndsWith(".iro", StringComparison.InvariantCultureIgnoreCase)) _dest = _dest.Substring(0, _dest.Length - 4);
                                var entries = archive.Entries.ToArray();
                                string extractTo = _dest;
                                if (!String.IsNullOrEmpty(ExtractInto)) extractTo = System.IO.Path.Combine(extractTo, ExtractInto);
                                System.IO.Directory.CreateDirectory(extractTo);
                                using (var reader = archive.ExtractAllEntries())
                                {
                                    int count = 0;
                                    while (reader.MoveToNextEntry())
                                    {
                                        if (!reader.Entry.IsDirectory)
                                        {
                                            string filepath = reader.Entry.Key.Replace('/', '\\');
                                            if (String.IsNullOrEmpty(ExtractSubFolder) || filepath.StartsWith(ExtractSubFolder, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                string path = System.IO.Path.Combine(extractTo, filepath.Substring(ExtractSubFolder.Length).TrimStart('\\', '/'));
                                                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                                                using (var fOut = new System.IO.FileStream(path, System.IO.FileMode.Create))
                                                    reader.WriteEntryTo(fOut);
                                            }
                                            count++;
                                            SetPCComplete(100 * count / entries.Length);
                                        }
                                    }
                                }
                            }
                            fs.Close();
                            System.IO.File.Delete(source);
                        }
                    }
                }
                catch (Exception e)
                {
                    Error(e);
                    return;
                }
                SetPCComplete(100);
                Complete();
            }

            public override void DownloadComplete(System.ComponentModel.AsyncCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    Sys.Message(new WMessage() { Text = "Error downloading " + Mod.Name + "\r\n" + e.Error.ToString() });
                    Sys.RevertStatus(Mod.ID);
                }
                else
                {
                    //ProcessDownload(file);
                    var inst = Sys.Library.GetItem(Mod.ID);
                    if (inst == null)
                    {
                        Mod = new ModImporter().ParseModXmlFromSource(_dest, Mod);

                        Sys.Library.AddInstall(new InstalledItem()
                        {
                            ModID = Mod.ID,
                            CachedDetails = Mod,
                            UpdateType = Sys.Library.DefaultUpdate,
                            Versions = new List<InstalledVersion>() { new InstalledVersion() { InstalledLocation = System.IO.Path.GetFileName(_dest), VersionDetails = Mod.LatestVersion } }
                        });
                        Sys.Message(new WMessage() { Text = "Installed " + Mod.Name, Link = "iros://" + Mod.ID.ToString() });
                        Sys.SetStatus(Mod.ID, ModStatus.Installed);
                    }
                    else
                    {
                        inst.CachedDetails = new ModImporter().ParseModXmlFromSource(_dest, Mod);

                        if (!Sys.Settings.HasOption(GeneralOptions.KeepOldVersions))
                        {
                            foreach (string ivfile in inst.Versions.Select(v => v.InstalledLocation))
                            {
                                string ifile = System.IO.Path.Combine(Sys.Settings.LibraryLocation, ivfile);
                                if (System.IO.File.Exists(ifile))
                                    System.IO.File.Delete(ifile);
                                else if (System.IO.Directory.Exists(ifile))
                                    System.IO.Directory.Delete(ifile, true);
                            }
                            inst.Versions.Clear();
                        }
                        inst.Versions.Add(new InstalledVersion() { InstalledLocation = System.IO.Path.GetFileName(_dest), VersionDetails = Mod.LatestVersion });
                        Sys.Message(new WMessage() { Text = "Updated " + Mod.Name, Link = "iros://" + Mod.ID.ToString() });
                        Sys.SetStatus(Mod.ID, ModStatus.Installed);
                    }
                    Sys.Save();
                }
            }

            public override void Schedule()
            {
                System.Threading.ThreadPool.QueueUserWorkItem(ProcessDownload);
            }
        }


    }
}
