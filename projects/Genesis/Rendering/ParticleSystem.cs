namespace Genesis.Rendering;

public sealed class ParticleSystem
{
    #region 字段

    private readonly List<Particle> _particles;
    private readonly ParticleEmitter _emitter;
    private float _emitAccumulator;
    private float _elapsedTime;
    private bool _isPlaying;

    #endregion

    #region 属性

    public float OriginX { get; set; }
    public float OriginY { get; set; }
    public int ActiveParticleCount => _particles.Count;
    public bool IsPlaying => _isPlaying;
    public ParticleEmitter Emitter => _emitter;
    public IReadOnlyList<Particle> Particles => _particles.AsReadOnly();

    #endregion

    #region 构造函数

    public ParticleSystem(ParticleEmitter emitter)
    {
        _emitter = emitter;
        _particles = new List<Particle>(emitter.MaxParticles);
        _emitAccumulator = 0f;
        _elapsedTime = 0f;
        _isPlaying = false;
    }

    #endregion

    #region 公开方法

    public void Play()
    {
        _isPlaying = true;
        _elapsedTime = 0f;
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    public void Reset()
    {
        _particles.Clear();
        _emitAccumulator = 0f;
        _elapsedTime = 0f;
        _isPlaying = false;
    }

    public void Update(float delta)
    {
        if (!_isPlaying)
        {
            return;
        }

        _elapsedTime += delta;

        if (!_emitter.IsLooping && _elapsedTime > _emitter.Duration)
        {
            _isPlaying = false;
        }

        EmitNewParticles(delta);
        UpdateExistingParticles(delta);
        RemoveDeadParticles();
    }

    public void EmitBurst(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (_particles.Count >= _emitter.MaxParticles)
            {
                break;
            }

            _particles.Add(CreateParticle());
        }
    }

    #endregion

    #region 私有方法

    private void EmitNewParticles(float delta)
    {
        _emitAccumulator += _emitter.EmitRate * delta;

        while (_emitAccumulator >= 1f && _particles.Count < _emitter.MaxParticles)
        {
            _particles.Add(CreateParticle());
            _emitAccumulator -= 1f;
        }
    }

    private void UpdateExistingParticles(float delta)
    {
        for (var i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            p.Age += delta;

            var t = p.Age / p.Lifetime;

            p.VelocityX += _emitter.GravityX * delta;
            p.VelocityY += _emitter.GravityY * delta;

            p.PositionX += p.VelocityX * delta;
            p.PositionY += p.VelocityY * delta;

            p.CurrentColorR = Lerp(_emitter.StartColorR, _emitter.EndColorR, t);
            p.CurrentColorG = Lerp(_emitter.StartColorG, _emitter.EndColorG, t);
            p.CurrentColorB = Lerp(_emitter.StartColorB, _emitter.EndColorB, t);
            p.CurrentColorA = Lerp(_emitter.StartColorA, _emitter.EndColorA, t);

            p.CurrentSize = Lerp(p.StartSize, 0f, t);

            _particles[i] = p;
        }
    }

    private void RemoveDeadParticles()
    {
        _particles.RemoveAll(p => p.Age >= p.Lifetime);
    }

    private Particle CreateParticle()
    {
        var angle = (float)(Math.Atan2(_emitter.DirectionY, _emitter.DirectionX) +
                            (Random.Shared.NextDouble() - 0.5) * _emitter.DirectionVariance * Math.PI * 2);

        var speed = _emitter.Speed + (float)(Random.Shared.NextDouble() - 0.5) * 2 * _emitter.SpeedVariance;
        var size = _emitter.Size + (float)(Random.Shared.NextDouble() - 0.5) * 2 * _emitter.SizeVariance;
        var lifetime = _emitter.Lifetime * (0.8f + (float)Random.Shared.NextDouble() * 0.4f);

        return new Particle
        {
            PositionX = OriginX,
            PositionY = OriginY,
            VelocityX = (float)Math.Cos(angle) * speed,
            VelocityY = (float)Math.Sin(angle) * speed,
            StartSize = Math.Max(0.1f, size),
            CurrentSize = Math.Max(0.1f, size),
            Lifetime = lifetime,
            Age = 0f,
            CurrentColorR = _emitter.StartColorR,
            CurrentColorG = _emitter.StartColorG,
            CurrentColorB = _emitter.StartColorB,
            CurrentColorA = _emitter.StartColorA
        };
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Math.Clamp(t, 0f, 1f);
    }

    #endregion
}

public struct Particle
{
    public float PositionX;
    public float PositionY;
    public float VelocityX;
    public float VelocityY;
    public float StartSize;
    public float CurrentSize;
    public float Lifetime;
    public float Age;
    public float CurrentColorR;
    public float CurrentColorG;
    public float CurrentColorB;
    public float CurrentColorA;
}
