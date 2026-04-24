using Gnosis.Audio.Driver;
using Gnosis.Audio.Clip;
using Gnosis.Audio.Source;
using Gnosis.Audio.Mixer;

namespace Genesis.Integration;

public sealed class AudioIntegration : IDisposable
{
    #region 字段

    private IAudioSystem? _audioSystem;
    private readonly Dictionary<string, IAudioSource> _sources = new();
    private readonly Dictionary<string, IAudioClip> _clips = new();
    private bool _disposed;

    #endregion

    #region 属性

    public IAudioSystem? System => _audioSystem;

    public IAudioBus? MasterBus => _audioSystem?.MasterBus;

    public bool IsInitialized => _audioSystem is not null;

    #endregion

    #region 初始化

    public void Initialize(IAudioSystem? audioSystem = null)
    {
        _audioSystem = audioSystem ?? new AudioSystem();

        Console.WriteLine("[Genesis] 音频系统初始化完成");
    }

    #endregion

    #region 音频源管理

    public IAudioSource CreateSource(string name)
    {
        if (_audioSystem is null)
        {
            throw new InvalidOperationException("音频系统未初始化，请先调用 Initialize()");
        }

        var source = _audioSystem.CreateSource();
        _sources[name] = source;
        return source;
    }

    public void DestroySource(string name)
    {
        if (_audioSystem is null)
        {
            return;
        }

        if (_sources.TryGetValue(name, out var source))
        {
            _audioSystem.DestroySource(source);
            _sources.Remove(name);
        }
    }

    public IAudioSource? GetSource(string name)
    {
        return _sources.GetValueOrDefault(name);
    }

    #endregion

    #region 音频片段管理

    public IAudioClip LoadClip(string name, string path)
    {
        if (_audioSystem is null)
        {
            throw new InvalidOperationException("音频系统未初始化，请先调用 Initialize()");
        }

        var clip = _audioSystem.LoadClip(path);
        _clips[name] = clip;
        return clip;
    }

    public void UnloadClip(string name)
    {
        if (_audioSystem is null)
        {
            return;
        }

        if (_clips.TryGetValue(name, out var clip))
        {
            _audioSystem.UnloadClip(clip.Name);
            _clips.Remove(name);
        }
    }

    #endregion

    #region 播放控制

    public void Play(string sourceName)
    {
        if (_sources.TryGetValue(sourceName, out var source))
        {
            source.Play();
        }
    }

    public void Stop(string sourceName)
    {
        if (_sources.TryGetValue(sourceName, out var source))
        {
            source.Stop();
        }
    }

    public void Pause(string sourceName)
    {
        if (_sources.TryGetValue(sourceName, out var source))
        {
            source.Pause();
        }
    }

    #endregion

    #region 音频总线

    public IAudioBus CreateBus(string name, string? parentName = null)
    {
        if (_audioSystem is null)
        {
            throw new InvalidOperationException("音频系统未初始化，请先调用 Initialize()");
        }

        var parent = parentName is not null ? _audioSystem.GetBus(parentName) : null;
        return _audioSystem.CreateBus(name, parent);
    }

    #endregion

    #region 更新

    public void Update(float delta)
    {
        _audioSystem?.Update(delta);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var source in _sources.Values)
        {
            _audioSystem?.DestroySource(source);
        }

        _sources.Clear();
        _clips.Clear();

        _disposed = true;
    }

    #endregion
}
