using Gnosis.Core.Math;
using Gnosis.Core.Time;

namespace Genesis.Core.Adapters;

/// <summary>
///     Position 适配器，在 Genesis 的 double 精度与 Gnosis 的 float 精度 Vector3 之间转换
/// </summary>
public static class PositionAdapter
{
    /// <summary>
    ///     将 Genesis 的 Position 转换为 Gnosis 的 Vector3（精度降级 float → double）
    /// </summary>
    public static Vector3 ToVector3(this Position position)
    {
        return new Vector3((float)position.X, (float)position.Y, (float)position.Z);
    }

    /// <summary>
    ///     将 Gnosis 的 Vector3 转换为 Genesis 的 Position（精度升级 float → double）
    /// </summary>
    public static Position ToGenesisPosition(this Vector3 vector)
    {
        return new Position(vector.X, vector.Y, vector.Z);
    }
}

/// <summary>
///     Timestamp 适配器，在 Genesis 的 DateTime 与 Gnosis 的 DateTimeOffset 之间转换
/// </summary>
public static class TimestampAdapter
{
    /// <summary>
    ///     将 Genesis 的 Timestamp 转换为 Gnosis 的 Timestamp
    /// </summary>
    public static Gnosis.Core.Time.Timestamp ToGnosisTimestamp(this Timestamp timestamp)
    {
        return new Gnosis.Core.Time.Timestamp(new DateTimeOffset(timestamp.Value, TimeSpan.Zero));
    }

    /// <summary>
    ///     将 Gnosis 的 Timestamp 转换为 Genesis 的 Timestamp
    /// </summary>
    public static Timestamp ToGenesisTimestamp(this Gnosis.Core.Time.Timestamp timestamp)
    {
        return new Timestamp(timestamp.Value.UtcDateTime);
    }
}
