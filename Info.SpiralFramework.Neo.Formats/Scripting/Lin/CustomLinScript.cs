namespace Info.SpiralFramework.Neo.Formats.Scripting.Lin;

using System.Collections.Generic;
using System.IO;

public class CustomLinScript
{
    protected List<string> TextPayloads = new();
    protected List<LinEntry> ScriptPayloads = new();

    public bool WriteMagicNumber = true;
    public bool IncludeTextByteOrderMarker = true;

    public int AddText(string text)
    {
        var index = TextPayloads.IndexOf(text);
        if (index >= 0) return index;

        TextPayloads.Add(text);
        index = TextPayloads.IndexOf(text);

        return index;
    }

    public void AddEntry(LinEntry entry)
    {
        ScriptPayloads.Add(entry);
    }

    public byte[] Compile()
    {
        using var stream = new MemoryStream();
        Compile(stream);
        return stream.ToArray();
    }

    public void Compile(Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        if (WriteMagicNumber) writer.Write(0x2E4C494E);

        // Pre-Compile
        ScriptPayloads.ForEach(payload => payload.PreCompile(this));

        using var scriptDataStream = new MemoryStream();
        using var scriptDataWriter = new BinaryWriter(scriptDataStream);

        // Compile
        ScriptPayloads.ForEach(payload => payload.Compile(this, scriptDataWriter));


        if (TextPayloads.Count == 0)
        {
            var scriptData = scriptDataStream.ToArray();
            
            writer.Write((int) 1);
            writer.Write((int) 12);
            writer.Write((int) 12 + scriptData.Length);
            
            writer.Write(scriptData);
        }
        else
        {
        }

        ScriptPayloads.ForEach(payload => payload.PostCompile(this));
    }
}