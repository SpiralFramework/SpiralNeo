using System.Diagnostics;
using System.IO;

namespace Info.SpiralFramework.Neo.Interfaces
{
    public static class Dr1Paths
    {
        public static readonly string GamePath =
            Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? "") ?? "";

        public const string SplashScreen = "DrCommon/data/all/cg/aglogo.tga";
    }
}