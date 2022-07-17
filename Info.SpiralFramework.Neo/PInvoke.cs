using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpiralNeo
{
    public static class PInvoke
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] in string? lpModuleName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetModuleHandleEx(UInt32 dwFlags, string? lpModuleName, out IntPtr phModule);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        // When calling with any variable parameters and Ansi
        [DllImport("msvcrt.dll", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sprintf(
            StringBuilder buffer,
            string format,
            __arglist);

        // When calling with any variable parameters and Ansi
        [DllImport("msvcrt.dll", EntryPoint = "sprintf", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sprintfWithPtr(
            StringBuilder buffer,
            string format,
            IntPtr args);

        // When calling with any variable parameters and Ansi
        [DllImport("msvcrt.dll", EntryPoint = "sprintf", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sprintfWithIter(
            StringBuilder buffer,
            string format,
            ArgIterator args);

        [DllImport("kernel32.dll")]
        internal static extern bool CreateSymbolicLink(
            string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool CreateHardLink(
            string lpFileName,
            string lpExistingFileName,
            IntPtr lpSecurityAttributes
        );

        internal enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }
    }
}