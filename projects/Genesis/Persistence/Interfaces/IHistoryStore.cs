using Genesis.Core.ValueObjects;

namespace Genesis.Persistence.Interfaces;

public interface IHistoryStore
{
    Task AppendAsync(ulong historyHash, byte[] data);
    Task<byte[]?> GetAsync(ulong historyHash);
    Task<bool> ExistsAsync(ulong historyHash);
    Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth);
}
