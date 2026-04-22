using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.GUI;

public sealed class WinFormsRenderSystem : ISystem
{
    #region 私有字段

    private readonly EcsWorld _world;
    private readonly TerrariaForm _form;
    private EntityId? _playerEntity;
    private EntityId? _tileMapEntity;
    private EntityId? _dayTimeEntity;

    private int _cameraX;
    private int _cameraY;

    private int _tileSize = 8;
    private int _viewportTilesX;
    private int _viewportTilesY;

    private Bitmap? _frameBuffer;
    private Graphics? _bufferGraphics;

    private readonly Dictionary<int, Brush> _tileBrushCache = [];
    private readonly Dictionary<int, Brush> _enemyBrushCache = [];

    private float _smoothCameraX;
    private float _smoothCameraY;

    #endregion

    #region 公开属性

    public SystemPhase Phase => SystemPhase.Render;

    #endregion

    #region 构造函数

    public WinFormsRenderSystem(EcsWorld world, TerrariaForm form)
    {
        _world = world;
        _form = form;
        _viewportTilesX = 80;
        _viewportTilesY = 50;
        _smoothCameraX = 0;
        _smoothCameraY = 0;

        InitializeBrushes();
    }

    #endregion

    #region 画刷初始化

    private void InitializeBrushes()
    {
        _tileBrushCache[TileId.Air] = new SolidBrush(Color.FromArgb(135, 206, 235));
        _tileBrushCache[TileId.Dirt] = new SolidBrush(Color.FromArgb(139, 90, 43));
        _tileBrushCache[TileId.Stone] = new SolidBrush(Color.FromArgb(128, 128, 128));
        _tileBrushCache[TileId.Grass] = new SolidBrush(Color.FromArgb(34, 139, 34));
        _tileBrushCache[TileId.Sand] = new SolidBrush(Color.FromArgb(210, 180, 140));
        _tileBrushCache[TileId.Water] = new SolidBrush(Color.FromArgb(30, 144, 255));
        _tileBrushCache[TileId.Wood] = new SolidBrush(Color.FromArgb(139, 69, 19));
        _tileBrushCache[TileId.Leaf] = new SolidBrush(Color.FromArgb(0, 100, 0));
        _tileBrushCache[TileId.IronOre] = new SolidBrush(Color.FromArgb(160, 82, 45));
        _tileBrushCache[TileId.GoldOre] = new SolidBrush(Color.FromArgb(255, 215, 0));
        _tileBrushCache[TileId.CopperOre] = new SolidBrush(Color.FromArgb(184, 115, 51));
        _tileBrushCache[TileId.Snow] = new SolidBrush(Color.FromArgb(240, 240, 255));
        _tileBrushCache[TileId.Ice] = new SolidBrush(Color.FromArgb(173, 216, 230));
        _tileBrushCache[TileId.Clay] = new SolidBrush(Color.FromArgb(160, 82, 45));
        _tileBrushCache[TileId.Boundary] = new SolidBrush(Color.FromArgb(40, 40, 40));

        _enemyBrushCache[EnemyType.Slime] = new SolidBrush(Color.FromArgb(0, 200, 0));
        _enemyBrushCache[EnemyType.Zombie] = new SolidBrush(Color.FromArgb(0, 150, 0));
        _enemyBrushCache[EnemyType.DemonEye] = new SolidBrush(Color.FromArgb(200, 0, 0));
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<PlayerTag, TilePosition>())
        {
            _playerEntity = entity;
            break;
        }

        foreach (var entity in _world.QueryEntities<TileMapData>())
        {
            _tileMapEntity = entity;
            break;
        }

        foreach (var entity in _world.QueryEntities<DayTime>())
        {
            _dayTimeEntity = entity;
            break;
        }

        ResizeViewport(_viewportTilesX, _viewportTilesY);
    }

    public void Update(float delta)
    {
        if (!_playerEntity.HasValue || !_tileMapEntity.HasValue)
        {
            return;
        }

        var playerPos = _world.GetComponent<TilePosition>(_playerEntity.Value);
        var tileMap = _world.GetComponent<TileMapData>(_tileMapEntity.Value);

        UpdateCamera(playerPos, delta);
        RenderWorld(in tileMap);
        UpdateStatusBar(playerPos, in tileMap);
        UpdateInventoryDisplay();
    }

    public void Shutdown()
    {
        _frameBuffer?.Dispose();
        _bufferGraphics?.Dispose();

        foreach (var brush in _tileBrushCache.Values)
        {
            brush.Dispose();
        }

        foreach (var brush in _enemyBrushCache.Values)
        {
            brush.Dispose();
        }
    }

    #endregion

    #region 相机

    private void UpdateCamera(TilePosition playerPos, float delta)
    {
        var targetX = playerPos.X - _viewportTilesX / 2.0f;
        var targetY = playerPos.Y - _viewportTilesY / 2.0f;

        var smoothing = 1.0f - MathF.Pow(0.05f, delta);
        _smoothCameraX += (targetX - _smoothCameraX) * smoothing;
        _smoothCameraY += (targetY - _smoothCameraY) * smoothing;

        _cameraX = (int)MathF.Floor(_smoothCameraX);
        _cameraY = (int)MathF.Floor(_smoothCameraY);
    }

    #endregion

    #region 渲染

    private void ResizeViewport(int tilesX, int tilesY)
    {
        _viewportTilesX = tilesX;
        _viewportTilesY = tilesY;

        _frameBuffer?.Dispose();
        _bufferGraphics?.Dispose();

        _frameBuffer = new Bitmap(tilesX * _tileSize, tilesY * _tileSize);
        _bufferGraphics = Graphics.FromImage(_frameBuffer);
        _bufferGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        _bufferGraphics.PixelOffsetMode = PixelOffsetMode.Half;

        _form.SetTileMapSize(tilesX, tilesY);
    }

    private void RenderWorld(in TileMapData tileMap)
    {
        if (_bufferGraphics == null || _frameBuffer == null)
        {
            return;
        }

        var daylight = 1.0f;
        if (_dayTimeEntity.HasValue)
        {
            var dayTime = _world.GetComponent<DayTime>(_dayTimeEntity.Value);
            daylight = dayTime.Daylight;
        }

        _bufferGraphics.Clear(GetSkyColor(daylight));

        RenderTiles(in tileMap, daylight);
        RenderEntities(in tileMap);
        RenderPlayer();
        RenderMinimap(in tileMap);

        _form.RenderFrame(_frameBuffer);
    }

    private Color GetSkyColor(float daylight)
    {
        var dayR = 135;
        var dayG = 206;
        var dayB = 235;

        var nightR = 10;
        var nightG = 10;
        var nightB = 30;

        var r = (int)(nightR + (dayR - nightR) * daylight);
        var g = (int)(nightG + (dayG - nightG) * daylight);
        var b = (int)(nightB + (dayB - nightB) * daylight);

        return Color.FromArgb(r, g, b);
    }

    private void RenderTiles(in TileMapData tileMap, float daylight)
    {
        for (var vy = 0; vy < _viewportTilesY; vy++)
        {
            for (var vx = 0; vx < _viewportTilesX; vx++)
            {
                var worldX = _cameraX + vx;
                var worldY = _cameraY + vy;

                var tile = tileMap.GetTile(worldX, worldY);
                if (tile == TileId.Air)
                {
                    if (worldY > tileMap.Height * 0.3)
                    {
                        var undergroundColor = ApplyDaylight(Color.FromArgb(80, 60, 40), daylight * 0.3f);
                        _bufferGraphics?.FillRectangle(
                            new SolidBrush(undergroundColor),
                            vx * _tileSize, vy * _tileSize, _tileSize, _tileSize);
                    }

                    continue;
                }

                var baseBrush = _tileBrushCache.TryGetValue(tile, out var b) ? b : Brushes.Magenta;
                var color = ((SolidBrush)baseBrush).Color;
                var litColor = ApplyDaylight(color, daylight);

                var rect = new Rectangle(vx * _tileSize, vy * _tileSize, _tileSize, _tileSize);
                _bufferGraphics?.FillRectangle(new SolidBrush(litColor), rect);

                if (tile == TileId.Grass && _tileSize >= 6)
                {
                    var grassTopColor = ApplyDaylight(Color.FromArgb(50, 180, 50), daylight);
                    _bufferGraphics?.FillRectangle(
                        new SolidBrush(grassTopColor),
                        vx * _tileSize, vy * _tileSize, _tileSize, 2);
                }
                else if (tile == TileId.IronOre && _tileSize >= 6)
                {
                    var oreColor = ApplyDaylight(Color.FromArgb(200, 180, 160), daylight);
                    _bufferGraphics?.FillRectangle(
                        new SolidBrush(oreColor),
                        vx * _tileSize + 2, vy * _tileSize + 2, 3, 3);
                }
                else if (tile == TileId.GoldOre && _tileSize >= 6)
                {
                    var oreColor = ApplyDaylight(Color.FromArgb(255, 240, 100), daylight);
                    _bufferGraphics?.FillRectangle(
                        new SolidBrush(oreColor),
                        vx * _tileSize + 2, vy * _tileSize + 2, 3, 3);
                }
                else if (tile == TileId.CopperOre && _tileSize >= 6)
                {
                    var oreColor = ApplyDaylight(Color.FromArgb(220, 140, 60), daylight);
                    _bufferGraphics?.FillRectangle(
                        new SolidBrush(oreColor),
                        vx * _tileSize + 2, vy * _tileSize + 2, 3, 3);
                }
            }
        }
    }

    private void RenderEntities(in TileMapData tileMap)
    {
        foreach (var entity in _world.QueryEntities<EnemyTag, TilePosition>())
        {
            var pos = _world.GetComponent<TilePosition>(entity);
            var tag = _world.GetComponent<EnemyTag>(entity);

            var vx = pos.X - _cameraX;
            var vy = pos.Y - _cameraY;

            if (vx < -2 || vx > _viewportTilesX + 2 || vy < -2 || vy > _viewportTilesY + 2)
            {
                continue;
            }

            var brush = _enemyBrushCache.TryGetValue(tag.EnemyType, out var b) ? b : Brushes.Red;
            var pixelX = vx * _tileSize;
            var pixelY = vy * _tileSize;

            switch (tag.EnemyType)
            {
                case EnemyType.Slime:
                    _bufferGraphics?.FillEllipse(brush,
                        pixelX - 2, pixelY + 2, _tileSize + 4, _tileSize - 2);
                    var eyeColor = Color.FromArgb(255, 255, 255);
                    _bufferGraphics?.FillEllipse(new SolidBrush(eyeColor),
                        pixelX + 1, pixelY + 2, 2, 2);
                    _bufferGraphics?.FillEllipse(new SolidBrush(eyeColor),
                        pixelX + 5, pixelY + 2, 2, 2);
                    break;

                case EnemyType.Zombie:
                    _bufferGraphics?.FillRectangle(brush,
                        pixelX, pixelY, _tileSize, _tileSize + 2);
                    var headColor = Color.FromArgb(100, 140, 80);
                    _bufferGraphics?.FillRectangle(new SolidBrush(headColor),
                        pixelX + 1, pixelY - 2, _tileSize - 2, 4);
                    _bufferGraphics?.FillEllipse(new SolidBrush(Color.Red),
                        pixelX + 2, pixelY - 1, 1, 1);
                    _bufferGraphics?.FillEllipse(new SolidBrush(Color.Red),
                        pixelX + 5, pixelY - 1, 1, 1);
                    break;

                case EnemyType.DemonEye:
                    _bufferGraphics?.FillEllipse(brush,
                        pixelX - 1, pixelY + 1, _tileSize + 2, _tileSize - 2);
                    _bufferGraphics?.FillEllipse(new SolidBrush(Color.White),
                        pixelX + 2, pixelY + 3, 4, 3);
                    _bufferGraphics?.FillEllipse(new SolidBrush(Color.Black),
                        pixelX + 3, pixelY + 3, 2, 2);
                    break;

                default:
                    _bufferGraphics?.FillRectangle(brush,
                        pixelX, pixelY, _tileSize, _tileSize);
                    break;
            }

            if (_world.HasComponent<Health>(entity))
            {
                var health = _world.GetComponent<Health>(entity);
                var healthPct = (float)health.Current / health.Max;
                var barWidth = _tileSize + 4;
                var barY = pixelY - 4;

                _bufferGraphics?.FillRectangle(Brushes.DarkRed,
                    pixelX - 2, barY, barWidth, 2);
                _bufferGraphics?.FillRectangle(Brushes.LimeGreen,
                    pixelX - 2, barY, barWidth * healthPct, 2);
            }
        }
    }

    private void RenderPlayer()
    {
        if (!_playerEntity.HasValue)
        {
            return;
        }

        var pos = _world.GetComponent<TilePosition>(_playerEntity.Value);
        var vx = pos.X - _cameraX;
        var vy = pos.Y - _cameraY;

        var pixelX = vx * _tileSize;
        var pixelY = vy * _tileSize;

        _bufferGraphics?.FillRectangle(Brushes.Gold,
            pixelX + 1, pixelY, _tileSize - 2, _tileSize - 2);

        var skinColor = new SolidBrush(Color.FromArgb(255, 220, 180));
        _bufferGraphics?.FillRectangle(skinColor,
            pixelX + 2, pixelY - 3, _tileSize - 4, 4);

        var eyeColor = new SolidBrush(Color.FromArgb(50, 50, 200));
        _bufferGraphics?.FillRectangle(eyeColor,
            pixelX + 2, pixelY - 2, 1, 1);
        _bufferGraphics?.FillRectangle(eyeColor,
            pixelX + 5, pixelY - 2, 1, 1);

        if (_world.HasComponent<Health>(_playerEntity.Value))
        {
            var health = _world.GetComponent<Health>(_playerEntity.Value);
            var healthPct = (float)health.Current / health.Max;
            var barWidth = _tileSize + 6;
            var barY = pixelY - 8;

            _bufferGraphics?.FillRectangle(Brushes.DarkRed,
                pixelX - 3, barY, barWidth, 3);
            _bufferGraphics?.FillRectangle(Brushes.LimeGreen,
                pixelX - 3, barY, barWidth * healthPct, 3);
        }
    }

    private void RenderMinimap(in TileMapData tileMap)
    {
        var minimapWidth = 80;
        var minimapHeight = 40;
        var minimapX = _frameBuffer!.Width - minimapWidth - 4;
        var minimapY = 4;

        _bufferGraphics?.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 0, 128)),
            minimapX - 1, minimapY - 1, minimapWidth + 2, minimapHeight + 2);

        var scaleX = (float)tileMap.Width / minimapWidth;
        var scaleY = (float)tileMap.Height / minimapHeight;

        using var minimapBmp = new Bitmap(minimapWidth, minimapHeight);
        for (var mx = 0; mx < minimapWidth; mx++)
        {
            for (var my = 0; my < minimapHeight; my++)
            {
                var tx = (int)(mx * scaleX);
                var ty = (int)(my * scaleY);
                var tile = tileMap.GetTile(tx, ty);

                if (tile == TileId.Air)
                {
                    continue;
                }

                var brush = _tileBrushCache.TryGetValue(tile, out var b) ? b : null;
                if (brush is SolidBrush sb)
                {
                    minimapBmp.SetPixel(mx, my, sb.Color);
                }
            }
        }

        _bufferGraphics?.DrawImage(minimapBmp, minimapX, minimapY);

        if (_playerEntity.HasValue)
        {
            var pos = _world.GetComponent<TilePosition>(_playerEntity.Value);
            var px = minimapX + (int)(pos.X / scaleX);
            var py = minimapY + (int)(pos.Y / scaleY);
            _bufferGraphics?.FillRectangle(Brushes.White, px - 1, py - 1, 3, 3);
        }
    }

    private static Color ApplyDaylight(Color color, float daylight)
    {
        var factor = 0.3f + 0.7f * daylight;
        var r = (int)(color.R * factor);
        var g = (int)(color.G * factor);
        var b = (int)(color.B * factor);
        return Color.FromArgb(Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
    }

    #endregion

    #region UI 更新

    private void UpdateStatusBar(TilePosition playerPos, in TileMapData tileMap)
    {
        var dayStr = "白天";
        if (_dayTimeEntity.HasValue)
        {
            var dayTime = _world.GetComponent<DayTime>(_dayTimeEntity.Value);
            var hour = (int)(dayTime.TimeOfDay * 24);
            dayStr = dayTime.IsDay ? $"☀ 白天 {hour:D2}:00" : $"🌙 夜晚 {hour:D2}:00";
        }

        var health = _world.HasComponent<Health>(_playerEntity!.Value)
            ? _world.GetComponent<Health>(_playerEntity.Value)
            : new Health(0, 100);

        var tile = tileMap.GetTile(playerPos.X, playerPos.Y + 1);
        var groundName = TileId.GetName(tile);

        _form.UpdateStatus(
            $"  位置: ({playerPos.X}, {playerPos.Y})  |  {dayStr}  |  ❤ HP: {health.Current}/{health.Max}  |  脚下: {groundName}  |  WASD:移动 空格:跳跃 J/K/I/S:挖掘 E:攻击");
    }

    private void UpdateInventoryDisplay()
    {
        if (!_playerEntity.HasValue || !_world.HasComponent<Inventory>(_playerEntity.Value))
        {
            return;
        }

        var inventory = _world.GetComponent<Inventory>(_playerEntity.Value);
        var slots = new List<(int, int)>();

        for (var i = 0; i < Math.Min(inventory.Slots, 10); i++)
        {
            slots.Add((inventory.ItemIds[i], inventory.ItemCounts[i]));
        }

        _form.UpdateInventory(inventory.SelectedSlot, slots);
    }

    #endregion
}
