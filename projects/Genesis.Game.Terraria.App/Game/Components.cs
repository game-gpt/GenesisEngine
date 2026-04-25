namespace Genesis.Game.Terraria.Game;

public struct TilePosition
{
    public int X;
    public int Y;
}

public struct TileType
{
    public int TileId;
    public int Variant;
    public bool IsSolid;
}

public struct Health
{
    public int Current;
    public int Max;
}

public struct Inventory
{
    public int Slots;
    public int SelectedSlot;
}

public struct BiomeTag
{
    public int BiomeId;
    public float CorruptionLevel;
    public float HallowLevel;
}

public struct PlayerTag
{
    public int PlayerId;
}

public struct NpcTag
{
    public int NpcType;
    public float Happiness;
    public int HomeX;
    public int HomeY;
}

public struct Velocity
{
    public float Vx;
    public float Vy;
}

public struct Damage
{
    public int Value;
    public int Source;
}

public struct EnemyTag
{
    public int EnemyType;
    public int AiState;
}

public struct TileMapData
{
    public int Width;
    public int Height;
    public ulong Seed;
}

public struct DayTime
{
    public float TimeOfDay;
    public float DayDuration;
    public int DayCount;
}

public struct CameraFollow
{
    public float OffsetX;
    public float OffsetY;
    public float Smoothing;
}
