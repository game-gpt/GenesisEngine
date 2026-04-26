namespace Genesis.Rendering;

public readonly record struct PostProcessEffect(
    string Name,
    PostProcessEffectType Type,
    float Intensity,
    bool IsEnabled)
{
    public static PostProcessEffect Bloom => new("Bloom", PostProcessEffectType.Bloom, 0.5f, true);
    public static PostProcessEffect Vignette => new("Vignette", PostProcessEffectType.Vignette, 0.3f, true);
    public static PostProcessEffect ColorGrading => new("ColorGrading", PostProcessEffectType.ColorGrading, 1.0f, true);
    public static PostProcessEffect Fog => new("Fog", PostProcessEffectType.Fog, 0.5f, false);
    public static PostProcessEffect ScreenShake => new("ScreenShake", PostProcessEffectType.ScreenShake, 0f, false);
    public static PostProcessEffect ChromaticAberration => new("ChromaticAberration", PostProcessEffectType.ChromaticAberration, 0f, false);
}

public enum PostProcessEffectType
{
    Bloom,
    Vignette,
    ColorGrading,
    Fog,
    ScreenShake,
    ChromaticAberration
}
