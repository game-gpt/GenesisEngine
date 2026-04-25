using System.Numerics;
using Genesis.Integration.Rendering;
using Gnosis.Core.Entity;
using Gnosis.ECS.System;
using Gnosis.ECS.World;

namespace Genesis.Integration.Physics;

public sealed class GenesisPhysicsSystem : ISystem, IWorldSystem
{
    #region 字段

    private Gnosis.ECS.World.World? _world;
    private bool _isInitialized;

    #endregion

    #region 属性

    public SystemPhase Phase => SystemPhase.Update;

    #endregion

    #region ISystem 实现

    public void Initialize()
    {
        _isInitialized = true;
    }

    public void Shutdown()
    {
        _isInitialized = false;
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
    }

    #endregion
}
