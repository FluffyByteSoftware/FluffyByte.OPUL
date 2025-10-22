using System;
using FluffyByte.OPUL.Core.FluffyIO.FluffyConsole;

namespace FluffyByte.OPUL;

public class Program
{
    public static CancellationTokenSource Cts { get; private set; } = new();

    public static void Main(string[] args)
    {
        if(args.Length == 0)
        {

        }

        Scribe.Initialize();

        Scribe.Info("Press enter to start the server...");
        
        Console.ReadLine();

        Start();

        Console.ReadLine();

        Scribe.Info("Server shutting down...");
        Scribe.Info("Goodbye!");
        Shutdown();

    }

    private static void Start()
    {
        Cts = new();
    }


    private static void Shutdown()
    {
        Cts.Cancel();
    }
}