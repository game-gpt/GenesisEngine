using GnosisPhysicsDynamics = Gnosis.Physics.Dynamics;

namespace Genesis.GameSystems.Components;

public readonly record struct RigidBodyRef(
    string BodyName,
    GnosisPhysicsDynamics.RigidBodyType BodyType = GnosisPhysicsDynamics.RigidBodyType.Dynamic,
    float Mass = 1f,
    bool UseGravity = true,
    bool IsKinematic = false)
{
    public static RigidBodyRef Dynamic => new("body", GnosisPhysicsDynamics.RigidBodyType.Dynamic, 1f, true, false);
    public static RigidBodyRef Static => new("body", GnosisPhysicsDynamics.RigidBodyType.Static, 0f, false, false);
    public static RigidBodyRef Kinematic => new("body", GnosisPhysicsDynamics.RigidBodyType.Kinematic, 1f, false, true);
}
