## Variadic Hotfix
A simple DLL that wraps calls to variadic C(++) functions, primarily for use in game modding.

### How do I use these?

These functions are designed to be easily hooked into with asm, since mocking up varargs in other languages from C is a nightmare.

Hooking these functions should be as easy as pushing the *new* arguments, then calling the wrapper.

### Wrapper Functions

`extern "C" __declspec(dllexport) void __cdecl sprintf_to(void (__cdecl *callback)(char* formatted, int length), const int buffer_size, int ret, const char* format, ...)`

#### Calling:
```assembly
push dword 1000 #Buffer Size
push dword callback #Callback Function

call sprintf_to

add esp,8 #Clear stack

ret
```