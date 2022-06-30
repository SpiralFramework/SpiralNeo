using System;
using System.Runtime.InteropServices;

namespace SpiralNeo
{
    public static class Dr1Addresses
    {
        private static IntPtr _baseAddress = IntPtr.Zero;
        private static int _lastWin32Error = 0;

        static Dr1Addresses()
        {
            if (!PInvoke.GetModuleHandleEx(0x02, null, out _baseAddress))
            {
                _lastWin32Error = Marshal.GetLastWin32Error();
                return;
            }

            _baseAddress += 0x1000;
        }

        public const int LogTraceMessagesOffset = 0xFFB00;

        public static IntPtr LogTraceMessages => _baseAddress + LogTraceMessagesOffset;
    }
}