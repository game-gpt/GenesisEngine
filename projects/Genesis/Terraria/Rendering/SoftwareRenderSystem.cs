using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;
using Gnosis.Rendering.Backends.Software;
using Gnosis.Rendering.Shader;
using Gnosis.Rendering.TwoD;

namespace Genesis.Terraria.Rendering;

public sealed class SoftwareRenderSystem : ISystem
{
    #region 私有字段

    private readonly EcsWorld _world;
    private readonly SoftwareRenderer _renderer;
    private readonly Camera2D _camera;
    private readonly TileColorTable _colorTable;
    private readonly IShaderModule _tileShader;
    private readonly IShaderModule _spriteShader;

    private EntityId? _playerEntity;
    private EntityId? _tileMapEntity;
    private EntityId? _dayTimeEntity;

    private int _tilePixelSize;
    private int _viewportTilesX;
    private int _viewportTilesY;

    private float _smoothCameraX;
    private float _smoothCameraY;

    private int _frameCount;

    #endregion

    #region 公开属性

    public SystemPhase Phase => SystemPhase.Render;
    public SoftwareRenderer Renderer => _renderer;
    public Camera2D Camera => _camera;
    public int FrameCount => _frameCount;
    public int ViewportTilesX => _viewportTilesX;
    public int ViewportTilesY => _viewportTilesY;

    #endregion

    #region 构造函数

    public SoftwareRenderSystem(EcsWorld world, int screenWidth = 640, int screenHeight = 480, int tilePixelSize = 8)
    {
        _world = world;
        _tilePixelSize = tilePixelSize;
        _viewportTilesX = screenWidth / tilePixelSize;
        _viewportTilesY = screenHeight / tilePixelSize;

        _renderer = new SoftwareRenderer(screenWidth, screenHeight);
        _camera = new Camera2D { ViewportSize = [screenWidth, screenHeight] };
        _colorTable = new TileColorTable();
        _tileShader = TileShader.CreateTileShader();
        _spriteShader = TileShader.CreateSpriteShader();

        _smoothCameraX = 0;
        _smoothCameraY = 0;
        _frameCount = 0;
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

        var daylight = 1.0f;
        if (_dayTimeEntity.HasValue)
        {
            var dayTime = _world.GetComponent<DayTime>(_dayTimeEntity.Value);
            daylight = dayTime.Daylight;
        }

        var skyColor = TileColorTable.ApplyDaylight((0.53f, 0.81f, 0.92f), daylight);
        _renderer.Clear(skyColor.r, skyColor.g, skyColor.b);

        RenderTiles(in tileMap, daylight);
        RenderEntities(daylight);
        RenderPlayer(daylight);
        RenderMinimap(in tileMap);

        _frameCount++;
    }

    public void Shutdown()
    {
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

        _camera.Position = [_smoothCameraX, _smoothCameraY];
    }

    #endregion

    #region 瓦片渲染

    private void RenderTiles(in TileMapData tileMap, float daylight)
    {
        var camX = (int)MathF.Floor(_smoothCameraX);
        var camY = (int)MathF.Floor(_smoothCameraY);
        var colorBuffer = _renderer.GetColorBuffer();

        for (var vy = 0; vy < _viewportTilesY; vy++)
        {
            for (var vx = 0; vx < _viewportTilesX; vx++)
            {
                var worldX = camX + vx;
                var worldY = camY + vy;

                var tile = tileMap.GetTile(worldX, worldY);

                if (tile == TileId.Air)
                {
                    if (worldY > tileMap.Height * 0.3)
                    {
                        var underground = TileColorTable.ApplyDaylight((0.31f, 0.24f, 0.16f), daylight * 0.3f);
                        FillTile(colorBuffer, vx, vy, underground);
                    }

                    continue;
                }

                var baseColor = _colorTable.GetTileColor(tile);
                var litColor = TileColorTable.ApplyDaylight(baseColor, daylight);
                FillTile(colorBuffer, vx, vy, litColor);

                if (tile == TileId.Grass)
                {
                    var topColor = TileColorTable.ApplyDaylight((0.20f, 0.71f, 0.20f), daylight);
                    FillTileTop(colorBuffer, vx, vy, topColor, 2);
                }
                else if (tile == TileId.IronOre || tile == TileId.GoldOre || tile == TileId.CopperOre)
                {
                    var oreColor = tile switch
                    {
                        TileId.IronOre => TileColorTable.ApplyDaylight((0.78f, 0.71f, 0.63f), daylight),
                        TileId.GoldOre => TileColorTable.ApplyDaylight((1.0f, 0.94f, 0.39f), daylight),
                        TileId.CopperOre => TileColorTable.ApplyDaylight((0.86f, 0.55f, 0.24f), daylight),
                        _ => litColor
                    };
                    FillTileCenter(colorBuffer, vx, vy, oreColor, 3);
                }
            }
        }
    }

    private void FillTile(Texture buffer, int tileVX, int tileVY, (float r, float g, float b) color)
    {
        var br = (byte)(color.r * 255);
        var bg = (byte)(color.g * 255);
        var bb = (byte)(color.b * 255);

        var startX = tileVX * _tilePixelSize;
        var startY = tileVY * _tilePixelSize;

        for (var py = 0; py < _tilePixelSize; py++)
        {
            for (var px = 0; px < _tilePixelSize; px++)
            {
                buffer.SetPixel(startX + px, startY + py, br, bg, bb);
            }
        }
    }

    private void FillTileTop(Texture buffer, int tileVX, int tileVY, (float r, float g, float b) color, int height)
    {
        var br = (byte)(color.r * 255);
        var bg = (byte)(color.g * 255);
        var bb = (byte)(color.b * 255);

        var startX = tileVX * _tilePixelSize;
        var startY = tileVY * _tilePixelSize;

        for (var py = 0; py < Math.Min(height, _tilePixelSize); py++)
        {
            for (var px = 0; px < _tilePixelSize; px++)
            {
                buffer.SetPixel(startX + px, startY + py, br, bg, bb);
            }
        }
    }

    private void FillTileCenter(Texture buffer, int tileVX, int tileVY, (float r, float g, float b) color, int size)
    {
        var br = (byte)(color.r * 255);
        var bg = (byte)(color.g * 255);
        var bb = (byte)(color.b * 255);

        var startX = tileVX * _tilePixelSize + (_tilePixelSize - size) / 2;
        var startY = tileVY * _tilePixelSize + (_tilePixelSize - size) / 2;

        for (var py = 0; py < size; py++)
        {
            for (var px = 0; px < size; px++)
            {
                buffer.SetPixel(startX + px, startY + py, br, bg, bb);
            }
        }
    }

    #endregion

    #region 实体渲染

    private void RenderEntities(float daylight)
    {
        var camX = (int)MathF.Floor(_smoothCameraX);
        var camY = (int)MathF.Floor(_smoothCameraY);
        var colorBuffer = _renderer.GetColorBuffer();

        foreach (var entity in _world.QueryEntities<EnemyTag, TilePosition>())
        {
            var pos = _world.GetComponent<TilePosition>(entity);
            var tag = _world.GetComponent<EnemyTag>(entity);

            var vx = pos.X - camX;
            var vy = pos.Y - camY;

            if (vx < -2 || vx > _viewportTilesX + 2 || vy < -2 || vy > _viewportTilesY + 2)
            {
                continue;
            }

            var enemyColor = _colorTable.GetEnemyColor(tag.EnemyType);
            var litColor = TileColorTable.ApplyDaylight(enemyColor, daylight);

            var pixelX = vx * _tilePixelSize;
            var pixelY = vy * _tilePixelSize;
            var s = _tilePixelSize;

            var br = (byte)(litColor.r * 255);
            var bg = (byte)(litColor.g * 255);
            var bb = (byte)(litColor.b * 255);

            switch (tag.EnemyType)
            {
                case EnemyType.Slime:
                    for (var py = 2; py < s; py++)
                    {
                        for (var px = -1; px < s + 1; px++)
                        {
                            colorBuffer.SetPixel(pixelX + px, pixelY + py, br, bg, bb);
                        }
                    }

                    colorBuffer.SetPixel(pixelX + 1, pixelY + 3, 255, 255, 255);
                    colorBuffer.SetPixel(pixelX + s - 2, pixelY + 3, 255, 255, 255);
                    break;

                case EnemyType.Zombie:
                    for (var py = 0; py < s + 2; py++)
                    {
                        for (var px = 0; px < s; px++)
                        {
                            colorBuffer.SetPixel(pixelX + px, pixelY + py, br, bg, bb);
                        }
                    }

                    colorBuffer.SetPixel(pixelX + 2, pixelY + 1, 255, 0, 0);
                    colorBuffer.SetPixel(pixelX + s - 3, pixelY + 1, 255, 0, 0);
                    break;

                case EnemyType.DemonEye:
                    for (var py = 1; py < s - 1; py++)
                    {
                        for (var px = -1; px < s + 1; px++)
                        {
                            colorBuffer.SetPixel(pixelX + px, pixelY + py, br, bg, bb);
                        }
                    }

                    for (var px = 2; px < s - 2; px++)
                    {
                        for (var py = 2; py < s - 2; py++)
                        {
                            colorBuffer.SetPixel(pixelX + px, pixelY + py, 255, 255, 255);
                        }
                    }

                    colorBuffer.SetPixel(pixelX + 3, pixelY + 3, 0, 0, 0);
                    colorBuffer.SetPixel(pixelX + s - 4, pixelY + 3, 0, 0, 0);
                    break;
            }

            if (_world.HasComponent<Health>(entity))
            {
                var health = _world.GetComponent<Health>(entity);
                RenderHealthBar(colorBuffer, pixelX, pixelY - 4, s + 4, health.Current, health.Max);
            }
        }
    }

    private void RenderPlayer(float daylight)
    {
        if (!_playerEntity.HasValue)
        {
            return;
        }

        var pos = _world.GetComponent<TilePosition>(_playerEntity.Value);
        var camX = (int)MathF.Floor(_smoothCameraX);
        var camY = (int)MathF.Floor(_smoothCameraY);
        var colorBuffer = _renderer.GetColorBuffer();

        var vx = pos.X - camX;
        var vy = pos.Y - camY;
        var pixelX = vx * _tilePixelSize;
        var pixelY = vy * _tilePixelSize;
        var s = _tilePixelSize;

        for (var py = 0; py < s - 2; py++)
        {
            for (var px = 1; px < s - 1; px++)
            {
                colorBuffer.SetPixel(pixelX + px, pixelY + py, 255, 215, 0);
            }
        }

        for (var px = 2; px < s - 2; px++)
        {
            for (var py = -3; py < 1; py++)
            {
                colorBuffer.SetPixel(pixelX + px, pixelY + py, 255, 220, 180);
            }
        }

        colorBuffer.SetPixel(pixelX + 2, pixelY - 2, 50, 50, 200);
        colorBuffer.SetPixel(pixelX + s - 3, pixelY - 2, 50, 50, 200);

        if (_world.HasComponent<Health>(_playerEntity.Value))
        {
            var health = _world.GetComponent<Health>(_playerEntity.Value);
            RenderHealthBar(colorBuffer, pixelX - 2, pixelY - 8, s + 6, health.Current, health.Max);
        }
    }

    private static void RenderHealthBar(Texture buffer, int x, int y, int width, int current, int max)
    {
        var pct = (float)current / max;

        for (var px = 0; px < width; px++)
        {
            buffer.SetPixel(x + px, y, 139, 0, 0);
            buffer.SetPixel(x + px, y + 1, 139, 0, 0);

            if (px < width * pct)
            {
                buffer.SetPixel(x + px, y, 50, 205, 50);
                buffer.SetPixel(x + px, y + 1, 50, 205, 50);
            }
        }
    }

    #endregion

    #region 小地图

    private void RenderMinimap(in TileMapData tileMap)
    {
        var colorBuffer = _renderer.GetColorBuffer();
        var minimapWidth = 80;
        var minimapHeight = 40;
        var minimapX = _renderer.Width - minimapWidth - 4;
        var minimapY = 4;

        for (var px = -1; px <= minimapWidth; px++)
        {
            colorBuffer.SetPixel(minimapX + px, minimapY - 1, 0, 0, 0);
            colorBuffer.SetPixel(minimapX + px, minimapY + minimapHeight, 0, 0, 0);
        }

        for (var py = -1; py <= minimapHeight; py++)
        {
            colorBuffer.SetPixel(minimapX - 1, minimapY + py, 0, 0, 0);
            colorBuffer.SetPixel(minimapX + minimapWidth, minimapY + py, 0, 0, 0);
        }

        var scaleX = (float)tileMap.Width / minimapWidth;
        var scaleY = (float)tileMap.Height / minimapHeight;

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

                var c = _colorTable.GetTileColor(tile);
                colorBuffer.SetPixel(minimapX + mx, minimapY + my,
                    (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
            }
        }

        if (_playerEntity.HasValue)
        {
            var pos = _world.GetComponent<TilePosition>(_playerEntity.Value);
            var px = minimapX + (int)(pos.X / scaleX);
            var py = minimapY + (int)(pos.Y / scaleY);

            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    colorBuffer.SetPixel(px + dx, py + dy, 255, 255, 255);
                }
            }
        }
    }

    #endregion
}
