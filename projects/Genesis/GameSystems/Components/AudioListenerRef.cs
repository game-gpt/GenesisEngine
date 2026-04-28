namespace Genesis.GameSystems.Components;

public readonly record struct AudioListenerRef(
    float Gain = 1f)
{
    public static AudioListenerRef Default => new();
}
