using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Genesis.Terraria.Platform;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class PlayerMovementSystem : ISystem
{
    #region 私有字段

    private readonly EcsWorld _world;
    private readonly Win32Window _window;
    private EntityId? _playerEntity;
    private EntityId? _tileMapEntity;

    #endregion

    #region 公开属性

    public SystemPhase Phase => SystemPhase.Update;

    #endregion

    #region 构造函数

    public PlayerMovementSystem(EcsWorld world, Win32Window window)
    {
        _world = world;
        _window = window;
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<PlayerTag, TilePosition, Velocity>())
        {
            _playerEntity = entity;
            break;
        }

        foreach (var entity in _world.QueryEntities<TileMapData>())
        {
            _tileMapEntity = entity;
            break;
        }
    }

    public void Update(float delta)
    {
        if (!_playerEntity.HasValue || !_tileMapEntity.HasValue)
        {
            return;
        }

        ref var pos = ref _world.GetComponentRef<TilePosition>(_playerEntity.Value);
        ref var vel = ref _world.GetComponentRef<Velocity>(_playerEntity.Value);
        ref var tileMap = ref _world.GetComponentRef<TileMapData>(_tileMapEntity.Value);

        var moveSpeed = 1;

        if (_window.IsKeyDown(Win32Window.VirtualKey.A) ||
            _window.IsKeyDown(Win32Window.VirtualKey.Left))
        {
            TryMove(ref pos, -moveSpeed, 0, in tileMap);
        }

        if (_window.IsKeyDown(Win32Window.VirtualKey.D) ||
            _window.IsKeyDown(Win32Window.VirtualKey.Right))
        {
            TryMove(ref pos, moveSpeed, 0, in tileMap);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.W) ||
            _window.IsKeyJustPressed(Win32Window.VirtualKey.Up) ||
            _window.IsKeyJustPressed(Win32Window.VirtualKey.Space))
        {
            TryJump(ref pos, ref vel, in tileMap);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.S) ||
            _window.IsKeyJustPressed(Win32Window.VirtualKey.Down))
        {
            MineTile(ref pos, ref tileMap, 0, 1);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.J))
        {
            MineTile(ref pos, ref tileMap, -1, 0);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.K))
        {
            MineTile(ref pos, ref tileMap, 1, 0);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.I))
        {
            MineTile(ref pos, ref tileMap, 0, -1);
        }

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.E))
        {
            AttackNearby(ref pos);
        }

        HandleInventorySlotSelection();
    }

    public void Shutdown()
    {
    }

    #endregion

    #region 移动

    private static void TryMove(ref TilePosition pos, int dx, int dy, in TileMapData tileMap)
    {
        var newX = pos.X + dx;
        var newY = pos.Y + dy;

        if (!tileMap.IsSolid(newX, newY))
        {
            pos.X = newX;
            pos.Y = newY;
        }
    }

    private static void TryJump(ref TilePosition pos, ref Velocity vel, in TileMapData tileMap)
    {
        if (pos.Y <= 0)
        {
            return;
        }

        var onGround = tileMap.IsSolid(pos.X, pos.Y + 1);
        if (!onGround)
        {
            return;
        }

        if (!tileMap.IsSolid(pos.X, pos.Y - 1))
        {
            pos.Y -= 1;

            if (!tileMap.IsSolid(pos.X, pos.Y - 1))
            {
                pos.Y -= 1;
            }
        }
    }

    #endregion

    #region 挖掘

    private void MineTile(ref TilePosition pos, ref TileMapData tileMap, int dx, int dy)
    {
        var targetX = pos.X + dx;
        var targetY = pos.Y + dy;

        var tile = tileMap.GetTile(targetX, targetY);
        if (tile == TileId.Air || tile == TileId.Boundary)
        {
            return;
        }

        tileMap.SetTile(targetX, targetY, TileId.Air);

        if (!_playerEntity.HasValue || !_world.HasComponent<Inventory>(_playerEntity.Value))
        {
            return;
        }

        ref var inventory = ref _world.GetComponentRef<Inventory>(_playerEntity.Value);
        AddItemToInventory(ref inventory, tile, 1);
    }

    private static void AddItemToInventory(ref Inventory inventory, int itemId, int count)
    {
        for (var i = 0; i < inventory.Slots; i++)
        {
            if (inventory.ItemIds[i] == itemId)
            {
                inventory.ItemCounts[i] += count;
                return;
            }
        }

        for (var i = 0; i < inventory.Slots; i++)
        {
            if (inventory.ItemIds[i] == ItemId.None)
            {
                inventory.ItemIds[i] = itemId;
                inventory.ItemCounts[i] = count;
                return;
            }
        }
    }

    #endregion

    #region 战斗

    private void AttackNearby(ref TilePosition playerPos)
    {
        if (!_playerEntity.HasValue)
        {
            return;
        }

        var inventory = _world.GetComponent<Inventory>(_playerEntity.Value);
        var selectedItem = inventory.ItemIds[inventory.SelectedSlot];
        var damage = ItemId.GetAttackDamage(selectedItem);

        foreach (var enemy in _world.QueryEntities<EnemyTag, TilePosition, Health>())
        {
            var enemyPos = _world.GetComponent<TilePosition>(enemy);
            var dx = Math.Abs(enemyPos.X - playerPos.X);
            var dy = Math.Abs(enemyPos.Y - playerPos.Y);

            if (dx <= 2 && dy <= 2)
            {
                _world.AddComponent(enemy, new Damage(damage, 0));
            }
        }
    }

    #endregion

    #region 物品栏

    private void HandleInventorySlotSelection()
    {
        if (!_playerEntity.HasValue || !_world.HasComponent<Inventory>(_playerEntity.Value))
        {
            return;
        }

        ref var inventory = ref _world.GetComponentRef<Inventory>(_playerEntity.Value);

        var slot = -1;

        if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D1)) slot = 0;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D2)) slot = 1;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D3)) slot = 2;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D4)) slot = 3;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D5)) slot = 4;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D6)) slot = 5;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D7)) slot = 6;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D8)) slot = 7;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D9)) slot = 8;
        else if (_window.IsKeyJustPressed(Win32Window.VirtualKey.D0)) slot = 9;

        if (slot >= 0 && slot < inventory.Slots)
        {
            inventory.SelectedSlot = slot;
        }
    }

    #endregion
}
