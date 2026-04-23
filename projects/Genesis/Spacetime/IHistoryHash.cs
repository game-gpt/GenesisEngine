namespace Genesis.Spacetime;

public interface IHistoryHash
{
    ulong Value { get; }
    void Update(ulong newHash);
    ulong Combine(ulong otherHash);
}
