namespace Genesis.Integration.Rendering;

public readonly record struct LightRef(
    LightType Type = LightType.Directional,
    float R = 1f,
    float G = 1f,
    float B = 1f,
    float Intensity = 1f,
    float Range = 10f,
    float SpotAngle = 45f,
    bool CastShadow = true)
{
    public static LightRef Directional => new(LightType.Directional, Intensity: 1f);
    public static LightRef Point => new(LightType.Point, Intensity: 1f, Range: 10f);
    public static LightRef Spot => new(LightType.Spot, Intensity: 1f, Range: 10f, SpotAngle: 45f);
}

public enum LightType
{
    Directional = 0,
    Point = 1,
    Spot = 2,
    Area = 3
}
