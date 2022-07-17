using Reloaded.Hooks.Definitions;

namespace SpiralNeo.Extensions;

public static class Hooks
{
    public static void SetEnabled(this IAsmHook hook, bool enabled)
    {
        if (enabled) hook.Enable();
        else hook.Disable();
    }

    public static void SetEnabled<T>(this IHook<T> hook, bool enabled)
    {
        if (enabled) hook.Enable();
        else hook.Disable();
    }
}