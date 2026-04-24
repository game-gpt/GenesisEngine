using Gnosis.Physics.Dynamics;

namespace Genesis.Integration.Physics;

public readonly record struct RigidBodyRef(
    string BodyName,
    RigidBodyType BodyType = RigidBodyType.Dynamic,
    float Mass = 1f,
    bool UseGravity = true,
    bool IsKinematic = false)
{
    public static RigidBodyRef Dynamic => new("body", RigidBodyType.Dynamic, 1f, true, false);
    public static RigidBodyRef Static => new("body", RigidBodyType.Static, 0f, false, false);
    public static RigidBodyRef Kinematic => new("body", RigidBodyType.Kinematic, 1f, false, true);
}
