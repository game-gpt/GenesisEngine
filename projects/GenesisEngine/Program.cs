using GenesisHost;

var worldSeed = 42UL;
var gameMode = "headless";
var gameDir = "";

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--seed":
            if (i + 1 < args.Length)
            {
                worldSeed = ulong.Parse(args[++i]);
            }
            break;
        case "--game":
            if (i + 1 < args.Length)
            {
                gameMode = args[++i];
            }
            break;
        case "--dir":
            if (i + 1 < args.Length)
            {
                gameDir = args[++i];
            }
            break;
    }
}

using var host = new GenesisHost(worldSeed);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    host.Shutdown();
};

switch (gameMode)
{
    case "terraria":
        host.InitializeWithWindow(1280, 720, "小小泰拉瑞亚");

        var terrariaDir = !string.IsNullOrEmpty(gameDir)
            ? gameDir
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "examples", "Genesis.Game.Terraria"));

        host.LoadGameScript(terrariaDir);
        host.Run();
        break;

    default:
        host.Initialize();
        host.Run();
        break;
}
