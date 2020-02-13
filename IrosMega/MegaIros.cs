/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using Microsoft.Win32.SafeHandles;

namespace Iros.Mega {
    public class MegaIros {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate int IrosFOpen(IntPtr handle, string filename, int read, int write);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IrosFRead(IntPtr handle, IntPtr buffer, long length, long pad, long pos);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IrosFWrite(IntPtr handle, IntPtr buffer, long length, long pos);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IrosFClose(IntPtr handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosHWriteback(IntPtr handle, byte[] data, int len);

        [StructLayout(LayoutKind.Sequential)]
        struct IrosHPostData {
            public IntPtr url;
            public IntPtr data;
            public uint len;
            public IntPtr contentType;
            public IrosHWriteback writeBack;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosHPost(IntPtr handle, ref IrosHPostData data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate long IrosHPostPos(IntPtr handle, IntPtr post);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IrosHDoIO(ref IntPtr pHandle, ref int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosHWaitIO(IntPtr handle, int ds);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosHClose(IntPtr handle);

        public enum NodeType : int {
            TYPE_UNKNOWN = -1,
            FILENODE = 0,
            FOLDERNODE,
            ROOTNODE,
            INCOMINGNODE,
            RUBBISHNODE,
            MAILNODE
        }


        public struct IrosNode {
            public long Handle;
            public long Parent;
            public NodeType Type;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Key;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Name;
        };

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosNodes(IntPtr nodes, int count, int options);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosTUpdate(int fd, long progress, long size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosTError(int fd, int httpcode, int count);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void IrosTFail(int td, string filename, int error);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void IrosTComplete(int td, string filename);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void IrosTOpened(int td, string filename);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void IrosLog(string msg);

        private enum LinkState {
            Starting = 0,
            Failed = 1,
            Connected = 2,
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IrosState(LinkState state);

        [StructLayout(LayoutKind.Sequential)]
        private struct IrosLinkCallbacks {
            public IrosFOpen FOpen;
            public IrosFRead FRead;
            public IrosFWrite FWrite;
            public IrosFClose FClose;

            public IrosHPost HPost;
            public IrosHPostPos HPostPos;
            public IrosHDoIO HDoIO;
            public IrosHWaitIO HWaitIO;
            public IrosHClose HClose;

            public IrosNodes Nodes;

            public IrosTUpdate TUpdate;
            public IrosTError TError;
            public IrosTFail TFail;
            public IrosTComplete TComplete;
            public IrosTOpened TOpened;

            public IrosState State;

            public IrosLog Log;
        }

        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int IrosTest(ref IrosLinkCallbacks link);

        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr IrosInit(IntPtr callbacks, string email, string pass);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void IrosClose(IntPtr cli);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void IrosSpin(IntPtr cli);
        //[DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //private unsafe static extern void IrosOpaque(int* ptr);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int IrosAttemptDownload(IntPtr cli, long node);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void IrosBeginDownload(IntPtr cli, int channel, string filename);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void IrosAddLink(IntPtr cli, string link);
        [DllImport(@"mega.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool IrosCancelDownload(IntPtr cli, int td);


        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern int ReadFile(IntPtr handle, IntPtr bytes, uint numBytesToRead, ref uint numBytesRead, IntPtr overlapped);
        [DllImport("kernel32.dll")]
        static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        private List<IrosNode> _nodes = new List<IrosNode>();

        public static Action<string> Logger { get; set; }

        static MegaIros() {
            Logger = s => { };
        }

        public IEnumerable<IrosNode> GetNodes() {
            lock (_nodes)
                return _nodes.ToArray();
        }

        private void Nodes(IntPtr nodes, int count, int options) {
            Logger(String.Format("Received {0} new nodes", count));
            lock (_nodes) {
                if (options != 0) _nodes.Clear();
                for (int i = 0; i < count; i++) {
                    IrosNode n;
                    n = (IrosNode)Marshal.PtrToStructure(nodes, typeof(IrosNode));
                    nodes = nodes + Marshal.SizeOf(n);
                    _nodes.Add(n);
                }
            }
            CheckWaiting(true);
            //IrosAttemptDownload(_cli, _nodes[0].Handle);
            //if (options != 0) IrosAddLink(_cli, "#!8Z4BQYrA!WbqytGasPT8QmBjZkVF1nQaRCqIW_QRmUFtJMYqxq9E");
        }

        private void CheckWaiting(bool clearFailed) {
            lock (_waitingTransfers) {
                lock (_nodes) {
                    foreach (var n in _nodes) {
                        var tfr = _waitingTransfers.Find(t => t.ID == n.Handle);
                        if (tfr == null) tfr = _waitingTransfers.Find(t => t.SourceName.Equals(n.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (tfr != null) {
                            tfr.ID = n.Handle;
                            int td = IrosAttemptDownload(_cli, tfr.ID);
                            Logger(String.Format("Attempting download of file {0} (resulting channel {1})", tfr.ID, td));
                            if (td >= 0) {
                                tfr.State = TransferState.Beginning;
                                tfr.TD = td;
                                _transfers[td] = tfr;
                            } else {
                                tfr.State = TransferState.Failed;
                            }
                            _waitingTransfers.Remove(tfr);
                            tfr.Notify();
                        }
                    }
                }
                if (clearFailed) {
                    for (int i = _waitingTransfers.Count - 1; i >= 0; i--) {
                        _waitingTransfers[i].State = TransferState.Failed;
                        _waitingTransfers[i].Notify();
                    }
                    _waitingTransfers.Clear();
                }
            }
        }

        private Dictionary<IntPtr, System.IO.FileStream> _files = new Dictionary<IntPtr, System.IO.FileStream>();

        private int _IrosFOpen(IntPtr handle, string filename, int read, int write) {
            System.IO.FileMode mode;
            System.IO.FileAccess access;

            mode = write != 0 ? (read != 0 ? System.IO.FileMode.Open : System.IO.FileMode.Create) : System.IO.FileMode.Open;
            access = write != 0 ? (read != 0 ? System.IO.FileAccess.ReadWrite : System.IO.FileAccess.Write) : System.IO.FileAccess.Read;

            _files[handle] = new System.IO.FileStream(filename, mode, access);
            Logger(String.Format("Opened file {0} for {1}", filename, access));
            return 1;
        }

        private int _IrosFRead(IntPtr handle, IntPtr buffer, long length, long pad, long pos) {
            System.IO.FileStream fs;
            if (_files.TryGetValue(handle, out fs)) {
                fs.Position = pos;
                uint read = 0;
                uint toRead = (uint)length;

                using (SafeFileHandle safeHandle = fs.SafeFileHandle)
                {
                    if (!safeHandle.IsInvalid)
                    {
                        ReadFile(safeHandle.DangerousGetHandle(), buffer, toRead, ref read, IntPtr.Zero);
                        Logger(String.Format("Read {0} bytes from file {1}", read, fs.Name));
                        return (int)read;
                    }
                }
            }

            return 0;
        }

        private int _IrosFWrite(IntPtr handle, IntPtr buffer, long length, long pos) {
            System.IO.FileStream fs;
            if (_files.TryGetValue(handle, out fs)) {
                fs.Position = pos;
                uint written = 0;

                using (SafeFileHandle safeHandle = fs.SafeFileHandle)
                {
                    if (!safeHandle.IsInvalid)
                    {
                        WriteFile(safeHandle.DangerousGetHandle(), buffer, (uint)length, out written, IntPtr.Zero);
                        Logger(String.Format("Written {0} bytes to file {1}", written, fs.Name));
                        return (int)written;
                    }
                }
            }

            return 0;
        }

        private int _IrosFClose(IntPtr handle) {
            System.IO.FileStream fs;
            if (_files.TryGetValue(handle, out fs)) {
                Logger(String.Format("Closed file {0}", fs.Name, fs));
                _files.Remove(handle);
                fs.Close();
            }
            return 0;
        }

        private class WebTask {
            public IntPtr Handle;
            public System.Net.WebRequest Req;
            public long Completed;
            public long Size;
            public IAsyncResult Async;
            public byte[] ReceiveBuffer;
            public IrosHWriteback writeBack;
            public bool ReceiveDone;
            public int Status;
        }

        public enum TransferState {
            Waiting,
            Attempting,
            Beginning,
            Transferring,
            Complete,
            Failed,
            Canceled,
        }

        public class Transfer {
            public string Filename;
            public long Size;
            public long Complete;
            public int TD;
            public TransferState State;

            public Action Notify;
            public string Source;
            public long ID;
            public string SourceName;

        }

        private Dictionary<int, Transfer> _transfers = new Dictionary<int, Transfer>();
        private List<Transfer> _waitingTransfers = new List<Transfer>();

        private void _IrosTUpdate(int fd, long progress, long size) {
            Transfer tr;
            if (_transfers.TryGetValue(fd, out tr)) {
                tr.Size = size;
                tr.Complete = progress;
                tr.Notify();
            }
        }

        private void _IrosTError(int td, int httpcode, int count) {
            Logger(String.Format("Transfer on channel {0} failed with error {1}", td, httpcode));
        }

        private void _IrosTFail(int td, string filename, int error) {
            Transfer t;
            if (_transfers.TryGetValue(td, out t)) {
                Logger(String.Format("Transfer on channel {0} failed with error {1}", td, error));
                t.State = TransferState.Failed;
                t.Notify();
            }
            _transfers.Remove(td);
        }

        private void _IrosTComplete(int td, string filename) {
            Transfer t;
            if (_transfers.TryGetValue(td, out t)) {
                t.State = TransferState.Complete;
                t.Notify();
            }
            _transfers.Remove(td);
        }

        private void _IrosTOpened(int td, string filename) {
            Transfer tfr;
            if (_transfers.TryGetValue(td, out tfr)) {
                IrosBeginDownload(_cli, td, tfr.Filename);
                tfr.State = TransferState.Transferring;
                tfr.Notify();
            }
        }

        private Dictionary<IntPtr, WebTask> _http = new Dictionary<IntPtr, WebTask>();

        private void _IrosHPost(IntPtr handle, ref IrosHPostData data) {
            string url = Marshal.PtrToStringAnsi(data.url);
            string ctype = Marshal.PtrToStringAnsi(data.contentType);

            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            req.ContentType = ctype;
            //req.Method = data.len > 0 ? "POST" : "GET";
            req.Method = "POST";

            Logger(String.Format("{3}ing {2} bytes to {0} handle {1}", url, handle, data.len, req.Method));

            byte[] buffer = new byte[data.len];
            Marshal.Copy(data.data, buffer, 0, buffer.Length);

            WebTask t = new WebTask() { Req = req, Handle = handle, writeBack = data.writeBack };
            _http[handle] = t;

            AsyncCallback doResponse = ar2 => {
                try {
                    var resp = req.EndGetResponse(ar2);
                    var rs = req.GetResponse().GetResponseStream();
                    t.Async = rs.BeginRead(t.ReceiveBuffer, 0, t.ReceiveBuffer.Length, AsyncReceive, t);
                } catch (WebException e) {
                    Logger(String.Format("Error receiving from {0}: {1}", handle, e.ToString()));
                    t.ReceiveDone = true;
                    if (e.Response != null)
                        t.Status = (int)((HttpWebResponse)e.Response).StatusCode;
                    else
                        t.Status = 500;
                }
            };
            t.ReceiveBuffer = new byte[0x10000];

            if (buffer.Length > 0) {
                t.Async = req.BeginGetRequestStream(ar => {
                    var s = req.EndGetRequestStream(ar);
                    using (s) {
                        s.Write(buffer, 0, buffer.Length);
                    }
                    t.Completed = t.Size = buffer.Length;
                    Logger(String.Format("Beginning GetResponse for {0}", handle));
                    t.Async = req.BeginGetResponse(doResponse, t);
                }, t);
            } else {
                Logger(String.Format("Beginning GetResponse [no request data] for {0}", handle));
                t.Async = req.BeginGetResponse(doResponse, t);
            }
        }

        void AsyncReceive(IAsyncResult ar) {
            WebTask t = (WebTask)ar.AsyncState;
            var resp = t.Req.GetResponse().GetResponseStream();
            int count = resp.EndRead(ar);
            Logger(String.Format("Received {0} bytes against handle {1}", count, t.Handle));
            t.ReceiveDone = (count == 0);
            if (count > 0) {
                t.writeBack(t.Handle, t.ReceiveBuffer, count);
                t.Async = resp.BeginRead(t.ReceiveBuffer, 0, t.ReceiveBuffer.Length, AsyncReceive, t);
            } else {
                t.Status = (int)((HttpWebResponse)t.Req.GetResponse()).StatusCode;
                t.Async = null;
            }
        }

        private long _IrosHPostPos(IntPtr handle, IntPtr post) {
            WebTask t;
            if (_http.TryGetValue(handle, out t))
                return t.Completed;
            return 0;
        }

        private int _IrosHDoIO(ref IntPtr pHandle, ref int status) {
            //            Logger(String.Format("IrosHDoIO...");
            status = 0;
            foreach (var task in _http.Values.ToArray()) {
                if (task.ReceiveDone) {
                    Logger(String.Format("Reporting complete against handle {0} status {1}", task.Handle, task.Status));
                    pHandle = task.Handle;
                    status = task.Status;
                    _http.Remove(task.Handle);
                    break;
                }
            }
            return 0;
        }

        private void _IrosHWaitIO(IntPtr handle, int ds) {
            if (ds <= 0) {

            } else {
                var handles = _http.Values
                    .Select(t => t.Async)
                    .Where(a => a != null)
                    .Select(a => a.AsyncWaitHandle)
                    .Concat(new[] { _wakeUp })
                    .ToArray();
                if (handles.Any()) { //which will always be true due to wakeup...
                    Logger(String.Format("Waiting for " + ds + " deciseconds"));
                    if (ds < int.MaxValue / 100)
                        ds *= 100;
                    else
                        ds = int.MaxValue;
                    int which = System.Threading.WaitHandle.WaitAny(handles, ds);
                    ProcessEvents();
                }
            }
        }

        private void ProcessEvents() {
            if (_state == LinkState.Connected) {
                lock (_waitingTransfers) {
                    /*
                    foreach (Transfer t in _waitingTransfers) {
                        if (t.State == TransferState.Waiting) {
                            IrosAddLink(_cli, t.Source);
                            t.State = TransferState.Attempting;
                        }
                    }
                     */
                }
            }
            _wakeUp.Reset();
        }

        private void _IrosHClose(IntPtr handle) {
            WebTask t;
            if (_http.TryGetValue(handle, out t) && t.Async != null) {

            }
            _http.Remove(handle);
        }

        private void _IrosState(LinkState state) {
            _state = state;
            Logger(String.Format("Mega LinkState became " + state.ToString()));
            _wakeUp.Set();
        }

        private IrosLinkCallbacks _link;
        private IntPtr _cli;
        private System.Threading.ManualResetEvent _wakeUp = new System.Threading.ManualResetEvent(false);
        private int _quit = 0;
        private string _user, _pass;
        private LinkState _state;

        public bool Dead { get; private set; }

        public MegaIros(string user, string pass) {
            System.Net.ServicePointManager.Expect100Continue = false;
            _user = user;
            _pass = pass;
            new System.Threading.Thread(ThreadSpin) { IsBackground = true, Name = "IrosMega" }.Start();
        }

        public void Quit() {
            _quit = -1;
            _wakeUp.Set();
        }

        public Transfer Download(string sourceID, string sourceName, string destinationFile, Action notify) {
            if (sourceID.StartsWith("#!")) sourceID = sourceID.Substring(2);

            Transfer t = new Transfer() { Source = sourceID, 
                Filename = destinationFile, 
                Notify = notify, 
                State = TransferState.Waiting, 
                Size = 1 
            };
            if (!String.IsNullOrEmpty(sourceID)) t.ID = Base64.atol(sourceID);
            t.SourceName = sourceName;
            lock (_waitingTransfers)
                _waitingTransfers.Add(t);
            return t;
        }

        public void CancelDownload(Transfer tfr) {
            if (tfr.TD >= 0) {
                IrosCancelDownload(_cli, tfr.TD);
                tfr.State = TransferState.Canceled;
                tfr.Notify();
            }
        }

        public void ConfirmStartTransfer() {
            CheckWaiting(false);
            _wakeUp.Set();
        }

        private void _IrosLog(string msg) {
            Logger(msg);
        }

        private void ThreadSpin(object o) {
            _link = new IrosLinkCallbacks() {
                FClose = _IrosFClose,
                FOpen = _IrosFOpen,
                FRead = _IrosFRead,
                FWrite = _IrosFWrite,
                HClose = _IrosHClose,
                HDoIO = _IrosHDoIO,
                HPost = _IrosHPost,
                HPostPos = _IrosHPostPos,
                HWaitIO = _IrosHWaitIO,
                Nodes = Nodes,
                TComplete = _IrosTComplete,
                TError = _IrosTError,
                TFail = _IrosTFail,
                TOpened = _IrosTOpened,
                TUpdate = _IrosTUpdate,
                State = _IrosState,
                Log = _IrosLog,
            };

            //IrosTest(ref _link);
            try {
                IntPtr ip = Marshal.AllocHGlobal(Marshal.SizeOf(_link));
                Marshal.StructureToPtr(_link, ip, false);
                _cli = IrosInit(ip, _user, _pass);
                while (_quit == 0) {
                    IrosSpin(_cli);
                }
                IrosClose(_cli);
                Marshal.DestroyStructure(ip, typeof(IrosLinkCallbacks));
            } catch (Exception e) {
                Dead = true;
                Logger("MegaIros died: " + e.ToString());
            }
        }
    }
}
