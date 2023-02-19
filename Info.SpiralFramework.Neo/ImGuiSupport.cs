using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DearImguiSharp;
using Info.SpiralFramework.Neo.Extensions;
using Info.SpiralFramework.Neo.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X86;
using Reloaded.Imgui.Hook;
using Reloaded.Imgui.Hook.Direct3D11;
using Reloaded.Imgui.Hook.Implementations;
using Reloaded.Imgui.Hook.Misc;

namespace Info.SpiralFramework.Neo;

public class ImGuiSupport : IDisposable
{
    private Program _program;
    private IAsmHook _sdlWaitEvent;
    private IReverseWrapper<ConsumeSDLEvent> _consumeSDLEvent;
    private static WndProcHook.WndProc? _wndProc;
    private static IHook<WndProcHook.WndProc>? _wndProcHook;
    private static IntPtr? _windowHandle;

    [Function(CallingConventions.Cdecl)]
    private delegate int ConsumeSDLEvent(IntPtr eventStruct);

    private unsafe ImGuiSupport(Program program)
    {
        this._program = program;

        try
        {
            _consumeSDLEvent = program.Hooks
                .CreateReverseWrapper<ConsumeSDLEvent>(ShouldConsumeSDLEvent);

            var sdlWaitEventAsmHook = new[]
            {
                "use32",
                "lea eax, [ebp-0x3c]",
                "push eax",
                program.Hooks.Utilities.GetAbsoluteCallMnemonics(_consumeSDLEvent.WrapperPointer, false),
                "test eax,eax",
                // "jz .sdl_no_return",
                "pop esi",
                "mov ecx, [ebp-0x4]",
                "xor ecx,ebp",
                // program.Hooks.Utilities.GetAbsoluteCallMnemonics(new IntPtr(0x000ae7b3), false),
                "mov esp,ebp",
                "pop ebp",
                "ret",
                ".sdl_no_return:",
                // "mov eax,DWORD PTR [ebp-0x3C]",
                // "cmp eax,0x400",
            };

            this._sdlWaitEvent = program.Hooks.CreateAsmHook(sdlWaitEventAsmHook, (long) Dr1Addresses.SDLWaitEvent,
                    AsmHookBehaviour.ExecuteFirst, 8)
                .Activate();
        }
        catch
            (Exception e)
        {
            _program.Logger.WriteLine($"Exception: {e}");
            throw;
        }
    }

    private int ShouldConsumeSDLEvent(IntPtr eventStruct)
    {
        _program.Logger.WriteLine($"Hello, World! @ {eventStruct:X}");
        return 0;
    }

    public static async Task<ImGuiSupport> Create(Program program)
    {
        SDK.Init(program.Hooks, s => program.Logger.WriteLine(s));

        var support = new ImGuiSupport(program);

        await ImguiHook.Create(support.Render, new ImguiHookOptions()
        {
            Implementations = new List<IImguiHook>()
            {
                new ImguiHookDx9(),
                new ImguiHookDx11()
            },
            // EnableViewports = true, // Enable docking.
            IgnoreWindowUnactivate = true // May help if game pauses when it loses focus.
        }).ConfigureAwait(false);


        return support;
    }

    public void Render()
    {
        if (_wndProc == null)
        {
            unsafe
            {
                Console.WriteLine("Hello, World!");

                IntPtr functionPointer =
                    (IntPtr) _program.Hooks.Utilities.GetFunctionPointer(typeof(ImGuiSupport), "WndProcHandler");

                _windowHandle = ImguiHook.WindowHandle;
                IntPtr windowLong = Native.GetWindowLong(ImguiHook.WindowHandle, Native.GWL.GWL_WNDPROC);
                Debug.WriteLine(string.Format("[WndProcHook] WindowProc: {0:X}", (object) (long) windowLong));

                _wndProc = Unsafe.As<IntPtr, WndProcHook.WndProc>(ref functionPointer);
                _wndProcHook = _program.Hooks.CreateHook(_wndProc.Value, (long) windowLong).Activate();
            }
        }

        ImguiHook.IO.MouseDrawCursor = ImguiHook.IO.WantCaptureMouse;
        if (!ImguiHook.IO.MouseDrawCursor) ImGui.SetMouseCursor((int) ImGuiMouseCursor.ImGuiMouseCursorNone);
        // _program.Logger.WriteLine($"Mouse Draw Cursor: {ImguiHook.IO.MouseDrawCursor}");

        foreach (var module in _program.Modules) module.Render();
    }

    [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvStdcall) })]
    private static unsafe IntPtr WndProcHandler(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam)
    {
        var result = ImGui.ImplWin32_WndProcHandler((void*) hWnd, msg, wParam, lParam);
        if (result != IntPtr.Zero) return result;

        if (ImguiHook.Options.IgnoreWindowUnactivate)
        {
            switch ((WindowMessage) msg)
            {
                case WindowMessage.WM_ACTIVATE:
                case WindowMessage.WM_ACTIVATEAPP:
                    if (wParam == IntPtr.Zero)
                        return IntPtr.Zero;
                    break;
                case WindowMessage.WM_KILLFOCUS:
                    return IntPtr.Zero;
            }
        }


        switch ((WindowMessage) msg)
        {
            // case WindowMessage.WM_LBUTTONDOWN:
            // case WindowMessage.WM_LBUTTONDBLCLK:
            // case WindowMessage.WM_RBUTTONDOWN:
            // case WindowMessage.WM_RBUTTONDBLCLK:
            // case WindowMessage.WM_MBUTTONDOWN:
            // case WindowMessage.WM_MBUTTONDBLCLK:
            // case WindowMessage.WM_XBUTTONDOWN:
            // case WindowMessage.WM_XBUTTONDBLCLK:
            //
            // case WindowMessage.WM_LBUTTONUP:
            // case WindowMessage.WM_RBUTTONUP:
            // case WindowMessage.WM_MBUTTONUP:
            // case WindowMessage.WM_XBUTTONUP:
            //
            // case WindowMessage.WM_MOUSEWHEEL:
            // case WindowMessage.WM_MOUSEHWHEEL:
            //     if (ImguiHook.IO.WantCaptureMouse)
            //         return new IntPtr(1);
            //     break;
            //
            // case WindowMessage.WM_KEYDOWN:
            // case WindowMessage.WM_SYSKEYDOWN:
            // case WindowMessage.WM_KEYUP:
            // case WindowMessage.WM_SYSKEYUP:
            // case WindowMessage.WM_CHAR:
            //     Console.WriteLine($"WM_Key {wParam}");
            //     if (ImguiHook.IO.WantCaptureKeyboard)
            //         return new IntPtr(1);
            //     break;

            case WindowMessage.WM_INPUT:
                // Console.WriteLine($"WM_Input {wParam}");
                if (ImguiHook.IO.WantCaptureMouse || ImguiHook.IO.WantCaptureKeyboard)
                    return new IntPtr(1);
                break;

            default:
                break;
        }

        return _wndProcHook == null
            ? IntPtr.Zero
            : _wndProcHook.OriginalFunction.Value.Invoke(hWnd, msg, wParam, lParam);
    }

    public void Suspend()
    {
        ImguiHook.Disable();
        _wndProcHook?.Disable();
    }

    public void Resume()
    {
        ImguiHook.Enable();
        _wndProcHook?.Enable();
    }

    public void Unload()
    {
        ImguiHook.Destroy();
        _wndProcHook?.Disable();
        _wndProcHook = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ImguiHook.Destroy();
        _wndProcHook?.Disable();
        _wndProcHook = null;
    }
}