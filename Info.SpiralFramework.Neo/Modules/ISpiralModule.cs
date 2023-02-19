using System;
using Info.SpiralFramework.Neo.Configuration;

namespace Info.SpiralFramework.Neo.Modules;

using Reloaded.Mod.Interfaces;

public interface ISpiralModule : IDisposable
{
    public void ReloadConfig(Config config)
    {
    }

    public void Suspend()
    {
    }

    public void Resume()
    {
    }

    public void Unload()
    {
    }

    public void Render()
    {
    }

    public void OnModLoaderInitialised()
    {
        
    }
}