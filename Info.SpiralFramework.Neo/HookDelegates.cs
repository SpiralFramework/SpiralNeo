using System;
using Reloaded.Hooks.Definitions.X86;

namespace SpiralNeo
{
    public static class HookDelegates
    {
        [Function(CallingConventions.Cdecl)]
        public delegate void TraceLog(IntPtr log, int len);

        [Function(CallingConventions.Cdecl)]
        public delegate int GetFilePath(IntPtr resultBuffer, string filename, int folder);

        [Function(CallingConventions.Cdecl)]
        public delegate int ReadFile(IntPtr[] resultStructure, string path);
    }
}