using System.Numerics;
using Gnosis.Graphic.RHI;
using Gnosis.Graphic.Sprite2D;

namespace Genesis.Integration;

public sealed class Graphic2DIntegration : IDisposable
{
    #region 字段

    private IDevice? _device;
    private SpriteBatch? _spriteBatch;
    private Layer2DManager? _layerManager;
    private bool _disposed;

    #endregion

    #region 属性

    public IDevice? Device => _device;

    public SpriteBatch? SpriteBatch => _spriteBatch;

    public Layer2DManager? LayerManager => _layerManager;

    public bool IsInitialized => _device is not null;

    #endregion

    #region 初始化

    public void Initialize(GraphicsBackend backend = default)
    {
        if (backend == default)
        {
            backend = DeviceFactory.DetectBestBackend();
        }

        _device = DeviceFactory.Create(backend);
        _spriteBatch = new SpriteBatch(_device);
        _layerManager = new Layer2DManager();

        Console.WriteLine($"[Genesis] 2D 渲染初始化完成 - 后端: {backend}");
    }

    #endregion

    #region 图层管理

    public Layer2D CreateLayer(string name, int order = 0)
    {
        if (_layerManager is null)
        {
            throw new InvalidOperationException("渲染系统未初始化，请先调用 Initialize()");
        }

        return _layerManager.AddLayer(name, order);
    }

    public bool RemoveLayer(string name)
    {
        return _layerManager?.RemoveLayer(name) ?? false;
    }

    public Layer2D? GetLayer(string name)
    {
        return _layerManager?.GetLayer(name);
    }

    #endregion

    #region 帧渲染

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

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _spriteBatch?.Dispose();
        _device?.Dispose();

        _disposed = true;
    }

    #endregion
}
