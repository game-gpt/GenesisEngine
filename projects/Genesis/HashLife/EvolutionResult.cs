using Genesis.Core;

namespace Genesis.HashLife;

public sealed class EvolutionResult : IEvolutionResult
{
    #region 属性

    public ulong ResultHash { get; }
    public Timestamp EvolvedAt { get; }
    public float TimeDelta { get; }
    public bool IsValid { get; }

    #endregion

    #region 构造函数

    public EvolutionResult(ulong resultHash, float timeDelta)
    {
        ResultHash = resultHash;
        EvolvedAt = Timestamp.Now;
        TimeDelta = timeDelta;
        IsValid = resultHash != 0;
    }

    public static EvolutionResult Invalid => new(0, 0);

    #endregion
}
