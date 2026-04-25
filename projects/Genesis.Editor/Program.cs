namespace Genesis.Editor;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var worldSeed = args.Length > 0 ? ulong.Parse(args[0]) : 42UL;

        var app = new EditorApp(worldSeed);
        app.Run();
    }
}
