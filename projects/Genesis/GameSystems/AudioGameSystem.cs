using System.Numerics;
using Genesis.Core;
using Genesis.Integration.Audio;
using Genesis.Integration.Rendering;
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
    private readonly GnosisAudioSystem _audioSystem;
    private readonly Dictionary<uint, GnosisAudioSource> _entitySources = new();
    private GnosisAudioBus? _sfxBus;
    private GnosisAudioBus? _ambientBus;
    private GnosisAudioBus? _musicBus;
    private bool _initialized;

    #endregion

    #region 属性

    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public SystemPhase Phase => SystemPhase.PostUpdate;

    /// <summary>
    /// 底层 Gnosis 音频系统
    /// </summary>
    public GnosisAudioSystem GnosisAudioSystem => _audioSystem;

    /// <summary>
    /// 全局音量
    /// </summary>
    public float GlobalVolume
    {
        get => _audioSystem.GlobalVolume;
        set => _audioSystem.GlobalVolume = value;
    }

    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化音频游戏系统
    /// </summary>
    public AudioGameSystem()
    {
        _audioSystem = new GnosisAudioSystem();
    }

    #endregion

    #region ISystem 实现

    /// <summary>
    /// 初始化音频系统
    /// </summary>
    public void Initialize()
    {
        _sfxBus = _audioSystem.CreateBus("SFX", _audioSystem.MasterBus);
        _ambientBus = _audioSystem.CreateBus("Ambient", _audioSystem.MasterBus);
        _musicBus = _audioSystem.CreateBus("Music", _audioSystem.MasterBus);
        _initialized = true;
    }

    /// <summary>
    /// 关闭音频系统
    /// </summary>
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

    /// <summary>
    /// 设置系统所属的 World
    /// </summary>
    public void SetWorld(GnosisWorld world)
    {
        _world = world;
    }

    #endregion

    #region ISystem.Update

    /// <summary>
    /// 帧更新
    /// </summary>
    /// <param name="delta">帧间隔时间（秒）</param>
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

    /// <summary>
    /// 获取音频总线
    /// </summary>
    /// <param name="name">总线名称</param>
    /// <returns>音频总线</returns>
    public GnosisAudioBus? GetBus(string name)
    {
        return _audioSystem.GetBus(name);
    }

    /// <summary>
    /// 播放实体音频源
    /// </summary>
    /// <param name="entityIndex">实体索引</param>
    public void PlaySource(uint entityIndex)
    {
        if (_entitySources.TryGetValue(entityIndex, out var source))
        {
            source.Play();
        }
    }

    /// <summary>
    /// 停止实体音频源
    /// </summary>
    /// <param name="entityIndex">实体索引</param>
    public void StopSource(uint entityIndex)
    {
        if (_entitySources.TryGetValue(entityIndex, out var source))
        {
            source.Stop();
        }
    }

    /// <summary>
    /// 设置总线音量
    /// </summary>
    /// <param name="busName">总线名称</param>
    /// <param name="volume">音量</param>
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
