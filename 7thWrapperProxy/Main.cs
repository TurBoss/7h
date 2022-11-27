using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace _7thWrapperProxy
{
    static unsafe class Proxy
    {
        private static Assembly? lib;
        private static Type? t;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct HostExports
        {
            public delegate* unmanaged<ushort*, uint, uint, void*, uint, uint, void*, void*> CreateFileW;
            public delegate* unmanaged<void*, void*, uint, uint*, void*, int> ReadFile;
            //public delegate* unmanaged<void*, void*, uint, uint*, void*, int> WriteFile;
            public delegate* unmanaged<ushort*, void*, void*> FindFirstFileW;
            public delegate* unmanaged<void*, int, int*, uint, uint> SetFilePointer;
            public delegate* unmanaged<void*, long, long*, uint, int> SetFilePointerEx;
            public delegate* unmanaged<void*, int> CloseHandle;
            public delegate* unmanaged<void*, uint> GetFileType;
            public delegate* unmanaged<void*, void*, int> GetFileInformationByHandle;
            public delegate* unmanaged<void*, void*, void*, void**, uint, int, uint, int> DuplicateHandle;
            public delegate* unmanaged<void*, uint*, uint> GetFileSize;
            public delegate* unmanaged<void*, int*, int> GetFileSizeEx;
        }

        private static unsafe HostExports* _exports;

        [UnmanagedCallersOnly]
        public unsafe static int Main(void* exports)
        {
            try
            {
                _exports = (HostExports*)exports;

                _exports->CreateFileW = &HCreateFileW;
                _exports->ReadFile = &HReadFile;
                _exports->FindFirstFileW = &HFindFirstFileW;
                _exports->SetFilePointer = &HSetFilePointer;
                _exports->SetFilePointerEx = &HSetFilePointerEx;
                _exports->CloseHandle = &HCloseHandle;
                _exports->GetFileType = &HGetFileType;
                _exports->GetFileInformationByHandle = &HGetFileInformationByHandle;
                _exports->DuplicateHandle = &HDuplicateHandle;
                _exports->GetFileSize = &HGetFileSize;
                _exports->GetFileSizeEx = &HGetFileSizeEx;

                lib = Assembly.LoadFrom(Path.Combine(Directory.GetCurrentDirectory(), "7thWrapperLib.dll"));
                t = lib.GetType("_7thWrapperLib.Wrap");

                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                if (m != null) m.Invoke(null, new object[] { Process.GetCurrentProcess(), Type.Missing });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return 0;
        }

        [UnmanagedCallersOnly]
        public static void* HCreateFileW(ushort* lpFileName, uint dwDesiredAccess, uint dwShareMode, void* lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, void* hTemplateFile)
        {
            IntPtr ret = IntPtr.Zero;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HCreateFileW", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (IntPtr)(m.Invoke(null, new object[] { new string((char*)lpFileName), (System.IO.FileAccess)dwDesiredAccess, (System.IO.FileShare)dwShareMode, new IntPtr(lpSecurityAttributes), (System.IO.FileMode)dwCreationDisposition, (System.IO.FileAttributes)dwFlagsAndAttributes, new IntPtr(hTemplateFile) }) ?? IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret == IntPtr.Zero ? null : ret.ToPointer();
        }

        [UnmanagedCallersOnly]
        public static int HReadFile(void* handle, void* bytes, uint numBytesToRead, uint* numBytesRead, void* overlapped)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HReadFile", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(handle), new IntPtr(bytes), numBytesToRead, new IntPtr(numBytesRead), new IntPtr(overlapped) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static void* HFindFirstFileW(ushort* lpFileName, void* lpFindFileData)
        {
            IntPtr ret = IntPtr.Zero;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HFindFirstFileW", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (IntPtr)(m.Invoke(null, new object[] { new string((char*)lpFileName), new IntPtr(lpFindFileData) }) ?? IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret == IntPtr.Zero ? null : ret.ToPointer();
        }

        [UnmanagedCallersOnly]
        public static uint HSetFilePointer(void* hFile, int lDistanceTomove, int* lpDistanceToMoveHigh, uint dwMoveMethod)
        {
            uint ret = uint.MaxValue;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HSetFilePointer", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (uint)(m.Invoke(null, new object[] { new IntPtr(hFile), lDistanceTomove, new IntPtr(lpDistanceToMoveHigh), dwMoveMethod }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static int HSetFilePointerEx(void* hFile, long liDistanceToMove, long* lpNewFilePointer, uint dwMoveMethod)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HSetFilePointerEx", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(hFile), liDistanceToMove, new IntPtr(lpNewFilePointer), dwMoveMethod }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static int HCloseHandle(void* hObject)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HCloseHandle", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(hObject) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static uint HGetFileType(void* hFile)
        {
            uint ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HGetFileType", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (uint)(m.Invoke(null, new object[] { new IntPtr(hFile) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static int HGetFileInformationByHandle(void* hFile, void* lpFileInformation)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HGetFileInformationByHandle", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(hFile), new IntPtr(lpFileInformation) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static int HDuplicateHandle(void* hSourceProcessHandle, void* hSourceHandle, void* hTargetProcessHandle, void** lpTargetHandle, uint dwDesiredAccess, int bInheritHandle, uint dwOptions)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HDuplicateHandle", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(hSourceProcessHandle), new IntPtr(hSourceHandle), new IntPtr(hTargetProcessHandle), new IntPtr(lpTargetHandle), dwDesiredAccess, bInheritHandle, dwOptions }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static uint HGetFileSize(void* hFile, uint* lpFileSizeHigh)
        {
            uint ret = 0xFFFFFFFF;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HGetFileSize", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (uint)(m.Invoke(null, new object[] { new IntPtr(hFile), new IntPtr(lpFileSizeHigh) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }

        [UnmanagedCallersOnly]
        public static int HGetFileSizeEx(void* hFile, int* lpFileSize)
        {
            int ret = 0;

            try
            {
                MethodInfo? m = null;
                if (t != null) m = t.GetMethod("HGetFileSizeEx", BindingFlags.Static | BindingFlags.Public);
                if (m != null) ret = (int)(m.Invoke(null, new object[] { new IntPtr(hFile), new IntPtr(lpFileSize) }) ?? 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return ret;
        }
    }
}