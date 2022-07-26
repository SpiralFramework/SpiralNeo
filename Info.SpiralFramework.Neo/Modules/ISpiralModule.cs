using System;
using Info.SpiralFramework.Neo.Configuration;

namespace Info.SpiralFramework.Neo.Modules;

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
}