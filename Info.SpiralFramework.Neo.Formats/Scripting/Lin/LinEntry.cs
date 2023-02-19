namespace Info.SpiralFramework.Neo.Formats.Scripting.Lin;

using System.IO;

public abstract class LinEntry
{
    public int OpCode;
    public int[] RawArguments;

    public virtual void PreCompile(CustomLinScript script)
    {
    }

    public virtual void Compile(CustomLinScript script, BinaryWriter writer)
    {
        writer.Write((byte) 0x70);
        writer.Write((byte) OpCode);

        foreach (var arg in RawArguments)
            writer.Write((byte) arg);
    }

    public virtual void PostCompile(CustomLinScript script)
    {
    }
}