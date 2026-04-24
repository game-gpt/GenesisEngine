using Gnosis.Core.Entity;

namespace Genesis.Integration.Rendering;

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
    public static Transform3D Identity => new(0f, 0f, 0f, 0f, 0f, 0f);

    public Transform3D WithPosition(float x, float y, float z) => this with { PosX = x, PosY = y, PosZ = z };

    public Transform3D WithRotation(float x, float y, float z) => this with { RotX = x, RotY = y, RotZ = z };

    public Transform3D WithScale(float x, float y, float z) => this with { ScaleX = x, ScaleY = y, ScaleZ = z };
}
