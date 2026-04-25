using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Genesis.Game.Terraria.Game;
using Genesis.Game.Terraria.Input;
using Genesis.Game.Terraria.Rendering;
using Gnosis.ECS.World;

namespace Genesis.Game.Terraria;

public sealed class GameWindow : Window
{
    #region 字段

    private readonly WriteableBitmap _bitmap;
    private readonly System.Windows.Threading.DispatcherTimer _timer;
    private readonly TerrariaGameHost _gameHost;
    private readonly GameInputBridge _inputBridge;
    private readonly Terraria2DRenderer _renderer;

    private readonly int _width = 1280;
    private readonly int _height = 720;
    private float _time;
    private int _frameCount;
    private DateTime _lastFrameTime;

    #endregion

    #region 构造函数

    public GameWindow(ulong worldSeed)
    {
        Title = "小小泰拉瑞亚";
        Width = _width;
        Height = _height;
        Background = Brushes.Black;
        ResizeMode = ResizeMode.NoResize;

        _bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgr32, null);

        var image = new System.Windows.Controls.Image
        {
            Source = _bitmap,
            Stretch = Stretch.Uniform
        };
        Content = image;

        _gameHost = new TerrariaGameHost(worldSeed, _width, _height);
        _renderer = new Terraria2DRenderer(_gameHost.World, _gameHost.TileMap, _width, _height);
        _inputBridge = new GameInputBridge(_gameHost.InputSystem);

        _gameHost.Initialize();

        _lastFrameTime = DateTime.UtcNow;

        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTick;
        _timer.Start();

        KeyDown += _inputBridge.OnKeyDown;
        KeyUp += _inputBridge.OnKeyUp;
        MouseDown += _inputBridge.OnMouseDown;
        MouseUp += _inputBridge.OnMouseUp;
        MouseMove += _inputBridge.OnMouseMove;

        Closed += OnClosed;
    }

    #endregion

    #region 渲染循环

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var delta = (float)(now - _lastFrameTime).TotalSeconds;
        _lastFrameTime = now;

        if (delta > 0.1f)
        {
            delta = 0.1f;
        }

        _time += delta;
        _frameCount++;

        _inputBridge.Update();
        _gameHost.Update(delta);

        Render();

        unsafe
        {
            _bitmap.Lock();
            var data = _renderer.GetFramebuffer();
            fixed (byte* src = data)
            {
                Buffer.MemoryCopy(src, _bitmap.BackBuffer.ToPointer(), data.Length, data.Length);
            }
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
            _bitmap.Unlock();
        }

        var fps = _time > 0 ? _frameCount / _time : 0;
        Title = $"小小泰拉瑞亚 - FPS: {fps:0} - 种子: {_gameHost.WorldSeed}";
    }

    private void Render()
    {
        _renderer.Render();
    }

    #endregion

    #region 事件处理

    private void OnClosed(object? sender, EventArgs e)
    {
        _timer.Stop();
        _gameHost.Shutdown();
    }

    #endregion
}
