using Genesis.Terraria.Components;
using Genesis.Terraria.ECS;
using Gnosis.ECS;

namespace Genesis.Terraria.Systems;

public sealed class InventorySystem : ISystem
{
    private readonly EcsWorld _world;
    private EntityId? _playerEntity;

    public SystemPhase Phase => SystemPhase.PostUpdate;

    public InventorySystem(EcsWorld world)
    {
        _world = world;
    }

    public void Initialize()
    {
        foreach (var entity in _world.QueryEntities<PlayerTag, Inventory>())
        {
            _playerEntity = entity;
            break;
        }
    }

    public void Update(float delta)
    {
    }

    public static string GetInventoryDisplay(EcsWorld world, EntityId playerEntity)
    {
        if (!world.HasComponent<Inventory>(playerEntity))
        {
            return "无物品栏";
        }

        var inventory = world.GetComponent<Inventory>(playerEntity);
        var lines = new List<string>();

        lines.Add("┌─────────── 物品栏 ───────────┐");

        for (var i = 0; i < Math.Min(inventory.Slots, 10); i++)
        {
            var selector = i == inventory.SelectedSlot ? "►" : " ";
            var itemName = inventory.ItemIds[i] == ItemId.None
                ? "---"
                : $"{ItemId.GetName(inventory.ItemIds[i])}x{inventory.ItemCounts[i]}";
            lines.Add($"│{selector} [{i + 1}] {itemName,-22}│");
        }

        lines.Add("└──────────────────────────────┘");

        var selectedItem = inventory.ItemIds[inventory.SelectedSlot];
        if (selectedItem != ItemId.None)
        {
            lines.Add($"  当前: {ItemId.GetName(selectedItem)}");
            if (ItemId.IsSword(selectedItem))
            {
                lines.Add($"  攻击力: {ItemId.GetAttackDamage(selectedItem)}");
            }
            else if (ItemId.IsPickaxe(selectedItem))
            {
                lines.Add($"  挖掘力: {ItemId.GetMiningPower(selectedItem)}");
            }
        }

        return string.Join('\n', lines);
    }

    public void Shutdown()
    {
    }
}
