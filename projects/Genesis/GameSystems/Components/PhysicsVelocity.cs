namespace Genesis.GameSystems.Components;

public readonly record struct PhysicsVelocity(
    float Vx = 0f,
    float Vy = 0f,
    float Vz = 0f,
    float Avx = 0f,
    float Avy = 0f,
    float Avz = 0f)
{
    public static PhysicsVelocity Zero => new();
}
