using System;
using Info.SpiralFramework.Neo.Configuration;
using Info.SpiralFramework.Neo.Configuration.Implementation;
using Info.SpiralFramework.Neo.Modules;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using Reloaded.Universal.Redirector.Interfaces;

namespace Info.SpiralFramework.Neo
{
    public unsafe class Program : IMod, IDisposable
    {
        /// <summary>
        /// Used for writing text to the Reloaded log.
        /// </summary>
        internal ILogger Logger = null!;

        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        internal IModLoader ModLoader = null!;

        /// <summary>
        /// Stores the contents of your mod's configuration. Automatically updated by template.
        /// </summary>
        internal Config Configuration = null!;

        /// <summary>
        /// An interface to Reloaded's the function hooks/detours library.
        /// See: https://github.com/Reloaded-Project/Reloaded.Hooks
        ///      for documentation and samples.
        /// </summary>
        internal IReloadedHooks Hooks = null!;

        internal WeakReference<IRedirectorController>? Redirects = null;

        /// <summary>
        /// Configuration of the current mod.
        /// </summary>
        internal IModConfig ModConfig = null!;

        internal VariadicHotfixer? Hotfixer;

        internal IOModule IOModule = null!;
        internal ConsoleCommandModule ConsoleCommandModule = null!;

        internal ISpiralModule[] Modules = null!;

        /// <summary>
        /// Entry point for your mod.
        /// </summary>
        public void StartEx(IModLoaderV1 loaderApi, IModConfigV1 modConfig)
        {
            try
            {
                // For more information about this template, please see
                // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

                ModLoader = (IModLoader) loaderApi;
                ModConfig = (IModConfig) modConfig;
                Logger = (ILogger) ModLoader.GetLogger();
                ModLoader.GetController<IReloadedHooks>().TryGetTarget(out Hooks!);
                Redirects = ModLoader.GetController<IRedirectorController>();

                // Your config file is in Config.json.
                // Need a different name, format or more configurations? Modify the `Configurator`.
                // If you do not want a config, remove Configuration folder and Config class.
                var configurator = new Configurator(ModLoader.GetModConfigDirectory(ModConfig.ModId));
                Configuration = configurator.GetConfiguration<Config>(0);
                Configuration.ConfigurationUpdated += OnConfigurationUpdated;

                // use this class for only interfacing with mod loader.
                Hotfixer = VariadicHotfixer.TryLoading(this);
                IOModule = new IOModule(this, Hotfixer);
                ConsoleCommandModule = new ConsoleCommandModule(this);

                Modules = new ISpiralModule[]
                {
                    IOModule, ConsoleCommandModule
                };

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");

                throw;
            }
        }

        private void OnConfigurationUpdated(IConfigurable obj)
        {
            /*
                This is executed when the configuration file gets 
                updated by the user at runtime.
            */

            // Replace configuration with new.
            Configuration = (Config) obj;
            Logger.WriteLine($"[{ModConfig.ModId}] Config Updated: Applying");

            // Apply settings from configuration.
            foreach (var module in Modules) module.ReloadConfig(Configuration);
        }

        /* Mod loader actions. */
        public void Suspend()
        {
            /*  Some tips if you wish to support this (CanSuspend == true)
             
                A. Undo memory modifications.
                B. Deactivate hooks. (Reloaded.Hooks Supports This!)
            */

            foreach (var module in Modules) module.Suspend();
        }

        public void Resume()
        {
            /*  Some tips if you wish to support this (CanSuspend == true)
             
                A. Redo memory modifications.
                B. Re-activate hooks. (Reloaded.Hooks Supports This!)
            */

            foreach (var module in Modules) module.Resume();
        }

        public void Unload()
        {
            /*  Some tips if you wish to support this (CanUnload == true).
             
                A. Execute Suspend(). [Suspend should be reusable in this method]
                B. Release any unmanaged resources, e.g. Native memory.
            */

            foreach (var module in Modules) module.Unload();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            foreach (var module in Modules) module.Dispose();
        }

        /*  If CanSuspend == false, suspend and resume button are disabled in Launcher and Suspend()/Resume() will never be called.
            If CanUnload == false, unload button is disabled in Launcher and Unload() will never be called.
        */
        public bool CanUnload() => false;
        public bool CanSuspend() => false;

        /* Automatically called by the mod loader when the mod is about to be unloaded. */
        public Action Disposing => Dispose;
    }
}