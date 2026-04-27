namespace Genesis.Integration.Rendering;

/// <summary>
/// 3D 变换组件
/// </summary>
/// <param name="PosX">X 位置</param>
/// <param name="PosY">Y 位置</param>
/// <param name="PosZ">Z 位置</param>
/// <param name="RotX">X 旋转</param>
/// <param name="RotY">Y 旋转</param>
/// <param name="RotZ">Z 旋转</param>
/// <param name="ScaleX">X 缩放</param>
/// <param name="ScaleY">Y 缩放</param>
/// <param name="ScaleZ">Z 缩放</param>
public readonly record struct Transform3D(
    float PosX,
    float PosY,
    float PosZ,
    float RotX,
    float RotY,
    float RotZ,
    float ScaleX = 1f,
    float ScaleY = 1f,
    float ScaleZ = 1f)
{
    /// <summary>
    /// 单位变换
    /// </summary>
    public static Transform3D Identity => new(0f, 0f, 0f, 0f, 0f, 0f);

    /// <summary>
    /// 设置位置
    /// </summary>
    public Transform3D WithPosition(float x, float y, float z) => this with { PosX = x, PosY = y, PosZ = z };

    /// <summary>
    /// 设置旋转
    /// </summary>
    public Transform3D WithRotation(float x, float y, float z) => this with { RotX = x, RotY = y, RotZ = z };

    /// <summary>
    /// 设置缩放
    /// </summary>
    public Transform3D WithScale(float x, float y, float z) => this with { ScaleX = x, ScaleY = y, ScaleZ = z };
}
