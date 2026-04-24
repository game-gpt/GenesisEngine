namespace Genesis.Integration.Navigation;

public readonly record struct NavMeshRef(
    string MeshName = "default",
    float AgentRadius = 0.5f,
    float AgentHeight = 2f,
    float StepHeight = 0.4f,
    float SlopeAngle = 45f,
    float VoxelSize = 0.15f,
    float RegionMinArea = 8f)
{
    public static NavMeshRef Default => new();
    public bool IsValid => !string.IsNullOrEmpty(MeshName);
}
