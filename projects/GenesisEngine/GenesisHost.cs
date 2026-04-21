using Genesis.Core.Enums;
using Genesis.Core.ValueObjects;
using Genesis.Spacetime.Interfaces;
using Genesis.Spacetime.Implementations;

namespace Genesis.EngineHost;

/// <summary>
/// Genesis 引擎宿主，管理引擎的生命周期与主循环
/// </summary>
public class GenesisHost : IDisposable
{
    #region 私有字段

    private readonly SpacetimeTree _spacetimeTree;
    private bool _isRunning;
    private readonly ulong _worldSeed;
    private ulong _frameCount;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 Genesis 引擎宿主
    /// </summary>
    /// <param name="worldSeed">世界种子值</param>
    public GenesisHost(ulong worldSeed)
    {
        _spacetimeTree = new SpacetimeTree();
        _worldSeed = worldSeed;
        _isRunning = false;
        _frameCount = 0;
    }

    #endregion

    #region 公开方法

    /// <summary>
    /// 初始化引擎，创建根 L3 节点
    /// </summary>
    public void Initialize()
    {
        var rootBounds = new Bounds(-256, -256, -256, 256, 256, 256);
        var rootNode = new HChunkNode(0, NodeLevel.L3, rootBounds, _worldSeed);

        _spacetimeTree.InsertNode(rootNode);

        Console.WriteLine($"[Genesis] 引擎初始化完成 - 世界种子: {_worldSeed}");
        Console.WriteLine($"[Genesis] 根节点已创建 - 空间哈希: {rootNode.SpatialHash:X16}");
    }

    /// <summary>
    /// 运行引擎主循环
    /// </summary>
    public void Run()
    {
        _isRunning = true;
        Console.WriteLine("[Genesis] 引擎主循环已启动");

        while (_isRunning)
        {
            UpdateSimulation();

            _frameCount++;

            if (_frameCount % 60 == 0)
            {
                Console.WriteLine($"[Genesis] 帧计数: {_frameCount} - 根节点状态: {_spacetimeTree.Root?.CollapseState}");
            }

            Thread.Sleep(16);
        }

        Console.WriteLine("[Genesis] 引擎主循环已退出");
    }

    /// <summary>
    /// 关闭引擎
    /// </summary>
    public void Shutdown()
    {
        _isRunning = false;
        Console.WriteLine("[Genesis] 引擎关闭请求已发送");
    }

    /// <summary>
    /// 更新模拟逻辑，预留注意力驱动与坍缩处理
    /// </summary>
    public void UpdateSimulation()
    {
    }

    #endregion

    #region IDisposable 实现

    private bool _disposed;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源的核心方法
    /// </summary>
    /// <param name="disposing">是否释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Shutdown();
        }

        _disposed = true;
    }

    #endregion
}
