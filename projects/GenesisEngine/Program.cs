using Genesis.EngineHost;

var worldSeed = args.Length > 0 ? ulong.Parse(args[0]) : 42UL;

using var host = new GenesisHost(worldSeed);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    host.Shutdown();
};

host.Initialize();
host.Run();
