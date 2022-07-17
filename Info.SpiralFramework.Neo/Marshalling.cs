using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpiralNeo
{
    public static class Marshalling
    {
        public static void WriteAsciiString(IntPtr ptr, string value, bool nullTerminated = true)
        {
            var encoded = Encoding.ASCII.GetBytes(value);
            Marshal.Copy(encoded, 0, ptr, encoded.Length);
            if (nullTerminated) Marshal.WriteByte(ptr, encoded.Length, 0x00);
        }
    }
}