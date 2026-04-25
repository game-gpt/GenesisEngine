namespace Genesis.Game.Terraria.Game;

public static class Tile
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

    public static bool IsSolid(int tileId)
    {
        return tileId != Air && tileId != Water;
    }
}

public static class Biome
{
    public const int Forest = 0;
    public const int Desert = 1;
    public const int Corruption = 2;
    public const int Crimson = 3;
    public const int Hallow = 4;
    public const int Jungle = 5;
    public const int Tundra = 6;
    public const int Ocean = 7;
}

public static class EnemyType
{
    public const int Slime = 0;
    public const int Zombie = 1;
    public const int DemonEye = 2;
}

public static class AiState
{
    public const int Idle = 0;
    public const int Wander = 1;
    public const int Chase = 2;
    public const int Attack = 3;
}
