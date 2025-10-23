using System;
using FluffyByte.OPUL.Core.FluffyIO;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL;

public class Program
{
    public static async Task Main(string[] args)
    {
        if(args.Length == 0)
        {

        }


        Scribe.Initialize();

        Scribe.Debug("Preparing System Operator...");

        await FluffySystemOperator.Instance.StartAllAsync();

        Console.WriteLine("Press enter to terminate.");

        Console.ReadLine();

        await FluffySystemOperator.Instance.ShutdownAsync();

        Scribe.Info("Goodbye!");
    }


}