// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <cstdio>      /* vsprintf_s */
#include <cstdarg>     /* va_list, va_start, va_arg, va_end */

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
    default:
        break;
    }
    return TRUE;
}

// ReSharper disable once CppParameterNeverUsed
extern "C" __declspec(dllexport) void __cdecl sprintf_to(void (__cdecl *callback)(char* formatted, int length), const int buffer_size, int ret, const char* format, ...) {
    const auto buffer = new char[buffer_size];
    va_list vl;
    va_start(vl, format);
    const int length = vsprintf_s(buffer, buffer_size, format, vl);
    va_end(vl);

    callback(buffer, length);

    delete[] buffer;
}
