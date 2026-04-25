using System.Numerics;

namespace Genesis.Integration;

public sealed class Physics2DIntegration : IDisposable
{
    #region 常量

    private const float DefaultGravityY = -9.81f;

    #endregion

    #region 字段

    private bool _disposed;

    #endregion

    #region 属性

    public bool IsInitialized { get; private set; }

    #endregion

    #region 初始化

    public void Initialize()
    {
        IsInitialized = true;
        Console.WriteLine($"[Genesis] 2D 物理初始化完成 - 重力: {DefaultGravityY}");
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed) return;
        IsInitialized = false;
        _disposed = true;
    }

    #endregion
}
