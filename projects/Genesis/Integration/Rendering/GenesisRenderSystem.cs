using System.Numerics;
using Gnosis.Core.Entity;
using Gnosis.ECS.System;
using Gnosis.ECS.World;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.Sprite2D;

namespace Genesis.Integration.Rendering;

[Obsolete("请使用 Genesis.GameSystems.RenderGameSystem 替代。Integration 层将在未来版本移除。")]
public sealed class GenesisRenderSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private IDevice? _device;
    private SpriteBatch? _spriteBatch;
    private Layer2DManager? _layerManager;
    private bool _initialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Render;

    public IDevice? Device => _device;

    public SpriteBatch? SpriteBatch => _spriteBatch;

    public Layer2DManager? LayerManager => _layerManager;

    public bool IsInitialized => _initialized;

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
    }

    public void Shutdown()
    {
        _spriteBatch?.Dispose();
        _device?.Dispose();
        _device = null;
        _spriteBatch = null;
        _layerManager = null;
        _initialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    public void SetWorld(Gnosis.ECS.World.World world)
    {
        _world = world;
    }

    #endregion

    #region 公开方法

    public void InitializeWithBackend(GraphicsBackend backend)
    {
        _device = DeviceFactory.Create(backend);
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
    }

    public void InitializeAuto()
    {
        var backend = DeviceFactory.DetectBestBackend();
        InitializeWithBackend(backend);
    }

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

    #region ISystem.Update

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
