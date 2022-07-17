using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Info.Spiralframework.Neo.Interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Mod.Interfaces;
using Reloaded.Universal.Redirector.Interfaces;
using SpiralNeo.Configuration;
using SpiralNeo.Extensions;

namespace SpiralNeo
{
    public class IOModule : ISpiralModule
    {
        public const string NeoModId = "info.spiralframework.spiral.neo";

        public string[] ArchiveRoots = { "DrCommon", "Dr1", "Dr2" };

        public string[] ArchiveFolderNames =
        {
            "bin", "cg", "flash", "model", "modelbg", "module", "se", "texture", "texture/low", "bin", "cg",
            "flash", "font", "icon", "movie", "script", "voice", "texture", "save_icon"
        };

        public Dictionary<string, Dictionary<string, ValueTuple<string, int>>> RegisteredPathRedirections = new();

        internal readonly ILogger Logger;
        internal readonly VariadicHotfixer? Hotfixer;

        private IAsmHook? _traceLogAsmHook;
        private IReverseWrapper<HookDelegates.TraceLog>? _traceLogReverseWrapper;

        private IHook<HookDelegates.GetFilePath> _getFilePathHook;
        private IReverseWrapper<HookDelegates.GetFilePath> _getFilePathReverseWrapper;
        private IAsmHook _getFilePathAsmHookFirst;
        private IAsmHook _getFilePathAsmHookSecond;

        private IHook<HookDelegates.ReadFile> _readFileHook;

        private WeakReference<IRedirectorController>? _redirectController;
        private Dictionary<string, string> _redirects = new();
        private string _tempFilePath = Path.Combine(Dr1Paths.GamePath, "neo_tmp");


        public IOModule(Program program, VariadicHotfixer? hotfixer)
        {
            this.Logger = program.Logger;
            this.Hotfixer = hotfixer;
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

        private void CheckNeoPaths(Config config)
        {
            var loadedFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";

            if (config.SpiralSplashEnabled)
            {
                var splashScreen = Path.Combine(loadedFrom, "resources/ag_spiral.tga");

                if (RegisteredPathRedirections.TryGetValue(Dr1Paths.SplashScreen, out var dictionary))
                {
                    if (File.Exists(splashScreen))
                        dictionary.TryAdd(NeoModId, ("B:\\Test\\ag_spiral.png", int.MaxValue));
                    else dictionary.Remove(NeoModId);
                }
                else
                {
                    RegisteredPathRedirections[Dr1Paths.SplashScreen] = new Dictionary<string, (string, int)>
                    {
                        [NeoModId] = ("B:\\Test\\ag_spiral.png", int.MaxValue)
                    };
                }
            }
            else
            {
                if (RegisteredPathRedirections.TryGetValue(Dr1Paths.SplashScreen, out var dictionary))
                    dictionary.Remove(NeoModId);
            }
        }

        private void LoadVariadicHooks(IReloadedHooks hooks)
        {
            if (this.Hotfixer is null) return;

            _traceLogReverseWrapper = hooks.CreateReverseWrapper<HookDelegates.TraceLog>(this.TraceLogImpl);
            _traceLogAsmHook = this.Hotfixer
                .CreateSPrintfToHook(_traceLogReverseWrapper, (long) Dr1Addresses.LogTraceMessages)
                ?.Activate();
        }

        private void TraceLogImpl(IntPtr log, int len)
        {
            this.Logger.WriteLine($"[AGConsole] {Marshal.PtrToStringAnsi(log, len)}", this.Logger.ColorYellowLight);
        }

        private int GetFilePathImpl(IntPtr resultBuffer, string filename, int folderIndex)
        {
            var archiveRoots = new[] { "DrCommon", "Dr1", "Dr2" };
            var archiveFolderNames = new[]
            {
                "bin", "cg", "flash", "model", "modelbg", "module", "se", "texture", "texture/low", "bin", "cg",
                "flash", "font", "icon", "movie", "script", "voice", "texture", "save_icon"
            };

            var archiveRoot = archiveRoots[Marshal.ReadInt32(Dr1Addresses.ArchiveRoot)];
            var folder = archiveFolderNames[folderIndex - 1];

            var requestedFile = string.Empty;

            if (folderIndex is >= 10 and < 20)
            {
                requestedFile = $"{archiveRoot}/data/us/{folder}/{filename}";
            }
            else
            {
                requestedFile = $"{archiveRoot}/data/all/{folder}/{filename}";
            }

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
                                    Logger.WriteLine($"Could not create hard link: {error} (0x{error:X})");

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

            Marshalling.WriteAsciiString(resultBuffer, dest);

            this.Logger.WriteLine(
                $"[SpiralNeo] Loading {requestedFile} @ {dest}",
                this.Logger.ColorPink);

            return requestedFile.Length;
        }

        private int ReadFileImpl(IntPtr[] resultStructure, string path)
        {
            this.Logger.WriteLine($"[SpiralNeo] Reading File {path}");
            return _readFileHook.OriginalFunction(resultStructure, path);
        }
    }
}