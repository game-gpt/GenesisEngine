using System.Drawing;
using System.Windows.Forms;
using Genesis.Terraria.Components;

namespace Genesis.Terraria.GUI;

public sealed class TerrariaForm : Form
{
    #region 私有字段

    private readonly PictureBox _gameCanvas;
    private readonly Label _statusLabel;
    private readonly Panel _inventoryPanel;
    private readonly PictureBox _tilesetPreview;
    private readonly ToolStrip _toolStrip;
    private readonly Timer _gameTimer;
    private readonly Timer _fpsTimer;

    private TerrariaGame? _game;
    private Bitmap _frameBuffer;
    private Graphics? _bufferGraphics;
    private int _tileSize = 8;
    private int _viewportTilesX = 80;
    private int _viewportTilesY = 50;

    private DateTime _lastFrameTime;
    private float _deltaTime;
    private int _frameCount;
    private int _fps;
    private bool _isRunning;

    #endregion

    #region 公开属性

    public bool IsRunning => _isRunning;
    public InputState Input { get; } = new();

    #endregion

    #region 构造函数

    public TerrariaForm()
    {
        Text = "Genesis Engine - 泰拉瑞亚 MVP";
        Size = new Size(1024, 768);
        MinimumSize = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(20, 20, 30);
        KeyPreview = true;

        _frameBuffer = new Bitmap(_viewportTilesX * _tileSize, _viewportTilesY * _tileSize);
        _bufferGraphics = Graphics.FromImage(_frameBuffer);
        _bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        _bufferGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        InitializeUI();
        InitializeTimers();
        SetupEventHandlers();

        _lastFrameTime = DateTime.UtcNow;
    }

    #endregion

    #region UI 初始化

    private void InitializeUI()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(4)
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));

        _toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            BackColor = Color.FromArgb(40, 40, 55),
            Renderer = new FlatToolStripRenderer()
        };

        var titleLabel = new ToolStripLabel("🎮 Genesis 泰拉瑞亚 MVP")
        {
            Font = new Font("Microsoft YaHei UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 200, 160)
        };

        var seedLabel = new ToolStripLabel("种子: 42")
        {
            ForeColor = Color.FromArgb(150, 150, 170)
        };

        var fpsLabel = new ToolStripLabel("FPS: --")
        {
            ForeColor = Color.FromArgb(120, 180, 120)
        };

        fpsLabel.Tag = "fps";

        _toolStrip.Items.Add(titleLabel);
        _toolStrip.Items.Add(new ToolStripSeparator());
        _toolStrip.Items.Add(seedLabel);
        _toolStrip.Items.Add(fpsLabel);

        var canvasPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(0)
        };

        _gameCanvas = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };

        _gameCanvas.Image = _frameBuffer;
        canvasPanel.Controls.Add(_gameCanvas);

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10),
            ForeColor = Color.FromArgb(200, 200, 210),
            BackColor = Color.FromArgb(30, 30, 42),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            Text = "按任意键开始游戏..."
        };

        _inventoryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 30, 42),
            Padding = new Padding(8)
        };

        var inventoryLayout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoScroll = false,
            WrapContents = false
        };

        for (var i = 0; i < 10; i++)
        {
            var slot = CreateInventorySlot(i);
            inventoryLayout.Controls.Add(slot);
        }

        _inventoryPanel.Controls.Add(inventoryLayout);

        mainLayout.Controls.Add(_toolStrip, 0, 0);
        mainLayout.Controls.Add(canvasPanel, 1, 1);
        mainLayout.Controls.Add(_statusLabel, 2, 0);
        mainLayout.SetColumnSpan(_statusLabel, 1);
        mainLayout.Controls.Add(_inventoryPanel, 2, 2);

        Controls.Add(mainLayout);
    }

    private Control CreateInventorySlot(int index)
    {
        var panel = new Panel
        {
            Size = new Size(64, 64),
            Margin = new Padding(2),
            BackColor = Color.FromArgb(45, 45, 60),
            BorderStyle = BorderStyle.FixedSingle,
            Tag = index
        };

        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = $"[{index + 1}]",
            BackColor = Color.Transparent
        };

        panel.Controls.Add(label);
        panel.Tag = new { Index = index, Label = label, Panel = panel };

        return panel;
    }

    #endregion

    #region 计时器

    private void InitializeTimers()
    {
        _gameTimer = new Timer
        {
            Interval = 16
        };
        _gameTimer.Tick += GameTick;

        _fpsTimer = new Timer
        {
            Interval = 1000
        };
        _fpsTimer.Tick += FpsTick;
    }

    #endregion

    #region 事件处理

    private void SetupEventHandlers()
    {
        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
        FormClosing += OnFormClosing;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        e.SuppressKeyPress = true;

        Input.SetKey(e.KeyCode, true);

        if (e.KeyCode == Keys.Escape && _game != null)
        {
            _isRunning = !_isRunning;
            if (_isRunning)
            {
                _gameTimer.Start();
                _fpsTimer.Start();
            }
            else
            {
                _gameTimer.Stop();
                _fpsTimer.Stop();
            }
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        Input.SetKey(e.KeyCode, false);
    }

    private void OnFormClosing(FormClosingEventArgs e)
    {
        if (_game != null)
        {
            _game.Shutdown();
        }

        _gameTimer.Stop();
        _fpsTimer.Stop();
    }

    private void GameTick(object? sender, EventArgs e)
    {
        if (!_isRunning || _game == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        _deltaTime = (float)(now - _lastFrameTime).TotalSeconds;
        _lastFrameTime = now;

        if (_deltaTime > 0.1f)
        {
            _deltaTime = 0.1f;
        }

        _game.Update(_deltaTime);
        _frameCount++;

        RenderFrame();
    }

    private void FpsTick(object? sender, EventArgs e)
    {
        _fps = _frameCount;
        _frameCount = 0;

        foreach (ToolStripItem item in _toolStrip.Items)
        {
            if (item.Tag as string == "fps")
            {
                item.Text = $"FPS: {_fps}";
                break;
            }
        }
    }

    #endregion

    #region 游戏集成

    public void AttachGame(TerrariaGame game)
    {
        _game = game;
        _isRunning = true;
        _gameTimer.Start();
        _fpsTimer.Start();
    }

    public void SetTileMapSize(int width, int height)
    {
        _viewportTilesX = width;
        _viewportTilesY = height;

        var oldBuffer = _frameBuffer;
        _frameBuffer = new Bitmap(width * _tileSize, height * _tileSize);
        _bufferGraphics?.Dispose();
        _bufferGraphics = Graphics.FromImage(_frameBuffer);
        _bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        _bufferGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        _gameCanvas.Image = _frameBuffer;
        oldBuffer?.Dispose();
    }

    #endregion

    #region 渲染

    public void RenderFrame(Bitmap frame)
    {
        if (_gameCanvas.InvokeRequired)
        {
            try
            {
                _gameCanvas.BeginInvoke(() =>
                {
                    if (_gameCanvas.Image != frame)
                    {
                        (_gameCanvas.Image as Bitmap)?.Dispose();
                        _gameCanvas.Image = frame;
                    }

                    _gameCanvas.Invalidate();
                });
            }
            catch (ObjectDisposedException)
            {
            }
        }
        else
        {
            if (_gameCanvas.Image != frame)
            {
                (_gameCanvas.Image as Bitmap)?.Dispose();
                _gameCanvas.Image = frame;
            }

            _gameCanvas.Invalidate();
        }
    }

    public void UpdateStatus(string text)
    {
        if (_statusLabel.InvokeRequired)
        {
            try
            {
                _statusLabel.BeginInvoke(() => _statusLabel.Text = text);
            }
            catch (ObjectDisposedException)
            {
            }
        }
        else
        {
            _statusLabel.Text = text;
        }
    }

    public void UpdateInventory(int selectedSlot, IReadOnlyList<(int itemId, int count)> slots)
    {
        if (_inventoryPanel.InvokeRequired)
        {
            return;
        }

        foreach (Control control in _inventoryPanel.Controls)
        {
            if (control is not FlowLayoutPanel layout)
            {
                continue;
            }

            for (var i = 0; i < layout.Controls.Count && i < slots.Count; i++)
            {
                if (layout.Controls[i] is not Panel slotPanel || slotPanel.Tag == null)
                {
                    continue;
                }

                var tag = (dynamic)slotPanel.Tag;
                var label = tag.Label as Label;
                var panel = tag.Panel as Panel;

                if (label == null || panel == null)
                {
                    continue;
                }

                var (itemId, count) = slots[i];
                var name = ItemId.GetName(itemId);
                label.Text = count > 0 ? $"{name}\nx{count}" : $"[{i + 1}]";

                if (i == selectedSlot)
                {
                    panel.BackColor = Color.FromArgb(70, 90, 130);
                    panel.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    panel.BackColor = Color.FromArgb(45, 45, 60);
                    panel.BorderStyle = BorderStyle.FixedSingle;
                }
            }
        }
    }

    #endregion

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _gameTimer?.Dispose();
            _fpsTimer?.Dispose();
            _bufferGraphics?.Dispose();
            _frameBuffer?.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
