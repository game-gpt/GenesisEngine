namespace Genesis.GameSystems.Components;

public readonly record struct CameraRef(
    float FieldOfView = 60f,
    float NearPlane = 0.1f,
    float FarPlane = 1000f,
    bool IsMain = false)
{
    public static CameraRef Main => new(IsMain: true);
}
