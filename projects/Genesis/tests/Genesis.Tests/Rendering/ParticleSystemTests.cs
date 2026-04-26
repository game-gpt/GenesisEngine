using Genesis.Rendering;
using Xunit;

namespace Genesis.Tests.Rendering;

public class ParticleSystemTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        Assert.Equal(0, ps.ActiveParticleCount);
        Assert.False(ps.IsPlaying);
    }

    [Fact]
    public void Play_SetsIsPlaying()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        ps.Play();
        Assert.True(ps.IsPlaying);
    }

    [Fact]
    public void Stop_ClearsIsPlaying()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        ps.Play();
        ps.Stop();
        Assert.False(ps.IsPlaying);
    }

    [Fact]
    public void Update_WhenPlaying_EmitsParticles()
    {
        var emitter = ParticleEmitter.Default with { EmitRate = 10f, MaxParticles = 100 };
        var ps = new ParticleSystem(emitter);
        ps.Play();
        ps.Update(1.0f);
        Assert.True(ps.ActiveParticleCount > 0);
    }

    [Fact]
    public void Update_WhenNotPlaying_DoesNothing()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        ps.Update(1.0f);
        Assert.Equal(0, ps.ActiveParticleCount);
    }

    [Fact]
    public void EmitBurst_CreatesParticles()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        ps.EmitBurst(20);
        Assert.Equal(20, ps.ActiveParticleCount);
    }

    [Fact]
    public void EmitBurst_RespectsMaxParticles()
    {
        var emitter = ParticleEmitter.Default with { MaxParticles = 10 };
        var ps = new ParticleSystem(emitter);
        ps.EmitBurst(50);
        Assert.Equal(10, ps.ActiveParticleCount);
    }

    [Fact]
    public void Update_ParticlesAgeAndDie()
    {
        var emitter = ParticleEmitter.Default with
        {
            EmitRate = 0f,
            Lifetime = 0.5f,
            MaxParticles = 100
        };
        var ps = new ParticleSystem(emitter);
        ps.EmitBurst(5);
        ps.Play();
        ps.Update(1.0f);
        Assert.Equal(0, ps.ActiveParticleCount);
    }

    [Fact]
    public void Reset_ClearsAllParticles()
    {
        var ps = new ParticleSystem(ParticleEmitter.Default);
        ps.EmitBurst(10);
        ps.Reset();
        Assert.Equal(0, ps.ActiveParticleCount);
        Assert.False(ps.IsPlaying);
    }
}

public class PostProcessPipelineTests
{
    [Fact]
    public void AddEffect_AddsToPipeline()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.AddEffect(PostProcessEffect.Bloom);
        Assert.Single(pipeline.Effects);
    }

    [Fact]
    public void AddEffect_DuplicateName_Replaces()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.AddEffect(PostProcessEffect.Bloom);
        pipeline.AddEffect(PostProcessEffect.Bloom with { Intensity = 0.8f });
        Assert.Single(pipeline.Effects);
        Assert.Equal(0.8f, pipeline.Effects[0].Intensity);
    }

    [Fact]
    public void RemoveEffect_RemovesByName()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.AddEffect(PostProcessEffect.Bloom);
        pipeline.RemoveEffect("Bloom");
        Assert.Empty(pipeline.Effects);
    }

    [Fact]
    public void SetIntensity_UpdatesEffect()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.AddEffect(PostProcessEffect.Bloom);
        pipeline.SetIntensity("Bloom", 0.9f);
        Assert.Equal(0.9f, pipeline.GetEffect("Bloom")!.Value.Intensity);
    }

    [Fact]
    public void ApplyPreset_Default_AddsExpectedEffects()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.ApplyPreset(PostProcessPreset.Default);
        Assert.True(pipeline.Effects.Count >= 2);
    }

    [Fact]
    public void ApplyPreset_Cinematic_AddsMoreEffects()
    {
        var pipeline = new PostProcessPipeline();
        pipeline.ApplyPreset(PostProcessPreset.Cinematic);
        Assert.True(pipeline.Effects.Count >= 3);
    }
}
