namespace Genesis.Spacetime.Interfaces;

public interface IHistoryHash
{
    ulong Value { get; }
    void Update(ulong newHash);
    ulong Combine(ulong otherHash);
}
