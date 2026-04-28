using System.Numerics;
using Genesis.Core;
using Genesis.GameSystems.Components;
using Genesis.HAL;
using GnosisAudioBus = Gnosis.Audio.Mixer.IAudioBus;
using GnosisAudioSource = Gnosis.Audio.Source.IAudioSource;
using GnosisAudioSystem = Gnosis.Audio.Driver.AudioSystem;
using Gnosis.ECS.System;
using GnosisWorld = Gnosis.ECS.World.World;

namespace Genesis.GameSystems;

/// <summary>
/// 统一音频游戏系统
/// 合并 GenesisAudioSystem（3D）和 Genesis2DAudioSystem（2D）
/// 通过 HAL 接口访问实体世界，不再直接依赖 Gnosis.ECS.World.World
/// </summary>
public sealed class AudioGameSystem : ISystem, IWorldSystem
{
    #region 字段

    private GnosisWorld? _world;
    private IEntityWorld? _entityWorld;
    private readonly GnosisAudioSystem _audioSystem;
    private readonly Dictionary<uint, GnosisAudioSource> _entitySources = new();
    private GnosisAudioBus? _sfxBus;
    private GnosisAudioBus? _ambientBus;
    private GnosisAudioBus? _musicBus;
    private bool _initialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.PostUpdate;

    public GnosisAudioSystem GnosisAudioSystem => _audioSystem;

    public float GlobalVolume
    {
        get => _audioSystem.GlobalVolume;
        set => _audioSystem.GlobalVolume = value;
    }

    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    public AudioGameSystem()
    {
        _audioSystem = new GnosisAudioSystem();
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

    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    /// <summary>
    /// 设置 HAL 实体世界
    /// 优先于 GnosisWorld 使用
    /// </summary>
    public void SetEntityWorld(IEntityWorld entityWorld)
    {
        _entityWorld = entityWorld;
    }

    #endregion

    #region ISystem.Update

    public void Update(float delta)
    {
        if (!_initialized)
        {
            return;
        }

        var ew = _entityWorld;
        if (ew is not null)
        {
            SyncNewSourcesFromEntityWorld(ew);
            SyncListenerFromEntityWorld(ew);
        }
        else if (_world is not null)
        {
            SyncNewSourcesFromWorld();
            SyncListenerFromWorld();
        }

        _audioSystem.Update(delta);
    }

    #endregion

    #region 公开方法

    public GnosisAudioBus? GetBus(string name)
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

    #region IEntityWorld 路径

    private void SyncNewSourcesFromEntityWorld(IEntityWorld ew)
    {
        var entities = ew.CreateQuery()
            .All<Transform3D>()
            .All<AudioSourceRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (_entitySources.ContainsKey(entityId.Index))
            {
                continue;
            }

            if (!ew.HasComponent<AudioSourceRef>(entityId))
            {
                continue;
            }

            var audioRef = ew.GetComponent<AudioSourceRef>(entityId);
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

    private void SyncListenerFromEntityWorld(IEntityWorld ew)
    {
        var entities = ew.CreateQuery()
            .All<Transform3D>()
            .All<AudioListenerRef>()
            .Build();

        foreach (var entityId in entities)
        {
            if (!ew.HasComponent<Transform3D>(entityId) || !ew.HasComponent<AudioListenerRef>(entityId))
            {
                continue;
            }

            var transform = ew.GetComponent<Transform3D>(entityId);
            var listenerRef = ew.GetComponent<AudioListenerRef>(entityId);

            var listener = _audioSystem.Listener;
            listener.Position = new Vector3(transform.PosX, transform.PosY, transform.PosZ);
            listener.Volume = listenerRef.Gain;

            break;
        }
    }

    #endregion

    #region GnosisWorld 回退路径

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
