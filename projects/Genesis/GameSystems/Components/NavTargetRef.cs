namespace Genesis.GameSystems.Components;

public readonly record struct NavTargetRef(
    float TargetX = 0f,
    float TargetY = 0f,
    float TargetZ = 0f,
    bool HasTarget = false)
{
    public static NavTargetRef None => new();
    public static NavTargetRef At(float x, float y, float z) => new(x, y, z, true);
}
