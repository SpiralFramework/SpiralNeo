using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Mod.Interfaces;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace SpiralNeo;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod
{
    // private IHook<TraceLog> _traceLogHook;
    private IAsmHook _traceLogAsmHook;
    private IReverseWrapper<TraceLog> _traceLogReverseWrapper;

    public Mod(IReloadedHooks hooks, ILogger logger)
    {
        /*Console.WriteLine("Hello, World!");

        IntPtr Dr1BaseAddress = PInvoke.GetModuleHandle(loaderApi.);*/
        var loadedFrom = Assembly.GetExecutingAssembly().Location;
        Console.WriteLine($"Loaded @ {loadedFrom}");
        IntPtr dr1BaseAddress;
        Console.WriteLine(PInvoke.GetModuleHandleEx(0x02, null, out dr1BaseAddress));

        var hModule = PInvoke.LoadLibrary(Path.Combine(Path.GetDirectoryName(loadedFrom), "VariadicHotfix.dll"));
        if (hModule == IntPtr.Zero)
        {
            Console.WriteLine($"Failed to load VariadicHotfix, error: {Marshal.GetLastWin32Error()}");
            return;
        }

        IntPtr sprintfPointer = PInvoke.GetProcAddress(hModule, "sprintf_to");

        Console.WriteLine($"sprintf_to @ {sprintfPointer:X}");

        var traceAddress = dr1BaseAddress + 0x1000 + 0xFFB00;
        Console.WriteLine($"Base Address: {dr1BaseAddress}");
        Console.WriteLine($"Trace @ {traceAddress}");
        // _traceLogHook = hooks.CreateHook<TraceLog>(TraceLogImpl, (long) traceAddress).Activate();

        _traceLogReverseWrapper = hooks.CreateReverseWrapper<TraceLog>(TraceLogImpl);

        Console.WriteLine($"Reverse Wrapper @ {_traceLogReverseWrapper}");

        string[] traceLogAsm =
        {
            $"use32",

            $"push dword 1000",
            $"push dword {_traceLogReverseWrapper.WrapperPointer.ToInt32()}",

            $"{hooks.Utilities.GetAbsoluteCallMnemonics(sprintfPointer, false)}",

            $"add esp,8",

            $"ret"
        };

        _traceLogAsmHook = hooks.CreateAsmHook(traceLogAsm, (long) traceAddress, AsmHookBehaviour.DoNotExecuteOriginal)
            .Activate();

        Console.WriteLine("Press any key to finish loading...");
        Console.ReadLine();

        // Console.WriteLine($"Hooked @ {_traceLogHook.ReverseWrapper.NativeFunctionPtr}");
    }

    [Function(CallingConventions.Cdecl)]
    public delegate void TraceLog(IntPtr log, int len);

    private static void TraceLogImpl(IntPtr log, int len)
    {
        Console.WriteLine($"[AGConsole] {Marshal.PtrToStringAnsi(log, len)}");
    }
}