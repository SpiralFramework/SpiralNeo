using Reloaded.Hooks.Definitions.X86;

namespace Info.SpiralFramework.Neo
{
    public static class HookDelegates
    {
        [Function(CallingConventions.Cdecl)]
        public unsafe delegate void TraceLog(char* log, int len);

        [Function(CallingConventions.Cdecl)]
        public unsafe delegate int GetFilePath(char* resultBuffer, string filename, int folder);

        [Function(CallingConventions.Cdecl)]
        public unsafe delegate int ReadFile(void* resultStructure, string path);
    }
}