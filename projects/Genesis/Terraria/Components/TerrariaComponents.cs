using Gnosis.ECS;

namespace Genesis.Terraria.Components;

public struct TilePosition : IComponent
{
    public int X;
    public int Y;

    public TilePosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct Velocity : IComponent
{
    public float Vx;
    public float Vy;

    public Velocity(float vx, float vy)
    {
        Vx = vx;
        Vy = vy;
    }
}

public struct Health : IComponent
{
    public int Current;
    public int Max;

    public Health(int current, int max)
    {
        Current = current;
        Max = max;
    }
}

public struct PlayerTag : IComponent
{
    public int PlayerId;

    public PlayerTag(int playerId)
    {
        PlayerId = playerId;
    }
}

public struct EnemyTag : IComponent
{
    public int EnemyType;
    public int AiState;

    public EnemyTag(int enemyType, int aiState = 0)
    {
        EnemyType = enemyType;
        AiState = aiState;
    }
}

public struct NpcTag : IComponent
{
    public int NpcType;
    public float Happiness;
    public int HomeX;
    public int HomeY;

    public NpcTag(int npcType, float happiness = 0.5f, int homeX = 0, int homeY = 0)
    {
        NpcType = npcType;
        Happiness = happiness;
        HomeX = homeX;
        HomeY = homeY;
    }
}

public struct Damage : IComponent
{
    public int Value;
    public int Source;

    public Damage(int value, int source = 0)
    {
        Value = value;
        Source = source;
    }
}

public struct BiomeTag : IComponent
{
    public int BiomeId;
    public float CorruptionLevel;
    public float HallowLevel;

    public BiomeTag(int biomeId, float corruptionLevel = 0.0f, float hallowLevel = 0.0f)
    {
        BiomeId = biomeId;
        CorruptionLevel = corruptionLevel;
        HallowLevel = hallowLevel;
    }
}

public struct Inventory : IComponent
{
    public int Slots;
    public int SelectedSlot;
    public int[] ItemIds;
    public int[] ItemCounts;

    public Inventory(int slots = 40, int selectedSlot = 0)
    {
        Slots = slots;
        SelectedSlot = selectedSlot;
        ItemIds = new int[slots];
        ItemCounts = new int[slots];
    }
}

public struct TileMapData : IComponent
{
    public int Width;
    public int Height;
    public int[] Tiles;
    public int[] TileVariants;
    public ulong Seed;

    public TileMapData(int width, int height, ulong seed)
    {
        Width = width;
        Height = height;
        Seed = seed;
        Tiles = new int[width * height];
        TileVariants = new int[width * height];
    }

    public int GetTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return TileId.Boundary;
        }

        return Tiles[y * Width + x];
    }

    public void SetTile(int x, int y, int tileId)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        Tiles[y * Width + x] = tileId;
    }

    public bool IsSolid(int x, int y)
    {
        var tile = GetTile(x, y);
        return tile != TileId.Air && tile != TileId.Water && tile != TileId.Boundary;
    }
}

public struct DayTime : IComponent
{
    public float TimeOfDay;
    public float DayDuration;
    public int DayCount;

    public DayTime(float timeOfDay = 0.25f, float dayDuration = 600.0f)
    {
        TimeOfDay = timeOfDay;
        DayDuration = dayDuration;
        DayCount = 1;
    }

    public float Daylight
    {
        get
        {
            if (TimeOfDay < 0.25f)
            {
                return TimeOfDay / 0.25f;
            }

            if (TimeOfDay < 0.75f)
            {
                return 1.0f;
            }

            return 1.0f - (TimeOfDay - 0.75f) / 0.25f;
        }
    }

    public bool IsDay => TimeOfDay is >= 0.25f and < 0.75f;
}

public struct CameraFollow : IComponent
{
    public float OffsetX;
    public float OffsetY;
    public float Smoothing;

    public CameraFollow(float offsetX = 0.0f, float offsetY = 0.0f, float smoothing = 0.1f)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
        Smoothing = smoothing;
    }
}
