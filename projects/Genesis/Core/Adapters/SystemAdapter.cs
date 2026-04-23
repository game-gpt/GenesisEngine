namespace Genesis.Core.Adapters;

/// <summary>
///     ISystem 适配器，将 Genesis 的 ISystem 适配为 Gnosis.ECS 的 ISystem
/// </summary>
public sealed class SystemAdapter : Gnosis.ECS.System.ISystem
{
    private readonly ISystem _genesisSystem;

    /// <summary>
    ///     创建系统适配器
    /// </summary>
    public SystemAdapter(ISystem genesisSystem, Gnosis.ECS.System.SystemPhase phase = Gnosis.ECS.System.SystemPhase.Update)
    {
        _genesisSystem = genesisSystem;
        Phase = phase;
    }

    /// <summary>
    ///     系统执行阶段
    /// </summary>
    public Gnosis.ECS.System.SystemPhase Phase { get; }

    /// <summary>
    ///     系统初始化
    /// </summary>
    public void Initialize()
    {
        _genesisSystem.Initialize();
    }

    /// <summary>
    ///     系统帧更新
    /// </summary>
    public void Update(float delta)
    {
        _genesisSystem.Update(delta);
    }

    /// <summary>
    ///     系统关闭
    /// </summary>
    public void Shutdown()
    {
        _genesisSystem.Shutdown();
    }
}

/// <summary>
///     ISystem 扩展方法
/// </summary>
public static class SystemAdapterExtensions
{
    /// <summary>
    ///     将 Genesis 的 ISystem 适配为 Gnosis.ECS 的 ISystem
    /// </summary>
    public static Gnosis.ECS.System.ISystem ToGnosisSystem(
        this ISystem system,
        Gnosis.ECS.System.SystemPhase phase = Gnosis.ECS.System.SystemPhase.Update)
    {
        return new SystemAdapter(system, phase);
    }
}
