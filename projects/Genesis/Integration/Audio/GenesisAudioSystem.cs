using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.Audio.Driver;
using Gnosis.Audio.Mixer;
using Gnosis.Audio.Source;
using Gnosis.Core.Entity;
using Gnosis.ECS.System;
using Gnosis.ECS.World;

namespace Genesis.Integration.Audio;

[Obsolete("请使用 Genesis.GameSystems.AudioGameSystem 替代。Integration 层将在未来版本移除。")]
public sealed class GenesisAudioSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private readonly AudioSystem _audioSystem;
    private readonly Dictionary<uint, IAudioSource> _entitySources = new();
    private IAudioBus? _sfxBus;
    private IAudioBus? _ambientBus;
    private IAudioBus? _musicBus;
    private bool _initialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.PostUpdate;

    public AudioSystem GnosisAudioSystem => _audioSystem;

    public float GlobalVolume
    {
        get => _audioSystem.GlobalVolume;
        set => _audioSystem.GlobalVolume = value;
    }

    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    public GenesisAudioSystem()
    {
        _audioSystem = new AudioSystem();
    }

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _sfxBus = _audioSystem.CreateBus("SFX", _audioSystem.MasterBus);
        _ambientBus = _audioSystem.CreateBus("Ambient", _audioSystem.MasterBus);
        _musicBus = _audioSystem.CreateBus("Music", _audioSystem.MasterBus);
        _initialized = true;
    }

    public void Shutdown()
    {
        foreach (var source in _entitySources.Values)
        {
            source.Stop();
            _audioSystem.DestroySource(source);
        }

        _entitySources.Clear();

        if (_sfxBus is not null)
        {
            _audioSystem.DestroyBus("SFX");
        }

        if (_ambientBus is not null)
        {
            _audioSystem.DestroyBus("Ambient");
        }

        if (_musicBus is not null)
        {
            _audioSystem.DestroyBus("Music");
        }

        _initialized = false;
    }

    #endregion

    #region IWorldSystem 实现

    public void SetWorld(Gnosis.ECS.World.World world)
    {
        _world = world;
    }

    #endregion

    #region ISystem.Update

    public void Update(float delta)
    {
        if (_world is null || !_initialized)
        {
            return;
        }

        SyncNewSourcesFromWorld();
        SyncListenerFromWorld();
        _audioSystem.Update(delta);
    }

    #endregion

    #region 公开方法

    public IAudioBus? GetBus(string name)
    {
        return _audioSystem.GetBus(name);
    }

    public void PlaySource(uint entityIndex)
    {
        if (_entitySources.TryGetValue(entityIndex, out var source))
        {
            source.Play();
        }
    }

    public void StopSource(uint entityIndex)
    {
        if (_entitySources.TryGetValue(entityIndex, out var source))
        {
            source.Stop();
        }
    }

    public void SetBusVolume(string busName, float volume)
    {
        var bus = _audioSystem.GetBus(busName);

        if (bus is not null)
        {
            bus.Volume = volume;
        }
    }

    #endregion

    #region 私有方法

    private void SyncNewSourcesFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<AudioSourceRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (_entitySources.ContainsKey(entityId.Index))
            {
                continue;
            }

            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<AudioSourceRef>(entityId))
            {
                continue;
            }

            var audioRef = _world.GetComponent<AudioSourceRef>(entityId);
            var source = _audioSystem.CreateSource();

            source.Volume = audioRef.Volume;
            source.Pitch = audioRef.Pitch;
            source.IsLooping = audioRef.IsLooping;
            source.Spatialize = audioRef.Spatialize;
            source.MinDistance = audioRef.MinDistance;
            source.MaxDistance = audioRef.MaxDistance;
            source.SpatialBlend = audioRef.SpatialBlend;
            source.BusName = audioRef.BusName;

            if (audioRef.IsValid)
            {
                var clip = _audioSystem.LoadClip(audioRef.ClipPath);
                source.Clip = clip;
            }

            _entitySources[entityId.Index] = source;
        }
    }

    private void SyncListenerFromWorld()
    {
        if (_world is null)
        {
            return;
        }

        var entities = _world.CreateQuery()
            .All<Transform3D>()
            .All<AudioListenerRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!_world.HasComponent<Transform3D>(entityId) || !_world.HasComponent<AudioListenerRef>(entityId))
            {
                continue;
            }

            var transform = _world.GetComponent<Transform3D>(entityId);
            var listenerRef = _world.GetComponent<AudioListenerRef>(entityId);

            var listener = _audioSystem.Listener;
            listener.Position = new Vector3(transform.PosX, transform.PosY, transform.PosZ);
            listener.Volume = listenerRef.Gain;

            break;
        }
    }

    #endregion
}
