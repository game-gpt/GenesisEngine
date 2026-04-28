namespace Genesis.GameSystems.Components;

public readonly record struct ColliderRef(
    ColliderShapeType ShapeType = ColliderShapeType.Box,
    float HalfExtentsX = 0.5f,
    float HalfExtentsY = 0.5f,
    float HalfExtentsZ = 0.5f,
    float Radius = 0.5f,
    float Height = 1f,
    bool IsTrigger = false)
{
    public static ColliderRef Box(float hx = 0.5f, float hy = 0.5f, float hz = 0.5f) => new(ColliderShapeType.Box, hx, hy, hz);
    public static ColliderRef Sphere(float radius = 0.5f) => new(ColliderShapeType.Sphere, Radius: radius);
    public static ColliderRef Capsule(float radius = 0.25f, float height = 1f) => new(ColliderShapeType.Capsule, Radius: radius, Height: height);
}

public enum ColliderShapeType
{
    Box = 0,
    Sphere = 1,
    Capsule = 2,
    Mesh = 3
}
