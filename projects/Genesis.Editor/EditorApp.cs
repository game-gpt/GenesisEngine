using System.Windows;

namespace Genesis.Editor;

public sealed class EditorApp : Application
{
    private readonly ulong _worldSeed;

    public EditorApp(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new GenesisEditorWindow(_worldSeed);
        window.Show();
    }
}
