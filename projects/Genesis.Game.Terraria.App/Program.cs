using System.Windows;

namespace Genesis.Game.Terraria;

public sealed class TerrariaApp : Application
{
    private readonly ulong _worldSeed;

    public TerrariaApp(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    [STAThread]
    public static void Main(string[] args)
    {
        var worldSeed = args.Length > 0 ? ulong.Parse(args[0]) : 42UL;
        var app = new TerrariaApp(worldSeed);
        app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new GameWindow(_worldSeed);
        window.Show();
    }
}
