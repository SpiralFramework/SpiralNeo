namespace Info.SpiralFramework.Neo.Extensions;

using Interfaces;
using Reloaded.Hooks.Definitions;

public abstract class OpCodeHook
{
    public class BaseHooked : OpCodeHook, IHook<Dr1Delegates.OpCodeFunction>
    {
        private IHook<Dr1Delegates.OpCodeFunction> _hook;

        public Dr1Delegates.OpCodeFunction OriginalFunction => _hook.OriginalFunction;

        public IReverseWrapper<Dr1Delegates.OpCodeFunction> ReverseWrapper => _hook.ReverseWrapper;

        public bool IsHookEnabled => _hook.IsHookEnabled;

        public bool IsHookActivated => _hook.IsHookActivated;

        public nint OriginalFunctionAddress => _hook.OriginalFunctionAddress;

        public nint OriginalFunctionWrapperAddress => _hook.OriginalFunctionWrapperAddress;

        public BaseHooked(IHook<Dr1Delegates.OpCodeFunction> hook)
        {
            this._hook = hook;
        }

        public IHook<Dr1Delegates.OpCodeFunction> Activate()
        {
            return _hook.Activate();
        }

        public void Disable()
        {
            _hook.Disable();
        }

        public void Enable()
        {
            _hook.Enable();
        }

        public override nint ToPtr() => _hook.ReverseWrapper.WrapperPointer;
    }

    public class BaseWrapper : OpCodeHook
    {
        private nint _wrapper;

        public BaseWrapper(nint wrapper)
        {
            this._wrapper = wrapper;
        }

        public override nint ToPtr() => _wrapper;
    }

    public class BaseReverseWrapper : OpCodeHook, IReverseWrapper<Dr1Delegates.OpCodeFunction>
    {
        private IReverseWrapper<Dr1Delegates.OpCodeFunction> _wrapper;

        public Dr1Delegates.OpCodeFunction CSharpFunction => _wrapper.CSharpFunction;

        public nint NativeFunctionPtr => _wrapper.NativeFunctionPtr;

        public nint WrapperPointer => _wrapper.WrapperPointer;

        public BaseReverseWrapper(IReverseWrapper<Dr1Delegates.OpCodeFunction> wrapper)
        {
            _wrapper = wrapper;
        }

        public override nint ToPtr() => WrapperPointer;
    }

    public class Undefined : OpCodeHook
    {
        public static readonly Undefined Instance = new();

        static Undefined()
        {
        }

        private Undefined()
        {
        }

        public override nint ToPtr() => nint.Zero;
    }

    public abstract nint ToPtr();
}