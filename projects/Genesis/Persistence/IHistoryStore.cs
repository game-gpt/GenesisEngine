namespace Genesis.Persistence;

public interface IHistoryStore
{
    Task AppendAsync(ulong historyHash, byte[] data);
    Task<byte[]?> GetAsync(ulong historyHash);
    Task<bool> ExistsAsync(ulong historyHash);
    Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth);
}
