using System;
using Reloaded.Hooks.Definitions.X86;

namespace Info.SpiralFramework.Neo.Interfaces
{
    public static class Dr1Delegates
    {
        [Function(CallingConventions.Cdecl)]
        public unsafe delegate int FUN_000c5d30(string param_1, int param_2, int param_3);

        [Function(CallingConventions.MicrosoftThiscall)]
        public unsafe delegate int FUN_001ae6d0(void* self, int param_1, int param_2, void* param_3, int param_4);

        [Function(CallingConventions.Cdecl)]
        public unsafe delegate void FUN_000516E0(uint param_1, uint param_2, int param_3);


        [Function(CallingConventions.Cdecl)]
        public unsafe delegate uint RandomMT();

        [Function(CallingConventions.Cdecl)]
        public delegate void SeedMT(int seed);


        [Function(CallingConventions.Cdecl)]
        public unsafe delegate void ParseLinFile(int* fileData);

        [Function(CallingConventions.Cdecl)]
        public unsafe delegate ushort* ParseLinString(ushort* str);

        [Function(CallingConventions.Cdecl)]
        public delegate uint OpCodeFunction();

        [Function(CallingConventions.Cdecl)]
        public delegate void LoadAndRunScript(int param1, uint scriptChapter, uint scriptScene, uint scriptVariant);
    }
}