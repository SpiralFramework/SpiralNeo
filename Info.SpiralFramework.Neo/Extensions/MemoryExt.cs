namespace Info.SpiralFramework.Neo.Extensions;

using System.Runtime.InteropServices;
using System.Text;

public static class MemoryExt
{
    public static unsafe int GetUnicodeStringLength(nint ptr)
    {
        var length = 0;
        while (*(((short*) ptr) + length) != 0) length++;
        return length * 2;
    }

    public static unsafe void ClearUnicodeStringPointer(nint ptr)
    {
        *((short*) ptr) = 0x00;
    }
    
    public static unsafe nint AllocUnicodeString(string str)
    {
        var newTextEncoded = Encoding.Unicode.GetBytes(str);
        var newTextPtr = Marshal.AllocHGlobal(newTextEncoded.Length + 2);
        Marshal.Copy(newTextEncoded, 0, newTextPtr, newTextEncoded.Length);
        *((byte*) newTextPtr + newTextEncoded.Length) = 0x00;
        *((byte*) newTextPtr + newTextEncoded.Length + 1) = 0x00;

        return newTextPtr;
    }
}