/*
  This source is subject to the Microsoft Public License. See LICENSE.TXT for details.
  The original developer is Iros <irosff@outlook.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace _7thWrapperLib {
    static class Win32 {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
           IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
           uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();


        internal struct OVERLAPPED {
            public UIntPtr Internal;
            public UIntPtr InternalHigh;
            public uint Offset;
            public uint OffsetHigh;
            public IntPtr EventHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern unsafe int ReadFile(IntPtr handle, IntPtr bytes, uint numBytesToRead, ref uint numBytesRead, IntPtr overlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        static internal extern unsafe int ReadFile(IntPtr handle, IntPtr bytes, uint numBytesToRead, ref uint numBytesRead, ref OVERLAPPED overlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, [In] ref System.Threading.NativeOverlapped lpOverlapped);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool CloseHandle(IntPtr hObject);
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal delegate void ReadFileCompletionDelegate(int dwErrorCode, int dwNumBytesTransferred, ref OVERLAPPED lpOverlapped);

        [DllImport("kernel32.dll")]
        static extern bool ReadFileEx(IntPtr hFile, [Out] byte[] lpBuffer,
           uint nNumberOfBytesToRead, [In] ref System.Threading.NativeOverlapped lpOverlapped,
           ReadFileCompletionDelegate lpCompletionRoutine);

    }

}
