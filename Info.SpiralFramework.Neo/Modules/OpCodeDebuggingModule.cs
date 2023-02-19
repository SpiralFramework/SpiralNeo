namespace Info.SpiralFramework.Neo.Modules;

using System;
using System.Runtime.InteropServices;
using System.Text;
using CppSharp.Runtime;
using Extensions;
using Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

public class OpCodeDebuggingModule
{
    private ILogger _logger;
    private ScriptingModule _scriptingModule;

    private IHook<Dr1Delegates.OpCodeFunction>? _textHook;
    private IReverseWrapper<Dr1Delegates.OpCodeFunction>? _debugOpHook;


    public OpCodeDebuggingModule(ILogger logger, ScriptingModule scriptingModule)
    {
        this._logger = logger;
        this._scriptingModule = scriptingModule;
    }

    public void Enable()
    {
        _scriptingModule.HookExistingOpCode(0x02, TextHook, out _textHook);
        _scriptingModule.AddNewOpCode(0xFF, DebugOp, out _debugOpHook);
    }

    public void Disable()
    {
        _textHook?.Disable();
    }

    public unsafe uint TextHook()
    {
        var textID = Dr1OpCodes.ReadOpCodeArgumentsAsInt16BE();
        var stringPtrOffset = Dr1Addresses.LinTextStrings + textID;
        var stringPtr = *stringPtrOffset;
        var length = 0;
        while (*(((short*) stringPtr) + length) != 0) length++;

        var text = Encoding.Unicode.GetString((byte*) stringPtr, length * 2);
        var newText = $"Current Time: {DateTime.Now}";
        _logger.WriteLine($"[SpiralNeo:TextHook] Displaying text with ID {textID}: \"{text}\"");
        _logger.WriteLine(
            $"[SpiralNeo:TextHook] Replacing @ {(nint) stringPtrOffset:X} with \"{newText}\"");

        var replacementBytes = new byte[Encoding.Unicode.GetByteCount(newText) + 2];
        var bytesReceived = Encoding.Unicode.GetBytes(newText, 0, newText.Length, replacementBytes, 0);
        replacementBytes[bytesReceived + 0] = 0x00;
        replacementBytes[bytesReceived + 1] = 0x00;

        uint ret;

        fixed (byte* replacementPtr = &replacementBytes[0])
        {
            try
            {
                *stringPtrOffset = (int) replacementPtr;
                ret = _textHook?.OriginalFunction?.Invoke() ?? 1;
            }
            finally
            {
                *stringPtrOffset = stringPtr;
            }
        }

        return ret;
    }

    public unsafe uint DebugOp()
    {
        var argCount = Dr1OpCodes.ReadOpCodeArgument();
        this._logger.WriteLine($"[SpiralNeo:DebugOp] Hello, World! Reading {argCount} numbers...");

        for (var i = 0; i < argCount; i++)
            this._logger.WriteLine($"[SpiralNeo:DebugOp] Hello, {Dr1OpCodes.ReadOpCodeArgument(i + 1)}");

        Dr1OpCodes.ConsumeOpCodeArguments(argCount + 1);
        return 1;
    }

    private uint NoOpCode()
    {
        return 1;
    }
}