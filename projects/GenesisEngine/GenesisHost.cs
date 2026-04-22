using Genesis.Terraria;

namespace Genesis.EngineHost;

public class GenesisHost : IDisposable
{
    #region 私有字段

    private TerrariaGame? _terrariaGame;
    private bool _isRunning;
    private readonly ulong _worldSeed;
    private ulong _frameCount;

    #endregion

    #region 构造函数

    public GenesisHost(ulong worldSeed)
    {
        _worldSeed = worldSeed;
        _isRunning = false;
        _frameCount = 0;
    }

    #endregion

    #region 公开方法

    public void Initialize()
    {
        Console.WriteLine($"[Genesis] 引擎初始化完成 - 世界种子: {_worldSeed}");

        _terrariaGame = new TerrariaGame(_worldSeed);
        _terrariaGame.Initialize();
    }

    public void Run()
    {
        _isRunning = true;
        Console.WriteLine("[Genesis] 引擎主循环已启动");

        if (_terrariaGame != null)
        {
            _terrariaGame.Run();
        }
        else
        {
            while (_isRunning)
            {
                _frameCount++;
                Thread.Sleep(16);
            }
        }

        Console.WriteLine("[Genesis] 引擎主循环已退出");
    }

    public void Shutdown()
    {
        _isRunning = false;
        _terrariaGame?.Shutdown();
        Console.WriteLine("[Genesis] 引擎关闭请求已发送");
    }

    #endregion

    #region IDisposable 实现

    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _terrariaGame?.Dispose();
        }

        _disposed = true;
    }

    #endregion
}
