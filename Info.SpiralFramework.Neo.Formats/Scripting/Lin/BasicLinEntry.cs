namespace Info.SpiralFramework.Neo.Formats.Scripting.Lin;

public class BasicLinEntry: LinEntry
{
    public BasicLinEntry(int opCode, int[] rawArguments)
    {
        this.OpCode = opCode;
        this.RawArguments = rawArguments;
    }
}