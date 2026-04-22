namespace Genesis.Terraria.Components;

public static class TileId
{
    public const int Air = 0;
    public const int Dirt = 1;
    public const int Stone = 2;
    public const int Grass = 3;
    public const int Sand = 4;
    public const int Water = 5;
    public const int Wood = 6;
    public const int Leaf = 7;
    public const int IronOre = 8;
    public const int GoldOre = 9;
    public const int CopperOre = 10;
    public const int Snow = 11;
    public const int Ice = 12;
    public const int Clay = 13;
    public const int Boundary = 99;

    private static readonly Dictionary<int, string> NameMap = new()
    {
        [Air] = "空气",
        [Dirt] = "泥土",
        [Stone] = "石头",
        [Grass] = "草地",
        [Sand] = "沙子",
        [Water] = "水",
        [Wood] = "木头",
        [Leaf] = "树叶",
        [IronOre] = "铁矿",
        [GoldOre] = "金矿",
        [CopperOre] = "铜矿",
        [Snow] = "雪",
        [Ice] = "冰",
        [Clay] = "黏土",
        [Boundary] = "边界"
    };

    private static readonly Dictionary<int, char> DisplayCharMap = new()
    {
        [Air] = ' ',
        [Dirt] = '.',
        [Stone] = '#',
        [Grass] = '"',
        [Sand] = ':',
        [Water] = '~',
        [Wood] = '█',
        [Leaf] = '♣',
        [IronOre] = '%',
        [GoldOre] = '$',
        [CopperOre] = '¢',
        [Snow] = '*',
        [Ice] = '+',
        [Clay] = '=',
        [Boundary] = '▓'
    };

    public static string GetName(int tileId)
    {
        return NameMap.TryGetValue(tileId, out var name) ? name : "未知";
    }

    public static char GetDisplayChar(int tileId)
    {
        return DisplayCharMap.TryGetValue(tileId, out var ch) ? ch : '?';
    }
}

public static class ItemId
{
    public const int None = 0;
    public const int Dirt = 1;
    public const int Stone = 2;
    public const int Wood = 3;
    public const int IronOre = 8;
    public const int GoldOre = 9;
    public const int CopperOre = 10;
    public const int CopperSword = 100;
    public const int IronSword = 101;
    public const int GoldSword = 102;
    public const int CopperPickaxe = 200;
    public const int IronPickaxe = 201;
    public const int GoldPickaxe = 202;

    private static readonly Dictionary<int, string> NameMap = new()
    {
        [None] = "无",
        [Dirt] = "泥土",
        [Stone] = "石头",
        [Wood] = "木头",
        [IronOre] = "铁矿石",
        [GoldOre] = "金矿石",
        [CopperOre] = "铜矿石",
        [CopperSword] = "铜剑",
        [IronSword] = "铁剑",
        [GoldSword] = "金剑",
        [CopperPickaxe] = "铜镐",
        [IronPickaxe] = "铁镐",
        [GoldPickaxe] = "金镐"
    };

    public static string GetName(int itemId)
    {
        return NameMap.TryGetValue(itemId, out var name) ? name : "未知";
    }

    public static int GetAttackDamage(int itemId)
    {
        return itemId switch
        {
            CopperSword => 8,
            IronSword => 15,
            GoldSword => 25,
            _ => 1
        };
    }

    public static int GetMiningPower(int itemId)
    {
        return itemId switch
        {
            CopperPickaxe => 2,
            IronPickaxe => 4,
            GoldPickaxe => 6,
            _ => 1
        };
    }

    public static bool IsSword(int itemId)
    {
        return itemId is >= CopperSword and <= GoldSword;
    }

    public static bool IsPickaxe(int itemId)
    {
        return itemId is >= CopperPickaxe and <= GoldPickaxe;
    }
}

public static class EnemyType
{
    public const int Slime = 0;
    public const int Zombie = 1;
    public const int DemonEye = 2;

    public static readonly string[] Names = ["史莱姆", "僵尸", "恶魔眼"];

    public static string GetName(int type)
    {
        return type >= 0 && type < Names.Length ? Names[type] : "未知";
    }

    public static int GetMaxHealth(int type)
    {
        return type switch
        {
            Slime => 25,
            Zombie => 45,
            DemonEye => 60,
            _ => 10
        };
    }

    public static int GetAttackDamage(int type)
    {
        return type switch
        {
            Slime => 6,
            Zombie => 14,
            DemonEye => 18,
            _ => 5
        };
    }

    public static float GetMoveSpeed(int type)
    {
        return type switch
        {
            Slime => 1.5f,
            Zombie => 0.8f,
            DemonEye => 2.0f,
            _ => 1.0f
        };
    }
}

public static class AiState
{
    public const int Idle = 0;
    public const int Wander = 1;
    public const int Chase = 2;
    public const int Attack = 3;
    public const int Flee = 4;
}

public static class BiomeId
{
    public const int Forest = 0;
    public const int Desert = 1;
    public const int Corruption = 2;
    public const int Crimson = 3;
    public const int Hallow = 4;
    public const int Jungle = 5;
    public const int Tundra = 6;
    public const int Ocean = 7;

    public static readonly string[] Names =
        ["森林", "沙漠", "腐化", "猩红", "神圣", "丛林", "冻原", "海洋"];

    public static string GetName(int id)
    {
        return id >= 0 && id < Names.Length ? Names[id] : "未知";
    }
}
