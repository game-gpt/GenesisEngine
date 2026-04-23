namespace GenesisHost;

public class GenesisHost : IDisposable
{
    #region 私有字段

    private TerrariaGame? _terrariaGame;
    private readonly ulong _worldSeed;
    private bool _disposed;

    #endregion

    #region 构造函数

    public GenesisHost(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    #endregion

    #region 公开方法

    public void Initialize()
    {
        Console.WriteLine($"[Genesis] 引擎初始化 - 世界种子: {_worldSeed}");

        _terrariaGame = new TerrariaGame(_worldSeed);
        _terrariaGame.Initialize();
    }

    public void Run()
    {
        Console.WriteLine("[Genesis] 引擎主循环启动");

        _terrariaGame?.Run();

        Console.WriteLine("[Genesis] 引擎主循环退出");
    }

    public void Shutdown()
    {
        _terrariaGame?.Shutdown();
        Console.WriteLine("[Genesis] 引擎关闭");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _terrariaGame?.Dispose();
        _disposed = true;
    }

    #endregion
}
