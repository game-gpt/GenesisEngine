using System.Numerics;
using Gnosis.Core.Math;
using Gnosis.Navigation.NavMesh;
using Gnosis.Navigation.Path;
using Gnosis.Navigation.Query;

namespace Genesis.Integration;

public sealed class NavigationIntegration : IDisposable
{
    #region 字段

    private INavigationSystem? _navigationSystem;
    private readonly Dictionary<string, INavMesh> _navMeshes = new();
    private bool _disposed;

    #endregion

    #region 属性

    public INavigationSystem? System => _navigationSystem;

    public IPathfinder? Pathfinder => _navigationSystem?.Pathfinder;

    public bool IsInitialized => _navigationSystem is not null;

    #endregion

    #region 初始化

    public void Initialize(INavigationSystem? navigationSystem = null)
    {
        _navigationSystem = navigationSystem ?? new NavigationSystem();

        Console.WriteLine("[Genesis] 导航系统初始化完成");
    }

    #endregion

    #region 导航网格

    public void BuildNavMesh(string name, NavMeshBuildSettings settings)
    {
        if (_navigationSystem is null)
        {
            throw new InvalidOperationException("导航系统未初始化，请先调用 Initialize()");
        }

        _navigationSystem.BuildNavMesh(name, settings);

        var navMesh = _navigationSystem.GetNavMesh(name);
        if (navMesh is not null)
        {
            _navMeshes[name] = navMesh;
        }
    }

    public INavMesh? GetNavMesh(string name)
    {
        return _navMeshes.GetValueOrDefault(name);
    }

    public INavMeshQuery CreateQuery(string navMeshName)
    {
        if (_navigationSystem is null)
        {
            throw new InvalidOperationException("导航系统未初始化，请先调用 Initialize()");
        }

        var navMesh = GetNavMesh(navMeshName);

        if (navMesh is null)
        {
            throw new ArgumentException($"导航网格不存在：{navMeshName}", nameof(navMeshName));
        }

        return _navigationSystem.CreateQuery(navMesh);
    }

    #endregion

    #region 寻路

    public IPath FindPath(Vector3 start, Vector3 end)
    {
        if (_navigationSystem is null)
        {
            throw new InvalidOperationException("导航系统未初始化，请先调用 Initialize()");
        }

        return _navigationSystem.Pathfinder.FindPath(start, end);
    }

    public IPath FindPath(IPathRequest request)
    {
        if (_navigationSystem is null)
        {
            throw new InvalidOperationException("导航系统未初始化，请先调用 Initialize()");
        }

        return _navigationSystem.Pathfinder.FindPath(request);
    }

    #endregion

    #region 更新

    public void Update(float delta)
    {
        _navigationSystem?.Update(delta);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _navMeshes.Clear();
        _disposed = true;
    }

    #endregion
}
