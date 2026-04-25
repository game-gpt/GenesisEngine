using GenesisEngine;
using Gnosis.Graphic.RHI;

var worldSeed = 42UL;
var gameMode = "headless";
var gameDir = "";
var backendStr = "";

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
        case "--backend":
            if (i + 1 < args.Length)
            {
                backendStr = args[++i];
            }
            break;
    }
}

var backend = backendStr.ToLower() switch
{
    "vulkan" => GraphicsBackend.Vulkan,
    "opengl" => GraphicsBackend.OpenGL,
    "software" => GraphicsBackend.Software,
    _ => DeviceFactory.DetectBestBackend()
};

using var host = new GenesisHost(worldSeed);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    host.Shutdown();
};

switch (gameMode)
{
    case "factorio":
        host.InitializeWithWindow(1280, 720, "异星工厂 - Genesis Demo", backend);

        var factorioDir = !string.IsNullOrEmpty(gameDir)
            ? gameDir
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "examples", "Genesis.Game.Factorio"));

        host.LoadGameScript(factorioDir);
        host.Run();
        break;

    case "minecraft":
        host.InitializeWithWindow(1280, 720, "我的世界 - Genesis Demo", backend);

        var minecraftDir = !string.IsNullOrEmpty(gameDir)
            ? gameDir
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "examples", "Genesis.Game.Minecraft"));

        host.LoadGameScript(minecraftDir);
        host.Run();
        break;

    case "noita":
        host.InitializeWithWindow(1280, 720, "女巫 - Genesis Demo", backend);

        var noitaDir = !string.IsNullOrEmpty(gameDir)
            ? gameDir
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "examples", "Genesis.Game.Noita"));

        host.LoadGameScript(noitaDir);
        host.Run();
        break;

    case "terraria":
        host.InitializeWithWindow(1280, 720, "小小泰拉瑞亚", backend);

        var terrariaDir = !string.IsNullOrEmpty(gameDir)
            ? gameDir
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "examples", "Genesis.Game.Terraria"));

        host.LoadGameScript(terrariaDir);
        host.Run();
        break;

    case "menu":
        host.InitializeWithWindow(1280, 720, "Genesis Engine - 游戏选择", backend);

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║       Genesis Engine 游戏选择        ║");
        Console.WriteLine("╠══════════════════════════════════════╣");
        Console.WriteLine("║  1. 异星工厂 (Factorio)              ║");
        Console.WriteLine("║  2. 我的世界 (Minecraft)             ║");
        Console.WriteLine("║  3. 女巫 (Noita)                     ║");
        Console.WriteLine("║  4. 小小泰拉瑞亚 (Terraria)          ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("使用 --game factorio|minecraft|noita|terraria 启动对应游戏");
        Console.WriteLine("示例: dotnet run --project projects/GenesisEngine -- --game factorio");
        Console.WriteLine();

        host.Run();
        break;

    default:
        Console.WriteLine();
        Console.WriteLine("用法: dotnet run --project projects/GenesisEngine -- [选项]");
        Console.WriteLine();
        Console.WriteLine("选项:");
        Console.WriteLine("  --game <游戏>      选择游戏: factorio, minecraft, noita, terraria");
        Console.WriteLine("  --seed <种子>      世界种子 (默认: 42)");
        Console.WriteLine("  --dir <路径>       游戏脚本目录 (覆盖默认路径)");
        Console.WriteLine("  --backend <后端>   图形后端: vulkan, opengl, software (默认: 自动检测)");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  dotnet run -- --game factorio");
        Console.WriteLine("  dotnet run -- --game minecraft --seed 12345");
        Console.WriteLine("  dotnet run -- --game terraria --backend opengl");
        Console.WriteLine();

        host.Initialize();
        host.Run();
        break;
}
