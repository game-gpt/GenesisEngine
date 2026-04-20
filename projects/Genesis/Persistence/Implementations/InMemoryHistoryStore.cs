using Genesis.Core.ValueObjects;
using Genesis.Persistence.Interfaces;

namespace Genesis.Persistence.Implementations;

public class InMemoryHistoryStore : IHistoryStore
{
    private readonly Dictionary<ulong, byte[]> _store = new();

    public Task AppendAsync(ulong historyHash, byte[] data)
    {
        _store[historyHash] = data;
        return Task.CompletedTask;
    }

    public Task<byte[]?> GetAsync(ulong historyHash)
    {
        _store.TryGetValue(historyHash, out var data);
        return Task.FromResult(data);
    }

    public Task<bool> ExistsAsync(ulong historyHash)
    {
        return Task.FromResult(_store.ContainsKey(historyHash));
    }

    public Task<IEnumerable<ulong>> GetHistoryChainAsync(ulong startHash, int maxDepth)
    {
        var chain = new List<ulong>();
        var current = startHash;
        var depth = 0;

        while (_store.ContainsKey(current) && depth < maxDepth)
        {
            chain.Add(current);
            depth++;
        }

        return Task.FromResult(chain.AsEnumerable());
    }
}
