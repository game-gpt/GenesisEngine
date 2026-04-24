namespace Genesis.Integration.Audio;

public readonly record struct AudioSourceRef(
    string ClipPath = "",
    float Volume = 1f,
    float Pitch = 1f,
    bool IsLooping = false,
    bool Spatialize = true,
    float MinDistance = 1f,
    float MaxDistance = 100f,
    float SpatialBlend = 1f,
    string? BusName = null)
{
    public static AudioSourceRef Default => new();
    public bool IsValid => !string.IsNullOrEmpty(ClipPath);
}
