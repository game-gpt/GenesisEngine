using System.Numerics;
using Genesis.Core;
using Genesis.GameSystems.Components;
using Genesis.HAL;
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
/// 通过 HAL 接口访问实体世界
/// </summary>
public sealed class RenderGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IEntityWorld? _entityWorld;
    private IDevice? _device;
    private SpriteBatch? _spriteBatch;
    private Layer2DManager? _layerManager;
    private bool _initialized;
    private bool _ownsDevice;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Render;

    public IDevice? Device => _device;

    public SpriteBatch? SpriteBatch => _spriteBatch;

    public Layer2DManager? LayerManager => _layerManager;

    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    public RenderGameSystem()
    {
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
    }

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

    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    /// <summary>
    /// 设置 HAL 实体世界
    /// 优先于 GnosisWorld 使用
    /// </summary>
    public void SetEntityWorld(IEntityWorld entityWorld)
    {
        _entityWorld = entityWorld;
    }

    #endregion

    #region 初始化方法

    public void InitializeWithBackend(GraphicsBackend backend)
    {
        _device = DeviceFactory.Create(backend);
        _ownsDevice = true;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
    }

    public void InitializeWithDevice(IDevice device)
    {
        _device = device;
        _ownsDevice = false;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
    }

    public void InitializeAuto()
    {
        var backend = DeviceFactory.DetectBestBackend();
        InitializeWithBackend(backend);
    }

    #endregion

    #region 图层管理

    public Layer2D CreateLayer(string name, int order = 0)
    {
        if (_layerManager is null)
        {
            throw new InvalidOperationException("渲染系统未初始化");
        }

        return _layerManager.AddLayer(name, order);
    }

    public Layer2D? GetLayer(string name)
    {
        return _layerManager?.GetLayer(name);
    }

    #endregion

    #region 帧控制

    public void BeginFrame(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendMode blendMode = BlendMode.Alpha, in Matrix4x4 transformMatrix = default)
    {
        _spriteBatch?.Begin(sortMode, blendMode, transformMatrix);
    }

    public void EndFrame()
    {
        if (_spriteBatch is not null && _layerManager is not null)
        {
            _layerManager.Render(_spriteBatch);
        }

        _spriteBatch?.End();
    }

    public void RenderWithCamera(Vector2 cameraOffset)
    {
        if (_spriteBatch is not null && _layerManager is not null)
        {
            _layerManager.Render(_spriteBatch, cameraOffset);
        }
    }

    #endregion

    #region ISystem.Update

    public void Update(float delta)
    {
        if (!_initialized)
        {
            return;
        }

        var ew = _entityWorld;
        if (ew is not null)
        {
            SyncSpritesFromEntityWorld(ew);
            return;
        }

        if (_world is not null)
        {
            SyncSpritesFromWorld();
        }
    }

    #endregion

    #region 私有方法

    private void SyncSpritesFromEntityWorld(IEntityWorld ew)
    {
        var entities = ew.CreateQuery()
            .All<Transform3D>()
            .All<SpriteRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!ew.HasComponent<SpriteRef>(entityId))
            {
                continue;
            }

            var spriteRef = ew.GetComponent<SpriteRef>(entityId);

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
