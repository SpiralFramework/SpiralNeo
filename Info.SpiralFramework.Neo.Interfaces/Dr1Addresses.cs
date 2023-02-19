using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Info.SpiralFramework.Neo.Interfaces
{
    using System.Runtime.CompilerServices;

    public record Dr1ParseLinDetails(nint Pointer, string Register, int Length, string? Cleanup = null);

    public static class Dr1Addresses
    {
        private static readonly nint BaseAddress;
        internal static int LastWin32Error;

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetModuleHandleEx(uint dwFlags, string? lpModuleName, out IntPtr phModule);

        internal static unsafe void* GetPtr(nint offset) => (BaseAddress + offset).ToPointer();
        internal static unsafe int* GetInt32Ptr(nint ptr) => (int*) (BaseAddress + ptr).ToPointer();

        internal static unsafe int* GetInt32Ptr(nint ptr, uint offset) =>
            (int*) (Unsafe.Read<int>((BaseAddress + ptr).ToPointer()) + offset);

        internal static unsafe short* GetInt16Ptr(nint ptr) => (short*) (BaseAddress + ptr).ToPointer();

        internal static unsafe short* GetInt16Ptr(nint ptr, uint offset) =>
            (short*) (Unsafe.Read<int>((BaseAddress + ptr).ToPointer()) + offset);


        internal static unsafe byte* GetBytePtr(nint ptr) => (byte*) (BaseAddress + ptr).ToPointer();

        internal static unsafe byte* GetBytePtr(nint ptr, uint offset) =>
            (byte*) (Unsafe.Read<int>((BaseAddress + ptr).ToPointer()) + offset);

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

        public const nint LogTraceMessagesOffset = 0x100B00;
        public const nint GetFilePathOffset = 0xC56A0;
        public const nint GetFilePathAsmFirstOffset = 0xC58F2;
        public const nint GetFilePathAsmSecondOffset = 0xC5B3B;
        public const nint ReadFileOffset = 0x1637e0;

        public const nint ArchiveRootOffset = 0x3405C8;
        public const nint SpeakerOffset = 0x33EA44;

        public const nint GameStateDataOffset = 0x2E47A8;

        public const nint SDLWaitEventOffset = 0x118728;

        public static unsafe void* LogTraceMessages => GetPtr(LogTraceMessagesOffset);
        public static unsafe void* GetFilePath => GetPtr(GetFilePathOffset);
        public static unsafe void* GetFilePathAsmFirst => GetPtr(GetFilePathAsmFirstOffset);
        public static unsafe void* GetFilePathAsmSecond => GetPtr(GetFilePathAsmSecondOffset);
        public static unsafe void* ReadFile => GetPtr(ReadFileOffset);

        public static unsafe void* FUN_000c5d30 => GetPtr(0x000c5d30);
        public static unsafe void* FUN_001ae6d0 => GetPtr(0x001ae6d0);

        public static unsafe void* FUN_000516E0 => GetPtr(0x000516e0);

        public static unsafe void* SDLWaitEvent => GetPtr(SDLWaitEventOffset);

        public static unsafe void* RandomMT => GetPtr(0xbc7f0);
        public static unsafe void* SeedMT => GetPtr(0xbc990);

        public static unsafe void* LoadAndRunScript => GetPtr(0x4bf30);

        public static unsafe Dr1ParseLinDetails[] ParseLinFiles => new Dr1ParseLinDetails[]
        {
            new((nint) GetPtr(0x49009), "edx", 125),
            new((nint) GetPtr(0x490d8), "edx", 126),
            new((nint) GetPtr(0x4c13d), "edx", 120),
            new((nint) GetPtr(0x4c37d), "edx", 128),
            new((nint) GetPtr(0x4c6cd), "edx", 120),
            new((nint) GetPtr(0x51dc2), "edx", 165),
            new((nint) GetPtr(0x51ec8), "edx", 122),
            new((nint) GetPtr(0x51f60), "edx", 114),
        };

        public static unsafe void* ParseLinString => GetPtr(0xb8a74);

        public static unsafe int* ArchiveRoot => GetInt32Ptr(ArchiveRootOffset);
        public static unsafe int* Speaker => GetInt32Ptr(SpeakerOffset);

        public static unsafe nint ScriptData => (nint) GetPtr(0x33c6b0);
        public static unsafe int* LinData => GetInt32Ptr(0x33c6b0, 0x38);
        public static unsafe int* LinText => GetInt32Ptr(0x33c6b0, 0x3C);
        public static unsafe int* LinTextStrings => GetInt32Ptr(0x33C6B0, 0x40);
        public static unsafe short* LinTextCount => GetInt16Ptr(0x33c6b0, 0xFE0);

        public static unsafe short* ScriptChapter = GetInt16Ptr(0x2e4666);
        public static unsafe short* ScriptScene = GetInt16Ptr(0x2e4668);
        public static unsafe short* ScriptVariant = GetInt16Ptr(0x2e466A);

        public static unsafe int* LinDataIndex => GetInt32Ptr(0x002e4678);

        public static unsafe short* GameStateData => GetInt16Ptr(GameStateDataOffset);

        public static unsafe nint OpcodeCompare => (nint) GetPtr(0x4e16f);
        public static unsafe nint OpcodeGetFunction => (nint) GetPtr(0x4e17B);
        public static unsafe int* OpcodeArray => GetInt32Ptr(0x2953a8);

        public static unsafe int* DAT_0033c6b0_89DD => GetInt32Ptr(0x0033c6b0, 0x89dd);

        public static unsafe nint TextBuffer => (nint) GetPtr(0x2FD2B0);
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