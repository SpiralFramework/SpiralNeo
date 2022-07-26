using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Info.SpiralFramework.Neo.Interfaces
{
    public static class Dr1Addresses
    {
        internal static IntPtr BaseAddress;
        internal static int LastWin32Error;

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetModuleHandleEx(uint dwFlags, string? lpModuleName, out IntPtr phModule);

        internal static unsafe void* GetVoidPtr(int offset) => (BaseAddress + offset).ToPointer();
        internal static unsafe int* GetInt32Ptr(int offset) => (int*) (BaseAddress + offset).ToPointer();
        internal static unsafe short* GetInt16Ptr(int offset) => (short*) (BaseAddress + offset).ToPointer();

        static Dr1Addresses()
        {
            var baseAddress = Process.GetCurrentProcess()?.MainModule?.BaseAddress;
            if (baseAddress == null)
            {
                if (GetModuleHandleEx(0x02, null, out BaseAddress)) return;

                LastWin32Error = Marshal.GetLastWin32Error();
                return;
            }

            BaseAddress = baseAddress.Value;

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

        public static unsafe void* LogTraceMessages => GetVoidPtr(LogTraceMessagesOffset);
        public static unsafe void* GetFilePath => GetVoidPtr(GetFilePathOffset);
        public static unsafe void* GetFilePathAsmFirst => GetVoidPtr(GetFilePathAsmFirstOffset);
        public static unsafe void* GetFilePathAsmSecond => GetVoidPtr(GetFilePathAsmSecondOffset);
        public static unsafe void* ReadFile => GetVoidPtr(ReadFileOffset);

        public static unsafe void* FUN_000c5d30 => GetVoidPtr(0x000c5d30);
        public static unsafe void* FUN_001ae6d0 => GetVoidPtr(0x001ae6d0);

        public static unsafe int* ArchiveRoot => GetInt32Ptr(ArchiveRootOffset);
        public static unsafe int* Speaker => GetInt32Ptr(SpeakerOffset);

        public static unsafe short* GameStateData = GetInt16Ptr(GameStateDataOffset);

        public static unsafe int* uVar6 => GetInt32Ptr(0x0033c6b0 + 0x38);
        public static unsafe int* bVar1 => GetInt32Ptr(0x002e4678);
    }

    public static class Dr1ArtefactAddresses
    {
        public const int CopyPrefixedNetstringOffset = 0x000fd530;
    }

    public static class Dr1AgPCStorageStream
    {
        public const int FUN_001ae860_Offset = 0x001ae860;
        public const int FUN_00101d10_Offset = 0x00101d10;
        public const int FUN_001aec80_Offset = 0x001aec80;
        public const int FUN_001aee40_Offset = 0x001aee40;
        public const int FUN_001aeb70_Offset = 0x001aeb70;
        public const int FUN_001aed60_Offset = 0x001aed60;
        public const int FUN_001aea90_Offset = 0x001aea90;
        public const int FUN_00101d50_Offset = 0x00101d50;
        public const int FUN_0015c270_Offset = 0x0015c270;
        public const int FUN_00101c40_Offset = 0x00101c40;
        public const int FUN_001ae8d0_Offset = 0x001ae8d0;
        public const int FUN_001ae9b0_Offset = 0x001ae9b0;

        public static readonly int[] VFTableOffsets =
        {
            FUN_001ae860_Offset, FUN_00101d10_Offset, FUN_001aec80_Offset, FUN_001aee40_Offset, FUN_001aeb70_Offset,
            FUN_001aed60_Offset, FUN_001aea90_Offset, FUN_00101d50_Offset, FUN_0015c270_Offset, FUN_0015c270_Offset,
            FUN_0015c270_Offset, FUN_00101c40_Offset, FUN_001ae8d0_Offset, FUN_001ae9b0_Offset
        };
    }
}