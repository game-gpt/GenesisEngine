using Genesis.Game.Terraria.Game;
using ECSWorld = Gnosis.ECS.World.World;

namespace Genesis.Game.Terraria.Rendering;

public sealed class Terraria2DRenderer
{
    private readonly ECSWorld _world;
    private readonly TileMap _tileMap;
    private readonly int _width;
    private readonly int _height;
    private readonly byte[] _framebuffer;
    private readonly TileColorTable _colorTable;

    private const int TilePixelSize = 4;

    private float _cameraX;
    private float _cameraY;

    public Terraria2DRenderer(ECSWorld world, TileMap tileMap, int width, int height)
    {
        _world = world;
        _tileMap = tileMap;
        _width = width;
        _height = height;
        _framebuffer = new byte[width * height * 4];
        _colorTable = new TileColorTable();
    }

    public byte[] GetFramebuffer() => _framebuffer;

    public void Render()
    {
        UpdateCamera();
        RenderTiles();
        RenderEntities();
        ApplyDayNightLighting();
    }

    private void UpdateCamera()
    {
        var players = _world.CreateQuery()
            .All<PlayerTag>()
            .All<TilePosition>()
            .All<CameraFollow>()
            .Build();

        foreach (var entityId in players)
        {
            var pos = _world.GetComponent<TilePosition>(entityId);
            var cam = _world.GetComponent<CameraFollow>(entityId);

            var targetX = pos.X * TilePixelSize - _width / 2.0f + cam.OffsetX * TilePixelSize;
            var targetY = pos.Y * TilePixelSize - _height / 2.0f + cam.OffsetY * TilePixelSize;

            _cameraX += (targetX - _cameraX) * cam.Smoothing;
            _cameraY += (targetY - _cameraY) * cam.Smoothing;
        }
    }

    private void RenderTiles()
    {
        var startTileX = (int)(_cameraX / TilePixelSize) - 1;
        var startTileY = (int)(_cameraY / TilePixelSize) - 1;
        var endTileX = startTileX + _width / TilePixelSize + 2;
        var endTileY = startTileY + _height / TilePixelSize + 2;

        for (var ty = startTileY; ty <= endTileY; ty++)
        {
            for (var tx = startTileX; tx <= endTileX; tx++)
            {
                var tileId = _tileMap.GetTile(tx, ty);
                var color = _colorTable.GetTileColor(tileId);

                var screenX = tx * TilePixelSize - (int)_cameraX;
                var screenY = ty * TilePixelSize - (int)_cameraY;

                FillRect(screenX, screenY, TilePixelSize, TilePixelSize, color.R, color.G, color.B);
            }
        }
    }

    private void RenderEntities()
    {
        RenderPlayers();
        RenderEnemies();
    }

    private void RenderPlayers()
    {
        var players = _world.CreateQuery()
            .All<PlayerTag>()
            .All<TilePosition>()
            .Build();

        foreach (var entityId in players)
        {
            var pos = _world.GetComponent<TilePosition>(entityId);
            var screenX = pos.X * TilePixelSize - (int)_cameraX;
            var screenY = pos.Y * TilePixelSize - (int)_cameraY;

            FillRect(screenX - 1, screenY - 2, TilePixelSize + 2, TilePixelSize + 4, 0.2f, 0.6f, 1.0f);
        }
    }

    private void RenderEnemies()
    {
        var enemies = _world.CreateQuery()
            .All<EnemyTag>()
            .All<TilePosition>()
            .Build();

        foreach (var entityId in enemies)
        {
            var pos = _world.GetComponent<TilePosition>(entityId);
            var enemyTag = _world.GetComponent<EnemyTag>(entityId);

            var color = _colorTable.GetEnemyColor(enemyTag.EnemyType);
            var screenX = pos.X * TilePixelSize - (int)_cameraX;
            var screenY = pos.Y * TilePixelSize - (int)_cameraY;

            FillRect(screenX - 1, screenY - 1, TilePixelSize + 2, TilePixelSize + 2, color.R, color.G, color.B);
        }
    }

    private void ApplyDayNightLighting()
    {
        var daytimeQuery = _world.CreateQuery()
            .All<DayTime>()
            .Build();

        float ambient = 1.0f;
        foreach (var entityId in daytimeQuery)
        {
            var daytime = _world.GetComponent<DayTime>(entityId);
            var t = daytime.TimeOfDay;

            if (t < 0.25f)
            {
                ambient = _colorTable.AmbientMin + _colorTable.AmbientRange * (t / 0.25f);
            }
            else if (t < 0.5f)
            {
                ambient = 1.0f;
            }
            else if (t < 0.75f)
            {
                ambient = 1.0f - _colorTable.AmbientRange * ((t - 0.5f) / 0.25f);
                ambient = Math.Max(ambient, _colorTable.AmbientMin);
            }
            else
            {
                ambient = _colorTable.AmbientMin;
            }
        }

        if (ambient >= 1.0f)
        {
            return;
        }

        for (var i = 0; i < _framebuffer.Length; i += 4)
        {
            _framebuffer[i] = (byte)(_framebuffer[i] * ambient);
            _framebuffer[i + 1] = (byte)(_framebuffer[i + 1] * ambient);
            _framebuffer[i + 2] = (byte)(_framebuffer[i + 2] * ambient);
        }
    }

    private void FillRect(int x, int y, int w, int h, float r, float g, float b)
    {
        var br = (byte)(r * 255);
        var bg = (byte)(g * 255);
        var bb = (byte)(b * 255);

        for (var dy = 0; dy < h; dy++)
        {
            for (var dx = 0; dx < w; dx++)
            {
                var px = x + dx;
                var py = y + dy;

                if (px < 0 || px >= _width || py < 0 || py >= _height)
                {
                    continue;
                }

                var offset = (py * _width + px) * 4;
                _framebuffer[offset] = bb;
                _framebuffer[offset + 1] = bg;
                _framebuffer[offset + 2] = br;
                _framebuffer[offset + 3] = 255;
            }
        }
    }
}
