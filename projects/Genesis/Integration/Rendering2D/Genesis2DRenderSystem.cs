using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.ECS.System;
using Gnosis.ECS.World;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.Sprite2D;

namespace Genesis.Integration.Rendering2D;

[Obsolete("请使用 Genesis.GameSystems.RenderGameSystem 替代。Integration 层将在未来版本移除。")]
public sealed class Genesis2DRenderSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
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

    public void SetWorld(Gnosis.ECS.World.World world)
    {
        _world = world;
    }

    #endregion

    #region 公开方法

    public void InitializeWithBackend(GraphicsBackend backend)
    {
        _device = DeviceFactory.Create(backend);
        _ownsDevice = true;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
        Console.WriteLine($"[Genesis] 2D 渲染系统初始化完成 - 后端: {backend}");
    }

    public void InitializeWithDevice(IDevice device)
    {
        _device = device;
        _ownsDevice = false;
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();
        _initialized = true;
        Console.WriteLine("[Genesis] 2D 渲染系统初始化完成 - 使用外部设备");
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
            throw new InvalidOperationException("2D 渲染系统未初始化");
        }

        return _layerManager.AddLayer(name, order);
    }

    public Layer2D? GetLayer(string name)
    {
        return _layerManager?.GetLayer(name);
    }

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
