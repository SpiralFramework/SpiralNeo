namespace Info.SpiralFramework.Neo.Interfaces;

public static class Dr1OpCodes
{
    public static unsafe int ReadOpCodeArgument(int index = 0) =>
        *((byte*) *Dr1Addresses.LinData + *Dr1Addresses.LinDataIndex + index);

    public static unsafe int ReadOpCodeArgumentsAsInt16LE(int index = 0)
    {
        var linDataIndex = *Dr1Addresses.LinDataIndex;
        var linData = (byte*) *Dr1Addresses.LinData;

        return *(linData + linDataIndex + index) | (*(linData + linDataIndex + index + 1) << 8);
    }

    public static unsafe int ReadOpCodeArgumentsAsInt16BE(int index = 0)
    {
        var linDataIndex = *Dr1Addresses.LinDataIndex;
        var linData = (byte*) *Dr1Addresses.LinData;

        return *(linData + linDataIndex + index + 1) | (*(linData + linDataIndex + index) << 8);
    }

    public static unsafe void ConsumeOpCodeArguments(int count)
    {
        *Dr1Addresses.LinDataIndex += count;
    }

    public static unsafe void ClearTextBuffer()
    {
        
    }
}