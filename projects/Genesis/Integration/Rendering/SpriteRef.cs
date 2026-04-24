namespace Genesis.Integration.Rendering;

public readonly record struct SpriteRef(
    string TexturePath = "",
    string LayerName = "default",
    int LayerOrder = 0,
    float Width = 1f,
    float Height = 1f,
    float OriginX = 0f,
    float OriginY = 0f,
    int SortOrder = 0)
{
    public static SpriteRef Default => new();
    public bool IsValid => !string.IsNullOrEmpty(TexturePath);
}
