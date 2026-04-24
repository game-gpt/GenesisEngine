namespace Genesis.Integration.Rendering;

public readonly record struct MaterialRef(
    string TemplateName,
    float R = 1f,
    float G = 1f,
    float B = 1f,
    float A = 1f,
    float Metallic = 0f,
    float Roughness = 0.5f,
    bool CastShadow = true,
    int RenderQueue = 2000)
{
    public static MaterialRef Default => new("Default");
}
