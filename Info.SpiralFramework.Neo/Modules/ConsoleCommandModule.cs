using System;
using System.Threading.Tasks;
using Info.SpiralFramework.Neo.Interfaces;

namespace Info.SpiralFramework.Neo.Modules;

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

                unsafe
                {
                    switch (line)
                    {
                        case "/speaker":
                            Console.WriteLine($"Speaker: {*(Dr1Addresses.Speaker)}");
                            break;
                        case "/lin":
                            var uVar6 = *(Dr1Addresses.uVar6);
                            var bVar1 = *((byte*) Dr1Addresses.bVar1 + uVar6);
                            var cVar2 = *((byte*) Dr1Addresses.bVar1 + 2 + uVar6);
                            var bVar3 = *((byte*) Dr1Addresses.bVar1 + 1 + uVar6);
                            var puVar6 = uVar6 & 0xffffff00;

                            Console.WriteLine($"{uVar6} / {bVar1} / {cVar2} / {bVar3} / {puVar6}");

                            break;

                        case "/state":

                            break;
                    }
                }
            }

            await Task.Delay(100);
        }
    }
}