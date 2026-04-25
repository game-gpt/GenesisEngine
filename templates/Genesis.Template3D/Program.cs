var worldSeed = args.Length > 0 ? ulong.Parse(args[0]) : 42UL;

using var host = new Genesis.Template3D.Sandbox3DHost(worldSeed);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    host.Shutdown();
};

host.Initialize();
host.Run();
