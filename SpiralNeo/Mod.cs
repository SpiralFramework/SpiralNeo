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

namespace SpiralNeo
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod
    {
        // private IHook<TraceLog> _traceLogHook;
        private IAsmHook? _traceLogAsmHook;
        private IReverseWrapper<TraceLog>? _traceLogReverseWrapper;

        internal ILogger logger;
        internal VariadicHotfixer? _hotfixer;

        public Mod(IReloadedHooks hooks, ILogger logger)
        {
            this.logger = logger;

            LoadVariadicHooks(hooks);
        }

        private void LoadVariadicHooks(IReloadedHooks hooks)
        {
            this._hotfixer = VariadicHotfixer.TryLoading(hooks, logger);
            if (this._hotfixer == null) return;

            _traceLogReverseWrapper = hooks.CreateReverseWrapper<TraceLog>(this.TraceLogImpl);
            _traceLogAsmHook = this._hotfixer
                ?.CreateSPrintfToHook(_traceLogReverseWrapper, (long) Dr1Addresses.LogTraceMessages)
                ?.Activate();
        }

        [Function(CallingConventions.Cdecl)]
        public delegate void TraceLog(IntPtr log, int len);

        private void TraceLogImpl(IntPtr log, int len)
        {
            this.logger.WriteLine($"[AGConsole] {Marshal.PtrToStringAnsi(log, len)}", this.logger.ColorYellowLight);
        }
    }
}