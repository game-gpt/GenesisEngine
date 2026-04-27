using System.Numerics;
using Genesis.Core;
using Genesis.HAL;
using Genesis.Integration.Rendering;
using ISystem = Gnosis.ECS.System.ISystem;
using IWorldSystem = Gnosis.ECS.System.IWorldSystem;
using SystemPhase = Gnosis.ECS.System.SystemPhase;
using GnosisWorld = Gnosis.ECS.World.World;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.Sprite2D;

namespace Genesis.GameSystems;

/// <summary>
/// 统一渲染游戏系统
/// 合并 GenesisRenderSystem（3D）和 Genesis2DRenderSystem（2D）
/// 支持设备所有权管理、帧控制、相机渲染
/// </summary>
public sealed class RenderGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IDevice? _device;
    private SpriteBatch? _spriteBatch;
    private Layer2DManager? _layerManager;
    private bool _initialized;
    private bool _ownsDevice;

    #endregion

    #region 属性

    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public SystemPhase Phase => SystemPhase.Render;

    /// <summary>
    /// 渲染设备
    /// </summary>
    public IDevice? Device => _device;

    /// <summary>
    /// 精灵批处理器
    /// </summary>
    public SpriteBatch? SpriteBatch => _spriteBatch;

    /// <summary>
    /// 图层管理器
    /// </summary>
    public Layer2DManager? LayerManager => _layerManager;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化渲染游戏系统
    /// </summary>
    public RenderGameSystem()
    {
    }

    #endregion

    #region ISystem 实现

    /// <summary>
    /// 初始化渲染系统
    /// </summary>
    public void Initialize()
    {
    }

    /// <summary>
    /// 关闭渲染系统
    /// </summary>
    public void Shutdown()
    {
        _spriteBatch?.Dispose();

        if (_ownsDevice)
        {
            _device?.Dispose();
        }

        _device = null;
        _spriteBatch = null;
        _layerManager = null;
        _initialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    /// <summary>
    /// 设置系统所属的 World
    /// </summary>
    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 使用指定后端初始化
    /// </summary>
    /// <param name="backend">图形后端</param>
    public void InitializeWithBackend(GraphicsBackend backend)
    {
        _device = DeviceFactory.Create(backend);
        _ownsDevice = true;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
    }

    /// <summary>
    /// 使用外部设备初始化
    /// </summary>
    /// <param name="device">渲染设备</param>
    public void InitializeWithDevice(IDevice device)
    {
        _device = device;
        _ownsDevice = false;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
    }

    /// <summary>
    /// 自动检测最佳后端并初始化
    /// </summary>
    public void InitializeAuto()
    {
        var backend = DeviceFactory.DetectBestBackend();
        InitializeWithBackend(backend);
    }

    #endregion

    #region 图层管理

    /// <summary>
    /// 创建图层
    /// </summary>
    /// <param name="name">图层名称</param>
    /// <param name="order">渲染顺序</param>
    /// <returns>图层实例</returns>
    public Layer2D CreateLayer(string name, int order = 0)
    {
        if (_layerManager is null)
        {
            throw new InvalidOperationException("渲染系统未初始化");
        }

        return _layerManager.AddLayer(name, order);
    }

    /// <summary>
    /// 获取图层
    /// </summary>
    /// <param name="name">图层名称</param>
    /// <returns>图层实例</returns>
    public Layer2D? GetLayer(string name)
    {
        return _layerManager?.GetLayer(name);
    }

    #endregion

    #region 帧控制

    /// <summary>
    /// 开始渲染帧
    /// </summary>
    /// <param name="sortMode">排序模式</param>
    /// <param name="blendMode">混合模式</param>
    /// <param name="transformMatrix">变换矩阵</param>
    public void BeginFrame(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendMode blendMode = BlendMode.Alpha, in Matrix4x4 transformMatrix = default)
    {
        _spriteBatch?.Begin(sortMode, blendMode, transformMatrix);
    }

    /// <summary>
    /// 结束渲染帧
    /// </summary>
    public void EndFrame()
    {
        if (_spriteBatch is not null && _layerManager is not null)
        {
            _layerManager.Render(_spriteBatch);
        }

        _spriteBatch?.End();
    }

    /// <summary>
    /// 使用相机偏移渲染
    /// </summary>
    /// <param name="cameraOffset">相机偏移</param>
    public void RenderWithCamera(Vector2 cameraOffset)
    {
        if (_spriteBatch is not null && _layerManager is not null)
        {
            _layerManager.Render(_spriteBatch, cameraOffset);
        }
    }

    #endregion

    #region ISystem.Update

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
    public void Update(float delta)
    {
        if (!_initialized || _world is null)
        {
            return;
        }

        SyncSpritesFromWorld();
    }

    #endregion

    #region 私有方法

    private void SyncSpritesFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<SpriteRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<SpriteRef>(entityId))
            {
                continue;
            }

            var spriteRef = _world.GetComponent<SpriteRef>(entityId);

            if (!spriteRef.IsValid || _layerManager is null)
            {
                continue;
            }

            var layer = _layerManager.GetLayer(spriteRef.LayerName);

            if (layer is null)
            {
                layer = _layerManager.AddLayer(spriteRef.LayerName, spriteRef.LayerOrder);
            }
        }
    }

    #endregion
}
