using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SpiralNeo.Modules;

public class ConsoleCommandModule : ISpiralModule
{
    private Task _consoleLoop;
    private bool _running = true;

    public ConsoleCommandModule(Program program)
    {
        this._consoleLoop = Task.Run(Run);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _running = false;
        _consoleLoop.Dispose();
    }

    private async void Run()
    {
        while (_running)
        {
            var line = Console.ReadLine();
            if (line != null && line.StartsWith("/"))
            {
                Console.WriteLine($"Command: {line[1..]}");

                if (line == "/speaker")
                {
                    Console.WriteLine($"Speaker: {Marshal.ReadInt32(Dr1Addresses.Speaker)}");
                }
            }

            await Task.Delay(100);
        }
    }
}