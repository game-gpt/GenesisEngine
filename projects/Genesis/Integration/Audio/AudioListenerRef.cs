namespace Genesis.Integration.Audio;

public readonly record struct AudioListenerRef(
    float Gain = 1f)
{
    public static AudioListenerRef Default => new();
}
