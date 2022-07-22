using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpiralNeo
{
    public static class Dr1Addresses
    {
        private static IntPtr _baseAddress = IntPtr.Zero;
        private static int _lastWin32Error = 0;

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetModuleHandleEx(uint dwFlags, string? lpModuleName, out IntPtr phModule);

        static Dr1Addresses()
        {
            var baseAddress = Process.GetCurrentProcess()?.MainModule?.BaseAddress;
            if (baseAddress == null)
            {
                if (GetModuleHandleEx(0x02, null, out _baseAddress)) return;

                _lastWin32Error = Marshal.GetLastWin32Error();
                return;
            }

            _baseAddress = baseAddress.Value;

            // _baseAddress += 0x1000;
        }

        public const int LogTraceMessagesOffset = 0x100B00;
        public const int GetFilePathOffset = 0xC56A0;
        public const int GetFilePathAsmFirstOffset = 0xC58F2;
        public const int GetFilePathAsmSecondOffset = 0xC5B3B;
        public const int ReadFileOffset = 0x1637e0;

        public const int ArchiveRootOffset = 0x3405C8;
        public const int SpeakerOffset = 0x33EA44;

        public const int GameStateDataOffset = 0x2E47A8;

        public static IntPtr LogTraceMessages => _baseAddress + LogTraceMessagesOffset;
        public static IntPtr GetFilePath => _baseAddress + GetFilePathOffset;
        public static IntPtr GetFilePathAsmFirst => _baseAddress + GetFilePathAsmFirstOffset;
        public static IntPtr GetFilePathAsmSecond => _baseAddress + GetFilePathAsmSecondOffset;
        public static IntPtr ReadFile => _baseAddress + ReadFileOffset;

        public static IntPtr ArchiveRoot => _baseAddress + ArchiveRootOffset;
        public static IntPtr Speaker => _baseAddress + SpeakerOffset;

        public static IntPtr GameStateData = _baseAddress + GameStateDataOffset;

        public static IntPtr uVar6 => _baseAddress + 0x0033c6b0 + 0x38;
        public static IntPtr bVar1 => _baseAddress + 0x002e4678;
    }
}