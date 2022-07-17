using System;
using SpiralNeo.Configuration;

namespace SpiralNeo;

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