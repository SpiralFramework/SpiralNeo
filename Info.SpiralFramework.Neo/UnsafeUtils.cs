using System;
using System.Text;

namespace Info.SpiralFramework.Neo
{
    public static class UnsafeUtils
    {
        public static unsafe void WriteAsciiString(char* ptr, string value, bool nullTerminated = true)
        {
            var encoded = Encoding.ASCII.GetBytes(value);
            new Span<byte>(encoded, 0, encoded.Length)
                .CopyTo(new Span<byte>(ptr, encoded.Length));

            if (nullTerminated) *((byte*) ptr + encoded.Length) = 0x00;
        }
    }
}