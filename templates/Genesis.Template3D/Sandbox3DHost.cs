using Gnosis.ECS.World;

namespace Genesis.Template3D;

public sealed class Sandbox3DHost : IDisposable
{
    #region 字段

    private readonly ulong _worldSeed;
    private World? _world;
    private bool _isRunning;
    private bool _disposed;
    private DateTime _lastFrameTime;

    #endregion

    #region 构造函数

    public Sandbox3DHost(ulong worldSeed)
    {
        _worldSeed = worldSeed;
    }

    #endregion

    #region 公开方法

    public void Initialize()
    {
        Console.WriteLine($"[Genesis 3D] 初始化 - 世界种子: {_worldSeed}");

        _world = new World();
        _isRunning = true;
        _lastFrameTime = DateTime.UtcNow;

        Console.WriteLine("[Genesis 3D] ECS 世界已创建");
    }

    public void Run()
    {
        if (_world is null)
        {
            throw new InvalidOperationException("引擎未初始化，请先调用 Initialize()");
        }

        Console.WriteLine("[Genesis 3D] 主循环启动");

        while (_isRunning)
        {
            var now = DateTime.UtcNow;
            var delta = (float)(now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;

            if (delta > 0.1f)
            {
                delta = 0.1f;
            }

            _world.Update(delta);
        }

        Console.WriteLine("[Genesis 3D] 主循环退出");
    }

    public void Shutdown()
    {
        _isRunning = false;
        Console.WriteLine("[Genesis 3D] 关闭");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _world = null;
        _disposed = true;
    }

    #endregion
}
