/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
  Heavy rework was done by Julian Xhokaxhiu <https://julianxhokaxhiu.com>
  Additional help and support on .NET internals + low level wiring by Benjamin Moir <https://github.com/DaZombieKiller>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Diagnostics;
using static _7thWrapperLib.Win32;
using Iros._7th;

namespace _7thWrapperLib {
    public static unsafe class Wrap {
        private static Dictionary<IntPtr, LGPWrapper> _hMap = new Dictionary<IntPtr, LGPWrapper>();
        private static Dictionary<IntPtr, string> _hNames = new Dictionary<IntPtr, string>();
        private static Dictionary<IntPtr, VStreamFile> _streamFiles = new Dictionary<IntPtr, VStreamFile>();
        //private static Dictionary<IntPtr, string> _saveFiles = new Dictionary<IntPtr, string>();
        private static Dictionary<IntPtr, VArchiveData> _varchives = new Dictionary<IntPtr, VArchiveData>();
        private static RuntimeProfile _profile;

        private static void MonitorThread(object rpo)
        {
            RuntimeProfile rp = (RuntimeProfile)rpo;
            var accessors = rp.MonitorVars
                .Select(t => new { Name = t.Item1, Data = t.Item2.Split(':') })
                .Select(a => new { Type = (VarType)Enum.Parse(typeof(VarType), a.Data[0]), Addr = new IntPtr(RuntimeVar.Parse(a.Data[1])), Name = a.Name, Mask = a.Data.Length < 3 ? -1 : (int)RuntimeVar.Parse(a.Data[2]) })
                .Where(a => (int)a.Type <= 2)
                .ToList();
            int[] values = accessors.Select(_ => 247834893).ToArray();

            do
            {
                DebugLogger.WriteLine("MONITOR:");
                for (int i = 0; i < accessors.Count; i++)
                {
                    int value;
                    switch (accessors[i].Type)
                    {
                        case VarType.Int:
                            value = System.Runtime.InteropServices.Marshal.ReadInt32(accessors[i].Addr);
                            break;
                        case VarType.Short:
                            value = System.Runtime.InteropServices.Marshal.ReadInt16(accessors[i].Addr);
                            break;
                        case VarType.Byte:
                            value = System.Runtime.InteropServices.Marshal.ReadByte(accessors[i].Addr);
                            break;
                        default:
                            continue;
                    }
                    value = value & accessors[i].Mask;
                    if (value != values[i])
                    {
                        values[i] = value;
                        DebugLogger.WriteLine($"  {accessors[i].Name} = {value}");
                    }
                }
            } while (true);
        }

        public static void Run(Process currentProcess, string profileFile = ".7thWrapperProfile")
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US", false);
            try {
                using (var fs = new FileStream(profileFile, FileMode.Open))
                {
                    _profile = Iros._7th.Util.DeserializeBinary<RuntimeProfile>(fs);
                }

                File.Delete(profileFile);

                if (!String.IsNullOrWhiteSpace(_profile.LogFile)) {
                    try {
                        try { File.Delete(_profile.LogFile); } catch { } // ensure old log is deleted since new run

                        DebugLogger.Init(_profile.LogFile);
                        DebugLogger.IsDetailedLogging = _profile.Options.HasFlag(RuntimeOptions.DetailedLog);

                        DebugLogger.WriteLine("Logging debug output to " + _profile.LogFile);
                    } catch (Exception ex) {
                        DebugLogger.WriteLine("Failed to log debug output: " + ex.ToString());
                    }
                }

                DebugLogger.WriteLine($"Wrap run... PName: {currentProcess.ProcessName} PID: {currentProcess.Id} Path: {_profile.ModPath} Capture: {String.Join(", ", _profile.MonitorPaths)}");
                for (int i = _profile.MonitorPaths.Count - 1; i >= 0; i--) {
                    if (!_profile.MonitorPaths[i].EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                        _profile.MonitorPaths[i] += System.IO.Path.DirectorySeparatorChar;
                    if (String.IsNullOrWhiteSpace(_profile.MonitorPaths[i])) _profile.MonitorPaths.RemoveAt(i);
                }

                foreach (var item in _profile.Mods) {
                    DebugLogger.WriteLine($"  Mod: {item.BaseFolder} has {item.Conditionals.Count} conditionals");
                    DebugLogger.WriteLine("     Additional paths: " + String.Join(", ", item.ExtraFolders));
                    item.Startup();
                }

                if (_profile.MonitorVars != null)
                    new System.Threading.Thread(MonitorThread) { IsBackground = true }.Start(_profile);

                foreach (string LL in _profile.Mods.SelectMany(m => m.GetLoadLibraries())) {
                    DebugLogger.WriteLine($"Loading library DLL {LL}");
                    NativeLibrary.Load(LL);
                }

                foreach (var mod in _profile.Mods) {
                    foreach (string LA in mod.GetLoadAssemblies()) {
                        DebugLogger.WriteLine($"Loading assembly DLL {LA}");
                        var asm = System.Reflection.Assembly.LoadFrom(LA);
                        try {
                            string path = mod.BaseFolder;
                            asm.GetType("_7thHeaven.Main")
                                .GetMethod("Init", new[] { typeof(RuntimeMod) })
                                .Invoke(null, new object[] { mod });
                        } catch { }
                    }
                }

                foreach (var mod in _profile.Mods.AsEnumerable().Reverse()) {
                    foreach (string file in mod.GetPathOverrideNames("hext")) {
                        foreach (var of in mod.GetOverrides("hext\\" + file)) {
                            System.IO.Stream s;
                            if (of.Archive == null) {
                                s = new System.IO.FileStream(of.File, FileMode.Open, FileAccess.Read);
                            } else {
                                s = of.Archive.GetData(of.File);
                            }
                            DebugLogger.WriteLine($"Applying hext patch {file} from mod {mod.BaseFolder}");
                            try {
                                HexPatch.Apply(s);
                            } catch (Exception ex) {
                                DebugLogger.WriteLine("Error applying patch: " + ex.Message);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                DebugLogger.WriteLine(e.ToString());
                return;
            }
        }

        // ------------------------------------------------------------------------------------------------------
        public static int HCloseHandle(IntPtr hObject)
        {
            int ret = 0;

            if (_varchives.ContainsKey(hObject))
            {
                _varchives.Remove(hObject);
                DebugLogger.WriteLine($"Closing dummy handle {hObject}");
            }

            if (_streamFiles.ContainsKey(hObject))
                _streamFiles.Remove(hObject);

            //if (_saveFiles.ContainsKey(hObject))
            //    _saveFiles.Remove(hObject);

            //ret = s_Trampolines.CloseHandle(hObject);

            return ret;
        }

        public static uint HGetFileType(IntPtr hFile)
        {            
            uint ret = 0;

            DebugLogger.DetailedWriteLine($"GetFileType on {hFile}");
            VArchiveData va;
            if (_varchives.TryGetValue(hFile, out va))
            {
                //DebugLogger.WriteLine(" ---faking dummy file");
                ret = 1;
            }
            else
            {
                //ret = s_Trampolines.GetFileType(hFile);
            }

            return ret;
        }

        public static uint HSetFilePointer(IntPtr hFile, int lDistanceTomove, IntPtr lpDistanceToMoveHigh, uint dwMoveMethod)
        {
            //DebugLogger.WriteLine("SetFilePointer on {0} to {1} by {2}", handle, lDistanceTomove, dwMoveMethod);
            uint ret = uint.MaxValue;

            VArchiveData va;
            VStreamFile vsf;
            int offset = lDistanceTomove;
            if (!lpDistanceToMoveHigh.Equals(IntPtr.Zero))
                offset |= Marshal.ReadInt32(lpDistanceToMoveHigh) << 32;

            if (_varchives.TryGetValue(hFile, out va))
            {
                ret = (uint)va.SetFilePointer(offset, (Win32.EMoveMethod)dwMoveMethod);
            }
            else if (_streamFiles.TryGetValue(hFile, out vsf))
            {
                ret = (uint)_streamFiles[hFile].SetPosition(offset, (Win32.EMoveMethod)dwMoveMethod);
            }
            else
            {
                //ret = s_Trampolines.SetFilePointer(handle, lDistanceTomove, lpDistanceToMoveHigh, dwMoveMethod);
            }
            
            return ret;
        }

        //public static int HWriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, uint lpNumberOfBytesWritten, [In] ref System.Threading.NativeOverlapped lpOverlapped)
        //{
        //    int result = 0;

        //    result = WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, ref lpOverlapped);
        //    //DebugLogger.WriteLine(String.Format("Write {0} bytes on {1}", lpNumberOfBytesWritten, hFile.ToInt32()));

        //    if (_saveFiles.ContainsKey(hFile))
        //    {
        //        int offset = SetFilePointer(hFile, 0, IntPtr.Zero, EMoveMethod.Current);
        //        //DebugLogger.WriteLine(String.Format("Write {0} bytes to {1} at offset {2}", lpNumberOfBytesWritten, _saveFiles[hFile], offset));
        //    }

        //    return result;
        //}

        public static int HReadFile(IntPtr handle, IntPtr bytes, uint numBytesToRead, IntPtr numBytesRead, IntPtr overlapped)
        {
            int ret = 0;

            uint _numBytesRead = 0;
            VArchiveData va;
            if (_varchives.TryGetValue(handle, out va))
            {
                ret = va.ReadFile(bytes, numBytesToRead, ref _numBytesRead);
                uint* ptrNumBytesRead = (uint*)numBytesRead.ToPointer();
                *ptrNumBytesRead = _numBytesRead;
                return ret;
            }

            VStreamFile vsf;
            if (_streamFiles.TryGetValue(handle, out vsf))
            {
                ret = vsf.Read(bytes, _numBytesRead, ref _numBytesRead);
                uint* ptrNumBytesRead = (uint*)numBytesRead.ToPointer();
                *ptrNumBytesRead = _numBytesRead;
                return ret;
            }

            //DebugLogger.WriteLine("Hooked ReadFile on {0} for {1} bytes", handle.ToInt32(), numBytesToRead);
            //if (overlapped != IntPtr.Zero) DebugLogger.WriteLine("(is overlapped)");

            LGPWrapper lgp;
            if (_hMap.TryGetValue(handle, out lgp))
            {
                try
                {
                    int pos = SetFilePointer(handle, 0, IntPtr.Zero, EMoveMethod.Current);
                    //DebugLogger.WriteLine("Hooked ReadFile on {0} for {1} bytes at {2}", handle.ToInt32(), numBytesToRead, pos);
                    lgp.VFile.Read((uint)pos, numBytesToRead, bytes, ref _numBytesRead);
                    uint* ptrNumBytesRead = (uint*)numBytesRead.ToPointer();
                    *ptrNumBytesRead = _numBytesRead;
                    //DebugLogger.WriteLine("--{0} bytes read", numBytesRead);
                    SetFilePointer(handle, (int)(pos + numBytesRead), IntPtr.Zero, EMoveMethod.Begin);
                    lgp.Ping();
                    ret = -1;
                }
                catch (Exception e)
                {
                    DebugLogger.WriteLine("ERROR: " + e.ToString());
                    throw;
                }
            }

            return ret;
            // return s_Trampolines.ReadFile(handle, bytes, numBytesToRead, numBytesRead, overlapped);
        }

        public static IntPtr CreateVA(OverrideFile of) {
            VArchiveData va = new VArchiveData(of.Archive.GetBytes(of.File));
            IntPtr dummy = of.Archive.GetDummyHandle();
            DebugLogger.WriteLine($"Creating dummy file handle {dummy} to access {of.Archive}{of.File}");
            _varchives[dummy] = va;

            return dummy;
        }

        public static IntPtr HCreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile)
        {
            IntPtr ret = IntPtr.Zero;

            // Usually this check should be enough...
            bool isFF7GameFile = lpFileName.StartsWith(_profile.FF7Path, StringComparison.InvariantCultureIgnoreCase);
            // ...but if it fails, last resort is to check if the file exists in the game directory
            if (!isFF7GameFile && !lpFileName.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase) && !Path.IsPathRooted(lpFileName))
            {
                isFF7GameFile = _profile.gameFiles.Any(s => s.EndsWith(lpFileName, StringComparison.InvariantCultureIgnoreCase));
            }

            // If a game file is found, process with replacing its content with relative mod file
            if (isFF7GameFile)
            {
                lpFileName = lpFileName.Replace("\\/", "\\").Replace("/", "\\").Replace("\\\\", "\\");
                DebugLogger.DetailedWriteLine($"CreateFileW for {lpFileName}...");
                if (lpFileName.IndexOf('\\') < 0)
                {
                    //DebugLogger.WriteLine("No path: curdir is {0}", System.IO.Directory.GetCurrentDirectory(), 0);
                    lpFileName = Path.Combine(Directory.GetCurrentDirectory(), lpFileName);
                }

                foreach (string path in _profile.MonitorPaths)
                {
                    if (lpFileName.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string match = lpFileName.Substring(path.Length);
                        OverrideFile mapped = LGPWrapper.MapFile(match, _profile);

                        //DebugLogger.WriteLine($"Attempting match '{match}' for {lpFileName}...");

                        if (mapped == null)
                        {
                            // Attempt a second round, this time relaxing the path match replacing only the game folder path.
                            match = lpFileName.Substring(_profile.FF7Path.Length + 1);
                            mapped = LGPWrapper.MapFile(match, _profile);

                            //DebugLogger.WriteLine($"Attempting match '{match}' for {lpFileName}...");
                        }

                        if (mapped != null)
                        {
                            DebugLogger.WriteLine($"Remapping {lpFileName} to {mapped.File} [ Matched: '{match}' ]");

                            if (mapped.Archive == null)
                            {
                                lpFileName = mapped.File;
                            }
                            else
                            {
                                ret = CreateVA(mapped);
                                break;
                            }
                        }
                    }
                }
            }
            else
                DebugLogger.DetailedWriteLine($"Skipped file {lpFileName}");

            if (ret == IntPtr.Zero)
                ret = CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);

            //DebugLogger.WriteLine("Hooked CreateFileW for {0} under {1}", lpFileName, handle.ToInt32());

            //if (isFF7GameFile && ret != null)
            //{
            //    IntPtr _ret = new IntPtr(ret);

            //    if (System.IO.Path.GetExtension(lpFileName).Equals(".ff7", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        _saveFiles.Add(_ret, lpFileName);
            //    }

            //    DebugLogger.DetailedWriteLine($"CreateFileW: {lpFileName} -> {_ret}");
            //}

            return ret;
        }

        public static IntPtr HFindFirstFileW(string lpFileName, IntPtr lpFindFileData)
        {
            DebugLogger.WriteLine("FindFirstFile for " + lpFileName);
            //ret = s_Trampolines.FindFirstFileW(lpFileName, lpFindFileData);

            return IntPtr.Zero;
        }

        public static uint HGetFileInformationByHandle(IntPtr hFile, IntPtr lpFileInformation)
        {
            VArchiveData va;
            BY_HANDLE_FILE_INFORMATION _lpFileInformation;

            bool result = GetFileInformationByHandle(hFile, out _lpFileInformation);
            if (result && _varchives.TryGetValue(hFile, out va))
            {
                DebugLogger.DetailedWriteLine($"Overriding GetFileInformationByHandle for dummy file {hFile}");
                BY_HANDLE_FILE_INFORMATION* ptr = (BY_HANDLE_FILE_INFORMATION*)lpFileInformation;

                *ptr = _lpFileInformation;
                ptr->FileSizeHigh = (uint)(va.Size >> 32);
                ptr->FileSizeLow = (uint)(va.Size & 0xffffffff);
            }

            return (uint)(result ? 1 : 0);
        }

        public static int HDuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, IntPtr lpTargetHandle, uint dwDesiredAccess, int bInheritHandle, uint dwOptions)
        {
            // DebugLogger.DetailedWriteLine("DuplicateHandle on {0}", hSourceHandle);

            if (_varchives.ContainsKey(hSourceHandle))
            {
                _varchives[lpTargetHandle] = _varchives[hSourceHandle];
                DebugLogger.DetailedWriteLine($"Duplicating dummy handle {hSourceHandle} to {lpTargetHandle}");
            }

            return 1;
        }

        public static uint HGetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh)
        {
            uint ret = 0xFFFFFFFF;

            VArchiveData va;
            if (_varchives.TryGetValue(hFile, out va))
                ret = va.GetFileSize(lpFileSizeHigh);
            //else
            //    ret = s_Trampolines.GetFileSize(hFile, lpFileSizeHigh);

            return ret;
        }

        public static uint HGetFileSizeEx(IntPtr hFile, IntPtr lpFileSize)
        {
            uint ret = 0;

            VArchiveData va;
            if (_varchives.TryGetValue(hFile, out va))
            {
                DebugLogger.WriteLine($"GetFileSizeEx on dummy handle {hFile}");

                long* _lpFileSize = (long*)lpFileSize.ToPointer();
                *_lpFileSize = va.Size;
                ret = 1;
            }
            //else
            //    ret = s_Trampolines.GetFileSizeEx(hFile, lpFileSize);

            return ret;
        }

        public static uint HSetFilePointerEx(IntPtr hFile, long liDistanceToMove, IntPtr lpNewFilePointer, uint dwMoveMethod)
        {
            uint ret = 0;

            VArchiveData va;
            if (_varchives.TryGetValue(hFile, out va))
                ret = (uint)va.SetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, (uint)dwMoveMethod);
            //else
            //    ret = s_Trampolines.SetFilePointerEx(hFile, liDistanceToMove, lpNewFilePointer, dwMoveMethod);

            return ret;
        }
    }
}
