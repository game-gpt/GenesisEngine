namespace Genesis.HashLife;

public interface IHashLifeEvolver
{
    ulong Evolve(ulong nodeHash, int steps);
    ulong Evolve(ulong nodeHash, float deltaTime);
    int GetNodeLevel(ulong nodeHash);
    ulong GetEmptyNode(int level);
}
