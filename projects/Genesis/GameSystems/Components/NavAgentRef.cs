namespace Genesis.GameSystems.Components;

public readonly record struct NavAgentRef(
    float AgentRadius = 0.5f,
    float AgentHeight = 2f,
    float StepHeight = 0.4f,
    float Speed = 3.5f,
    float StoppingDistance = 0.5f,
    bool AutoBraking = true)
{
    public static NavAgentRef Default => new();
}
