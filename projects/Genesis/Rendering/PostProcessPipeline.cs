namespace Genesis.Rendering;

public sealed class PostProcessPipeline
{
    #region 字段

    private readonly List<PostProcessEffect> _effects;

    #endregion

    #region 属性

    public IReadOnlyList<PostProcessEffect> Effects => _effects.AsReadOnly();
    public int EnabledCount => _effects.Count(e => e.IsEnabled);

    #endregion

    #region 构造函数

    public PostProcessPipeline()
    {
        _effects = new List<PostProcessEffect>();
    }

    #endregion

    #region 公开方法

    public void AddEffect(PostProcessEffect effect)
    {
        var idx = _effects.FindIndex(e => e.Name == effect.Name);
        if (idx >= 0)
        {
            _effects[idx] = effect;
        }
        else
        {
            _effects.Add(effect);
        }
    }

    public void RemoveEffect(string name)
    {
        _effects.RemoveAll(e => e.Name == name);
    }

    public void SetIntensity(string name, float intensity)
    {
        var idx = _effects.FindIndex(e => e.Name == name);
        if (idx >= 0)
        {
            _effects[idx] = _effects[idx] with { Intensity = intensity };
        }
    }

    public void SetEnabled(string name, bool enabled)
    {
        var idx = _effects.FindIndex(e => e.Name == name);
        if (idx >= 0)
        {
            _effects[idx] = _effects[idx] with { IsEnabled = enabled };
        }
    }

    public PostProcessEffect? GetEffect(string name)
    {
        var idx = _effects.FindIndex(e => e.Name == name);
        return idx >= 0 ? _effects[idx] : null;
    }

    public void ApplyPreset(PostProcessPreset preset)
    {
        _effects.Clear();

        switch (preset)
        {
            case PostProcessPreset.Default:
                AddEffect(PostProcessEffect.Bloom with { Intensity = 0.3f });
                AddEffect(PostProcessEffect.Vignette with { Intensity = 0.2f });
                break;

            case PostProcessPreset.Cinematic:
                AddEffect(PostProcessEffect.Bloom with { Intensity = 0.5f });
                AddEffect(PostProcessEffect.Vignette with { Intensity = 0.4f });
                AddEffect(PostProcessEffect.ColorGrading with { Intensity = 0.8f });
                AddEffect(PostProcessEffect.ChromaticAberration with { Intensity = 0.1f, IsEnabled = true });
                break;

            case PostProcessPreset.Atmospheric:
                AddEffect(PostProcessEffect.Bloom with { Intensity = 0.4f });
                AddEffect(PostProcessEffect.Fog with { Intensity = 0.6f, IsEnabled = true });
                AddEffect(PostProcessEffect.Vignette with { Intensity = 0.3f });
                AddEffect(PostProcessEffect.ColorGrading with { Intensity = 0.6f });
                break;

            case PostProcessPreset.Minimal:
                AddEffect(PostProcessEffect.Vignette with { Intensity = 0.1f });
                break;

            case PostProcessPreset.Intense:
                AddEffect(PostProcessEffect.Bloom with { Intensity = 0.8f });
                AddEffect(PostProcessEffect.Vignette with { Intensity = 0.5f });
                AddEffect(PostProcessEffect.ColorGrading with { Intensity = 1.2f });
                AddEffect(PostProcessEffect.Fog with { Intensity = 0.3f, IsEnabled = true });
                AddEffect(PostProcessEffect.ChromaticAberration with { Intensity = 0.2f, IsEnabled = true });
                break;
        }
    }

    #endregion
}

public enum PostProcessPreset
{
    Default,
    Cinematic,
    Atmospheric,
    Minimal,
    Intense
}
