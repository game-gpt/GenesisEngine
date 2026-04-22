using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class ConsoleRenderSystem : ISystem
{
    private readonly EcsWorld _world;
    private EntityId? _playerEntity;
    private EntityId? _tileMapEntity;
    private EntityId? _dayTimeEntity;

    private int _viewportWidth;
    private int _viewportHeight;
    private char[] _frameBuffer;
    private int[] _colorBuffer;

    private int _cameraX;
    private int _cameraY;

    private float _renderTimer;

    public SystemPhase Phase => SystemPhase.Render;

    public ConsoleRenderSystem(EcsWorld world)
    {
        _world = world;
        _viewportWidth = 50;
        _viewportHeight = 20;
        _frameBuffer = new char[_viewportWidth * _viewportHeight];
        _colorBuffer = new int[_viewportWidth * _viewportHeight];
        _renderTimer = 0;
    }

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

        try
        {
            Console.Clear();
            Console.CursorVisible = false;
        }
        catch (IOException)
        {
        }
    }

    public void Update(float delta)
    {
        _renderTimer += delta;

        if (_renderTimer < 0.05f)
        {
            return;
        }

        _renderTimer = 0;

        if (!_playerEntity.HasValue || !_tileMapEntity.HasValue)
        {
            return;
        }

        var playerPos = _world.GetComponent<TilePosition>(_playerEntity.Value);
        var tileMap = _world.GetComponent<TileMapData>(_tileMapEntity.Value);

        UpdateCamera(playerPos, in tileMap);
        ClearFrameBuffer();
        RenderTileMap(in tileMap);
        RenderEntities();
        RenderPlayer(playerPos);
        PresentFrame(playerPos, in tileMap);
    }

    private void UpdateCamera(TilePosition playerPos, in TileMapData tileMap)
    {
        var targetX = playerPos.X - _viewportWidth / 2;
        var targetY = playerPos.Y - _viewportHeight / 2;

        _cameraX = Math.Clamp(targetX, 0, Math.Max(0, tileMap.Width - _viewportWidth));
        _cameraY = Math.Clamp(targetY, 0, Math.Max(0, tileMap.Height - _viewportHeight));
    }

    private void ClearFrameBuffer()
    {
        Array.Fill(_frameBuffer, ' ');
        Array.Fill(_colorBuffer, 0);
    }

    private void RenderTileMap(in TileMapData tileMap)
    {
        var daylight = 1.0f;
        if (_dayTimeEntity.HasValue)
        {
            var dayTime = _world.GetComponent<DayTime>(_dayTimeEntity.Value);
            daylight = dayTime.Daylight;
        }

        for (var vy = 0; vy < _viewportHeight; vy++)
        {
            for (var vx = 0; vx < _viewportWidth; vx++)
            {
                var worldX = _cameraX + vx;
                var worldY = _cameraY + vy;

                var tile = tileMap.GetTile(worldX, worldY);
                var ch = TileId.GetDisplayChar(tile);
                var color = GetTileColor(tile, worldY, in tileMap, daylight);

                var idx = vy * _viewportWidth + vx;
                _frameBuffer[idx] = ch;
                _colorBuffer[idx] = color;
            }
        }
    }

    private static int GetTileColor(int tile, int worldY, in TileMapData tileMap, float daylight)
    {
        var depth = worldY / (float)tileMap.Height;

        return tile switch
        {
            TileId.Air => daylight > 0.5f
                ? GetSkyColor(depth)
                : GetNightSkyColor(depth),
            TileId.Dirt => ApplyDaylight(0x6B4226, daylight),
            TileId.Stone => ApplyDaylight(0x808080, daylight),
            TileId.Grass => ApplyDaylight(0x228B22, daylight),
            TileId.Sand => ApplyDaylight(0xC2B280, daylight),
            TileId.Water => ApplyDaylight(0x1E90FF, daylight),
            TileId.Wood => ApplyDaylight(0x8B4513, daylight),
            TileId.Leaf => ApplyDaylight(0x2E8B57, daylight),
            TileId.IronOre => ApplyDaylight(0xA0522D, daylight),
            TileId.GoldOre => ApplyDaylight(0xFFD700, daylight),
            TileId.CopperOre => ApplyDaylight(0xB87333, daylight),
            TileId.Snow => ApplyDaylight(0xF0F0F0, daylight),
            TileId.Ice => ApplyDaylight(0xADD8E6, daylight),
            TileId.Clay => ApplyDaylight(0xA0522D, daylight),
            _ => 0x404040
        };
    }

    private static int GetSkyColor(float depth)
    {
        if (depth < 0.3f)
        {
            return 0x87CEEB;
        }

        return depth < 0.5f ? 0x6495ED : 0x4169E1;
    }

    private static int GetNightSkyColor(float depth)
    {
        if (depth < 0.3f)
        {
            return 0x191970;
        }

        return depth < 0.5f ? 0x0F0F3D : 0x0A0A2E;
    }

    private static int ApplyDaylight(int color, float daylight)
    {
        var r = (color >> 16) & 0xFF;
        var g = (color >> 8) & 0xFF;
        var b = color & 0xFF;

        var factor = 0.3f + 0.7f * daylight;
        r = (int)(r * factor);
        g = (int)(g * factor);
        b = (int)(b * factor);

        return (r << 16) | (g << 8) | b;
    }

    private void RenderEntities()
    {
        foreach (var entity in _world.QueryEntities<EnemyTag, TilePosition>())
        {
            var pos = _world.GetComponent<TilePosition>(entity);
            var tag = _world.GetComponent<EnemyTag>(entity);

            var vx = pos.X - _cameraX;
            var vy = pos.Y - _cameraY;

            if (vx < 0 || vx >= _viewportWidth || vy < 0 || vy >= _viewportHeight)
            {
                continue;
            }

            var ch = tag.EnemyType switch
            {
                EnemyType.Slime => '●',
                EnemyType.Zombie => '☠',
                EnemyType.DemonEye => '⊙',
                _ => '?'
            };

            _frameBuffer[vy * _viewportWidth + vx] = ch;
            _colorBuffer[vy * _viewportWidth + vx] = tag.EnemyType switch
            {
                EnemyType.Slime => 0x00FF00,
                EnemyType.Zombie => 0x00AA00,
                EnemyType.DemonEye => 0xFF0000,
                _ => 0xFFFFFF
            };
        }
    }

    private void RenderPlayer(TilePosition playerPos)
    {
        var vx = playerPos.X - _cameraX;
        var vy = playerPos.Y - _cameraY;

        if (vx < 0 || vx >= _viewportWidth || vy < 0 || vy >= _viewportHeight)
        {
            return;
        }

        _frameBuffer[vy * _viewportWidth + vx] = '@';
        _colorBuffer[vy * _viewportWidth + vx] = 0xFFFF00;
    }

    private void PresentFrame(TilePosition playerPos, in TileMapData tileMap)
    {
        var dayStr = "白天";
        if (_dayTimeEntity.HasValue)
        {
            var dayTime = _world.GetComponent<DayTime>(_dayTimeEntity.Value);
            var hour = (int)(dayTime.TimeOfDay * 24);
            dayStr = dayTime.IsDay ? $"白天 {hour:D2}:00" : $"夜晚 {hour:D2}:00";
        }

        var health = _world.HasComponent<Health>(_playerEntity!.Value)
            ? _world.GetComponent<Health>(_playerEntity.Value)
            : new Health(0, 100);

        var inventoryStr = "";
        if (_playerEntity.HasValue && _world.HasComponent<Inventory>(_playerEntity.Value))
        {
            inventoryStr = InventorySystem.GetInventoryDisplay(_world, _playerEntity.Value);
        }

        var sb = new System.Text.StringBuilder(4096);

        sb.Append($" 泰拉瑞亚 | 位置:({playerPos.X},{playerPos.Y}) | {dayStr} | HP:{health.Current}/{health.Max}");
        sb.AppendLine();
        sb.Append('┌');
        sb.Append(new string('─', _viewportWidth));
        sb.Append('┐');
        sb.AppendLine();

        for (var y = 0; y < _viewportHeight; y++)
        {
            sb.Append('│');
            sb.Append(_frameBuffer, y * _viewportWidth, _viewportWidth);
            sb.Append('│');
            sb.AppendLine();
        }

        sb.Append('└');
        sb.Append(new string('─', _viewportWidth));
        sb.Append('┘');
        sb.AppendLine();
        sb.Append(" WASD:移动 空格:跳跃 J/K/I/S:挖掘 E:攻击 1-0:物品 Q:退出");
        sb.AppendLine();
        sb.Append(inventoryStr);

        try
        {
            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
        }
        catch (IOException)
        {
        }
    }

    public void Shutdown()
    {
        try
        {
            Console.CursorVisible = true;
        }
        catch (IOException)
        {
        }
    }
}
