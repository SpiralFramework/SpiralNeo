namespace Info.SpiralFramework.Neo.Modules;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using CppSharp.Runtime;
using Extensions;
using Formats.Scripting.Lin;
using Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;

public unsafe class ScriptingModule : ISpiralModule
{
    private readonly ILogger _logger;
    private readonly VariadicHotfixer? _hotfixer;

    private readonly IHook<Dr1Delegates.FUN_000516E0> _fun000516E0Hook;
    private readonly IHook<Dr1Delegates.RandomMT> _randomMT;
    private readonly Dr1Delegates.SeedMT _seedMT;

    private readonly IReverseWrapper<Dr1Delegates.ParseLinFile> _parseLinFile;
    private readonly IAsmHook[] _parseLinFileHook;

    private readonly IReverseWrapper<Dr1Delegates.ParseLinString> _parseLinString;
    private readonly IAsmHook _parseLinStringHook;

    private readonly IHook<Dr1Delegates.LoadAndRunScript> _loadAndRunScriptHook;

    private readonly IReloadedHooks _hooks;

    private SafeUnmanagedMemoryHandle? _parsedLinHandle;

    private readonly OpCodeHook[] _opCodeFunctions = new OpCodeHook[256];
    private SafeUnmanagedMemoryHandle _opCodeFunctionsInMemory;
    private Dictionary<string, IDisposable> _linAllocations = new();

    private readonly OpCodeDebuggingModule _debuggingModule;

    public ScriptingModule(Program program, VariadicHotfixer? hotfixer)
    {
        this._logger = program.Logger;
        this._hotfixer = hotfixer;
        this._hooks = program.Hooks;
        this._debuggingModule = new OpCodeDebuggingModule(program.Logger, this);

        _fun000516E0Hook = program.Hooks
            .CreateHook<Dr1Delegates.FUN_000516E0>(Fun516E0Impl, (long) Dr1Addresses.FUN_000516E0)
            .Activate();

        _randomMT = program.Hooks
            .CreateHook<Dr1Delegates.RandomMT>(RandomMTImpl, (long) Dr1Addresses.RandomMT)
            .Activate();

        _seedMT = program.Hooks
            .CreateWrapper<Dr1Delegates.SeedMT>((long) Dr1Addresses.SeedMT, out _);

        _seedMT((int) DateTimeOffset.Now.ToUnixTimeSeconds());
        this._logger.WriteLine($"[SpiralNeo] Random: {RandomMTImpl()}");

        this._logger.WriteLine($"Loaded FUN_000516E0 @ {_fun000516E0Hook.ReverseWrapper.WrapperPointer:X}");

        this._parseLinFile = program.Hooks
            .CreateReverseWrapper<Dr1Delegates.ParseLinFile>(ParseLinFile);

        _parseLinFileHook = Dr1Addresses.ParseLinFiles
            .Select((details, index) => program.Hooks.CreateAsmHook(new[]
                {
                    "use32",
                    $"push {details.Register}",
                    program.Hooks.Utilities.GetAbsoluteCallMnemonics(this._parseLinFile.WrapperPointer, false),
                    "add esp, 0x8",
                    $"mov edx, [dword 0x{Dr1Addresses.ScriptData:X}]",
                },
                details.Pointer,
                AsmHookBehaviour.DoNotExecuteOriginal, details.Length).Activate()).ToArray();

        Array.Fill(_opCodeFunctions, OpCodeHook.Undefined.Instance);

        _loadAndRunScriptHook =
            program.Hooks.CreateHook<Dr1Delegates.LoadAndRunScript>(LoadAndRunScriptImpl,
                    (long) Dr1Addresses.LoadAndRunScript)
                .Activate();

        // this._parseLinString = program.Hooks
        //     .CreateReverseWrapper<Dr1Delegates.ParseLinString>(this.ParseLinString);
        //
        // var parseLinStringAsm = new[]
        // {
        //     "use32",
        //     "push eax",
        //     "push edx",
        //     program.Hooks.Utilities.GetAbsoluteCallMnemonics(this._parseLinString.WrapperPointer, false),
        //     "pop edx",
        //     "mov edx, eax",
        //     "pop eax",
        // };
        //
        // _parseLinStringHook = program.Hooks.CreateAsmHook(parseLinStringAsm,
        //         (long) Dr1Addresses.ParseLinString,
        //         AsmHookBehaviour.DoNotExecuteOriginal, 11)
        //     .Activate();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _parsedLinHandle?.Dispose();

        foreach (var (key, value) in _linAllocations) value.Dispose();
    }

    public void AddLinAllocation(string key, IDisposable value)
    {
        if (this._linAllocations.TryGetValue(key, out var existing)) existing.Dispose();

        this._linAllocations[key] = value;
    }

    public void OnModLoaderInitialised()
    {
        _logger.WriteLine($"Loading opcode functions...", _logger.ColorGreen);

        for (var i = 0; i <= 0x3C; i++)
        {
            var functionPtr = *(Dr1Addresses.OpcodeArray + i);
            if (functionPtr == nint.Zero)
            {
                _opCodeFunctions[i] = OpCodeHook.Undefined.Instance;
            }
            else
            {
                _opCodeFunctions[i] = new OpCodeHook.BaseWrapper(functionPtr);
            }
        }

        _logger.WriteLine("Allocating opcode array memory...", _logger.ColorGreen);
        var opCodeFunctionsInMemory = Marshal.AllocHGlobal(_opCodeFunctions.Length * 4);
        _opCodeFunctionsInMemory = new SafeUnmanagedMemoryHandle(opCodeFunctionsInMemory, true);

        _logger.WriteLine("Loading opcode functions into array...", _logger.ColorGreen);
        LoadOpCodeFunctionsIntoArray();

        _logger.WriteLine($"NOPing out opcode comparison...", _logger.ColorGreen);
        // NOP out the comparison for the opcode
        for (var i = 0; i < 6; i++)
            Memory.CurrentProcess.SafeWrite(Dr1Addresses.OpcodeCompare + i, (byte) 0x90);

        _logger.WriteLine("Overwriting opcode array address...", _logger.ColorGreen);
        // Overwrite the function address
        Memory.CurrentProcess.SafeWrite(Dr1Addresses.OpcodeGetFunction, opCodeFunctionsInMemory);

        _debuggingModule.Enable();
    }

    public void ResetOpCodeFunctions()
    {
        Array.Fill(_opCodeFunctions, OpCodeHook.Undefined.Instance);
        for (var i = 0; i <= 0x3C; i++)
        {
            var functionPtr = *(Dr1Addresses.OpcodeArray + i);
            if (functionPtr == nint.Zero)
            {
                _opCodeFunctions[i] = OpCodeHook.Undefined.Instance;
            }
            else
            {
                _opCodeFunctions[i] = new OpCodeHook.BaseWrapper(functionPtr);
            }
        }

        LoadOpCodeFunctionsIntoArray();
    }

    private void LoadOpCodeFunctionsIntoArray()
    {
        var mustRelease = false;

        try
        {
            _opCodeFunctionsInMemory.DangerousAddRef(ref mustRelease);
            var handle = (nint*) _opCodeFunctionsInMemory.DangerousGetHandle();
            for (var i = 0; i < _opCodeFunctions.Length; i++)
                *(handle + i) = _opCodeFunctions[i].ToPtr();
        }
        finally
        {
            if (mustRelease)
            {
                _opCodeFunctionsInMemory.DangerousRelease();
            }
        }
    }

    public bool HookExistingOpCode(byte opcode, Dr1Delegates.OpCodeFunction opcodeFunc,
        [MaybeNullWhen(false)] out IHook<Dr1Delegates.OpCodeFunction> hook)
    {
        var existing = _opCodeFunctions[opcode];
        if (existing is not OpCodeHook.BaseWrapper)
        {
            hook = null;
            return false;
        }

        hook = _hooks.CreateHook(opcodeFunc, existing.ToPtr());
        hook.Enable();

        _opCodeFunctions[opcode] = new OpCodeHook.BaseHooked(hook);
        LoadOpCodeFunctionsIntoArray();

        return true;
    }

    public bool AddNewOpCode(byte opcode, Dr1Delegates.OpCodeFunction opcodeFunc,
        [MaybeNullWhen(false)] out IReverseWrapper<Dr1Delegates.OpCodeFunction> hook)
    {
        if (_opCodeFunctions[opcode] is not OpCodeHook.Undefined)
        {
            hook = null;
            return false;
        }

        hook = _hooks.CreateReverseWrapper(opcodeFunc);
        _opCodeFunctions[opcode] = new OpCodeHook.BaseReverseWrapper(hook);
        LoadOpCodeFunctionsIntoArray();

        return true;
    }

    private unsafe void Fun516E0Impl(uint param_1, uint param_2, int param_3)
    {
        _logger.WriteLine($"FUN_516E0: {param_1}, {param_2}, {param_3}");
        _fun000516E0Hook.OriginalFunction(param_1, param_2, param_3);
    }

    private unsafe uint RandomMTImpl()
    {
        var ret = _randomMT.OriginalFunction();
        // _logger.WriteLine($"RandomMT: {ret}");
        return ret;
    }

    public void LoadScript(int arg1, int arg2, int arg3)
    {
        this._logger.WriteLine($"[SpiralNeo:LoadScript] Loading e{arg1:00}_{arg2:000}_{arg3:000}",
            this._logger.ColorPink);

        var customLin = new CustomLinScript();
        customLin.AddEntry(new BasicLinEntry(0x00, new[] { 0, 0 }));
        customLin.AddEntry(new BasicLinEntry(0x19, new[] { arg1, arg2, arg3 }));
        customLin.AddEntry(new BasicLinEntry(0x1A, Array.Empty<int>()));

        var customLinData = customLin.Compile();
        var customLinPtr = Marshal.AllocHGlobal(customLinData.Length);
        Marshal.Copy(customLinData, 0, customLinPtr, customLinData.Length);

        *Dr1Addresses.LinDataIndex = 0;

        ParseLinFile((int*) customLinPtr);

        _parsedLinHandle = new SafeUnmanagedMemoryHandle(customLinPtr, true);
    }

    private void ParseLinFile(int* fileData)
    {
        _parsedLinHandle?.Dispose();

        // We're going to be nasty gremlins and create our OWN lin

        if (_parsedLinHandle == null)
        {
            this._logger.WriteLine("[SpiralNeo:ParseLinFile] Rewriting LIN", this._logger.ColorPink);

            var customLin = new CustomLinScript();
            customLin.AddEntry(new BasicLinEntry(0x00, new[] { 0, 0 }));
            customLin.AddEntry(new BasicLinEntry(0xFF, new[] { 6, 1, 2, 3, 4, 5, 6 }));
            customLin.AddEntry(new BasicLinEntry(0xFF, new[] { 2, 9, 8 }));
            customLin.AddEntry(new BasicLinEntry(0x19, new[] { 1, 100, 0 }));
            customLin.AddEntry(new BasicLinEntry(0x1A, Array.Empty<int>()));

            var customLinData = customLin.Compile();
            var customLinPtr = Marshal.AllocHGlobal(customLinData.Length);
            Marshal.Copy(customLinData, 0, customLinPtr, customLinData.Length);

            fileData = (int*) customLinPtr;
            _parsedLinHandle = new SafeUnmanagedMemoryHandle(customLinPtr, true);
        }

        var linType = *fileData;
        if (linType == 0x2E4C494E)
        {
            this._logger.WriteLine("[SpiralNeo:ParseLinFile] Detected magic number, skipping!", this._logger.ColorPink);
            fileData += 1;
            linType = *fileData;
        }

        this._logger.WriteLine($"[SpiralNeo:ParseLinFile] Reading @ 0x{(long) fileData:X} (Type: {linType})");

        *Dr1Addresses.LinData = (int) fileData + *(fileData + 1);
        if (linType == 2)
        {
            var linTextOffset = (int) fileData + *(fileData + 2);
            *Dr1Addresses.LinText = linTextOffset;

            var textCount = *((short*) linTextOffset);
            *Dr1Addresses.LinTextCount = textCount;

            this._logger.WriteLine($"Loading {textCount} lines of text");

            if (textCount != 0)
            {
                var iVar7 = 0;

                do
                {
                    var offset =
                        linTextOffset + 2 + *((int*) (linTextOffset + 4 + iVar7 * 4));
                    this._logger.WriteLine($"[SpiralNeo:Scripting] String {iVar7}: {offset}");
                    *(Dr1Addresses.LinTextStrings + iVar7) = offset;

                    iVar7++;
                } while (iVar7 < textCount);
            }
        }
        else
        {
            *Dr1Addresses.LinText = 0;
        }

        //FUN_516E0: 246, 32, 0
        // Fun516E0Impl((uint) (ushort) (*Dr1Addresses.LinDataIndex), 0x20, 0);
        // *Dr1Addresses.DAT_0033c6b0_89DD = 0;
    }

    private void LoadAndRunScriptImpl(int param1, uint scriptChapter, uint scriptScene, uint scriptVariant)
    {
        _logger.WriteLine(
            $"[SpiralNeo] Loading script with mode {param1}: e{scriptChapter:00}_{scriptScene:000}_{scriptVariant:000}",
            _logger.ColorGreen);
        _loadAndRunScriptHook.OriginalFunction(param1, scriptChapter, scriptScene, scriptVariant);
    }
}