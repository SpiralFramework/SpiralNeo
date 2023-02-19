using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Info.SpiralFramework.Neo.Configuration;
using Info.SpiralFramework.Neo.Extensions;
using Info.SpiralFramework.Neo.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;

namespace Info.SpiralFramework.Neo.Modules
{
    public unsafe class IOModule : ISpiralModule
    {
        public const string NeoModId = "info.spiralframework.spiral.neo";

        public string[] ArchiveRoots = { "DrCommon", "Dr1", "Dr2" };

        public string[] ArchiveFolderNames =
        {
            "bin", "cg", "flash", "model", "modelbg", "module", "se", "texture", "texture/low", "bin", "cg",
            "flash", "font", "icon", "movie", "script", "voice", "texture", "save_icon"
        };

        public Dictionary<string, Dictionary<string, ValueTuple<string, int>>> RegisteredPathRedirections = new();

        private readonly ILogger _logger;
        private readonly VariadicHotfixer? _hotfixer;

        private IAsmHook? _traceLogAsmHook;
        private IReverseWrapper<HookDelegates.TraceLog>? _traceLogReverseWrapper;

        private IHook<HookDelegates.GetFilePath> _getFilePathHook;
        private IReverseWrapper<HookDelegates.GetFilePath> _getFilePathReverseWrapper;
        private IAsmHook _getFilePathAsmHookFirst;
        private IAsmHook _getFilePathAsmHookSecond;

        private IHook<HookDelegates.ReadFile> _readFileHook;
        private IHook<Dr1Delegates.FUN_000c5d30> _funC5D30Hook;
        private IHook<Dr1Delegates.FUN_001ae6d0> _fun1AE6D0Hook;

        private WeakReference<IRedirectorController>? _redirectController;
        private Dictionary<string, string> _redirects = new();
        private string _tempFilePath = Path.Combine(Dr1Paths.GamePath, "neo_tmp");


        public IOModule(Program program, VariadicHotfixer? hotfixer)
        {
            this._logger = program.Logger;
            this._hotfixer = hotfixer;
            this._redirectController = program.Redirects;

            _getFilePathHook = program.Hooks
                .CreateHook<HookDelegates.GetFilePath>(GetFilePathImpl, (long) Dr1Addresses.GetFilePath)
                .Activate();


            _readFileHook = program.Hooks
                .CreateHook<HookDelegates.ReadFile>(ReadFileImpl, (long) Dr1Addresses.ReadFile)
                .Activate();

            _getFilePathReverseWrapper =
                _getFilePathHook.ReverseWrapper;

            var getFilePathAsmHook = new[]
            {
                "use32",
                "push ecx",
                "push edx",
                "push eax",
                "push ecx",
                "lea eax, [ebp-0x150]",
                "push eax",
                program.Hooks.Utilities.GetAbsoluteCallMnemonics(_getFilePathReverseWrapper.WrapperPointer, false),
                "add esp, 0xC",
                "pop edx",
                "pop ecx"
            };

            _getFilePathAsmHookFirst = program.Hooks.CreateAsmHook(getFilePathAsmHook,
                    (long) Dr1Addresses.GetFilePathAsmFirst,
                    AsmHookBehaviour.DoNotExecuteOriginal, 86)
                .Activate();

            _getFilePathAsmHookSecond = program.Hooks.CreateAsmHook(getFilePathAsmHook,
                    (long) Dr1Addresses.GetFilePathAsmSecond,
                    AsmHookBehaviour.DoNotExecuteOriginal, 86)
                .Activate();

            _funC5D30Hook = program.Hooks
                .CreateHook<Dr1Delegates.FUN_000c5d30>(FunC5D30Impl, (long) Dr1Addresses.FUN_000c5d30)
                .Activate();

            _fun1AE6D0Hook = program.Hooks
                .CreateHook<Dr1Delegates.FUN_001ae6d0>(FUN_001ae6d0, (long) Dr1Addresses.FUN_001ae6d0)
                .Activate();

            LoadVariadicHooks(program.Hooks);

            ReloadConfig(program.Configuration);
        }

        public void ReloadConfig(Config config)
        {
            _redirects.Clear();
            if (Directory.Exists(_tempFilePath)) Directory.Delete(_tempFilePath, true);

            _traceLogAsmHook?.SetEnabled(config.TraceLogEnabled);

            if (config.PathReplacementEnabled)
            {
                _getFilePathHook.Enable();
                _getFilePathAsmHookFirst.Enable();
                _getFilePathAsmHookSecond.Enable();
            }
            else
            {
                _getFilePathHook.Disable();
                _getFilePathAsmHookFirst.Disable();
                _getFilePathAsmHookSecond.Disable();
            }

            CheckNeoPaths(config);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _redirects.Clear();
            if (Directory.Exists(_tempFilePath)) Directory.Delete(_tempFilePath, true);

            if (this._redirectController != null &&
                this._redirectController.TryGetTarget(out var controller))
            {
                foreach (var redirect in this._redirects.Keys)
                {
                    controller.RemoveRedirect(Path.GetFullPath(Path.Combine(_tempFilePath, redirect)));
                }
            }
        }

        public void RegisterPath(string gamePath, string modId, string replacementPath, int weight)
        {
            this._logger.WriteLine(
                $"[SpiralNeo] Trying to register a replacement {gamePath} => ({modId},{weight}) {replacementPath}");
            if (RegisteredPathRedirections.TryGetValue(gamePath, out var dictionary))
            {
                if (File.Exists(replacementPath))
                    dictionary.TryAdd(modId, (replacementPath, weight));
                else
                    dictionary.Remove(modId);
            }
            else
            {
                RegisteredPathRedirections[gamePath] = new Dictionary<string, (string, int)>
                {
                    [modId] = (replacementPath, weight)
                };
            }
        }

        public void DeregisterPath(string gamePath, string modId)
        {
            if (RegisteredPathRedirections.TryGetValue(gamePath, out var dictionary))
                dictionary.Remove(modId);
        }

        private void CheckNeoPaths(Config config)
        {
            var loadedFrom =
                "B:\\Danganronpa\\Mod Data"; //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

            if (config.SpiralSplashEnabled)
                RegisterPath(Dr1Paths.SplashScreen, NeoModId, Path.Combine(loadedFrom, "ag_spiral.tga"),
                    int.MaxValue);
            else
                DeregisterPath(Dr1Paths.SplashScreen, NeoModId);

            RegisterPath("Dr1/data/us/script/e00_001_000.lin", NeoModId,
                Path.Combine(loadedFrom, "e00_001_000.lin"), int.MaxValue);
        }

        private void LoadVariadicHooks(IReloadedHooks hooks)
        {
            if (this._hotfixer is null) return;

            _traceLogReverseWrapper = hooks.CreateReverseWrapper<HookDelegates.TraceLog>(this.TraceLogImpl);
            _traceLogAsmHook = this._hotfixer
                .CreateSPrintfToHook(_traceLogReverseWrapper, (long) Dr1Addresses.LogTraceMessages)
                ?.Activate();
        }

        private void TraceLogImpl(char* log, int len)
        {
            // this.Logger.WriteLine($"[AGConsole] {new string((sbyte*) log, 0, len)}", this.Logger.ColorYellowLight);
        }

        private unsafe int GetFilePathImpl(char* resultBuffer, string filename, int folderIndex)
        {
            var archiveRoot = ArchiveRoots[*Dr1Addresses.ArchiveRoot];
            var folder = ArchiveFolderNames[folderIndex - 1];

            var requestedFile = folderIndex is >= 10 and < 20
                ? $"{archiveRoot}/data/us/{folder}/{filename}"
                : $"{archiveRoot}/data/all/{folder}/{filename}";

            var dest = $"archive:{requestedFile}";
            var tmpPath = $"neo_tmp/{requestedFile}";

            if (this._redirects.ContainsKey(requestedFile))
            {
                dest = tmpPath;
            }
            else
            {
                // We have an optional dependency on Reloaded.Universal.Redirector to support what should be "guaranteed" linking
                // For some reason, Danganronpa doesn't like absolute paths?
                // It's weird

                if (RegisteredPathRedirections.TryGetValue(requestedFile, out var dictionary))
                {
                    var values = dictionary.Values.ToList();
                    values.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                    string? modPath = null;

                    foreach (var (path, _) in values)
                    {
                        if (!File.Exists(path)) continue;

                        modPath = path;
                        break;
                    }

                    if (modPath != null)
                    {
                        if (this._redirectController != null &&
                            this._redirectController.TryGetTarget(out var controller))
                        {
                            var key = Path.GetFullPath(Path.Combine(_tempFilePath, requestedFile));
                            var path = Path.GetFullPath(modPath);

                            _logger.WriteLine($"Adding redirect! {key} => {path}");
                            controller.AddRedirect(key, path);
                            this._redirects.TryAdd(requestedFile, path);

                            dest = tmpPath;
                        }
                        else
                        {
                            try
                            {
                                var link = Path.GetFullPath(Path.Combine(_tempFilePath, requestedFile));
                                var linkDir = Path.GetDirectoryName(link);
                                if (linkDir is not null) Directory.CreateDirectory(linkDir);

                                var path = Path.GetFullPath(modPath);

                                if (!PInvoke.CreateHardLink(link, path, IntPtr.Zero))
                                {
                                    var error = Marshal.GetLastWin32Error();
                                    _logger.WriteLine($"Could not create hard link: {error} (0x{error:X})");

                                    File.Copy(path, link);
                                }

                                this._redirects.TryAdd(requestedFile, path);
                                dest = tmpPath;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }


            this._logger.WriteLine($"[SpiralNeo:GetFilePath] Requesting {requestedFile} => {dest}");
            UnsafeUtils.WriteAsciiString(resultBuffer, dest);

            // this.Logger.WriteLine(
            // $"[SpiralNeo] Loading {requestedFile} @ {dest}",
            // this.Logger.ColorPink);

            return requestedFile.Length;
        }

        private int ReadFileImpl(void* resultStructure, string path)
        {
            this._logger.WriteLine($"[SpiralNeo] Reading File {path}");
            return _readFileHook.OriginalFunction(resultStructure, path);
        }

        private int FunC5D30Impl(string param_1, int param_2, int param_3)
        {
            var returnValue = _funC5D30Hook.OriginalFunction(param_1, param_2, param_3);

            _logger.WriteLine($"[SpiralNeo] FUN_C5D30({param_1}, {param_2}, {param_3}) => {returnValue}");

            return returnValue;
        }

        private int FUN_001ae6d0(void* self, int param_1, int param_2, void* param_3, int param_4)
        {
            var returnValue = _fun1AE6D0Hook.OriginalFunction(self, param_1, param_2, param_3, param_4);

            var path = new string((sbyte*) *((int*) ((byte*) param_3 + 4)), 0, *((int*) param_3));

            // Logger.WriteLine(
            // $"[SpiralNeo] FUN_001ae6d0({(int) self}, {param_1}, {param_2}, {path}, {param_4}) => {returnValue}");

            return returnValue;
        }

        private bool _isOpen = true;

        public void Render()
        {
            // Logger.WriteLine($"[SpiralNeo] WantCaptureMouse {ImGui.GetIO().WantCaptureMouse}");
            // ImGui.ShowDemoWindow(ref _isOpen);
        }
    }
}