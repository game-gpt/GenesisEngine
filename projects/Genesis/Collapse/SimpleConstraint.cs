using Genesis.Core;

namespace Genesis.Collapse;

public sealed class SimpleConstraint : IConstraint
{
    #region 属性

    public string Name { get; }
    public int Priority { get; }
    public ulong AllowedMask { get; }

    #endregion

    #region 构造函数

    public SimpleConstraint(string name, int priority, ulong allowedMask)
    {
        Name = name;
        Priority = priority;
        AllowedMask = allowedMask;
    }

    #endregion

    #region IConstraint 实现

    public bool Validate(int cellIndex, ulong cellMask, IWaveFunctionState state)
    {
        return (cellMask & AllowedMask) != 0;
    }

    public ulong Apply(int cellIndex, ulong cellMask, IWaveFunctionState state)
    {
        return cellMask & AllowedMask;
    }

    #endregion
}
