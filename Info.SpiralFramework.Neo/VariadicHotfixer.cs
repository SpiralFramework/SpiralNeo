using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace SpiralNeo
{

    public class VariadicHotfixer
    {
        private IntPtr _sprintfTo = IntPtr.Zero;
        private bool _loadedSuccessfully = false;

        private IReloadedHooks? _hooks;

        public static VariadicHotfixer? TryLoading(Program program)
        {
            var hotfixer = new VariadicHotfixer(program);
            return hotfixer._loadedSuccessfully ? hotfixer : null;
        }

        private VariadicHotfixer(Program program)
        {
            var loadedFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var hModule = PInvoke.LoadLibrary(Path.Combine(loadedFrom, "VariadicHotfix.dll"));
            var win32Error = Marshal.GetLastWin32Error();

            if (hModule == IntPtr.Zero)
            {
                program.Logger.WriteLine(
                    $"[SpiralNeo] Failed to load VariadicHotfix, error: {win32Error} (0x{win32Error:X})",
                    program.Logger.ColorRed);

                return;
            }

            _loadedSuccessfully = true;
            _hooks = program.Hooks;

            _sprintfTo = PInvoke.GetProcAddress(hModule, "sprintf_to");
        }

        public string[]? GetSPrintfTo(IntPtr callback)
        {
            if (this._hooks == null || _sprintfTo == IntPtr.Zero) return null;

            return new[]
            {
                $"use32",

                $"push dword 1000",
                $"push dword {callback.ToInt32()}",

                $"{this._hooks.Utilities.GetAbsoluteCallMnemonics(_sprintfTo, false)}",

                $"add esp,8",

                $"ret"
            };
        }

        public IAsmHook? CreateSPrintfToHook(IReverseWrapper callback, long address,
            AsmHookBehaviour behaviour = AsmHookBehaviour.DoNotExecuteOriginal) =>
            CreateSPrintfToHook(callback.WrapperPointer, address, behaviour);

        public IAsmHook? CreateSPrintfToHook(IntPtr callback, long address,
            AsmHookBehaviour behaviour = AsmHookBehaviour.DoNotExecuteOriginal)
        {
            var asm = GetSPrintfTo(callback);

            return asm == null || this._hooks == null ? null : this._hooks.CreateAsmHook(asm, address, behaviour);
        }
    }
}