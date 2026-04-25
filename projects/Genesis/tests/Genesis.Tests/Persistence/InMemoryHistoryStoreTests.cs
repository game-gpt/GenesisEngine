using Genesis.Persistence;
using Xunit;

namespace Genesis.Tests.Persistence;

public class InMemoryHistoryStoreTests
{
    [Fact]
    public async Task AppendAsync_ThenGetAsync_ReturnsData()
    {
        var store = new InMemoryHistoryStore();
        var data = new byte[] { 1, 2, 3, 4, 5 };

        await store.AppendAsync(42, data);
        var result = await store.GetAsync(42);

        Assert.Equal(data, result);
    }

    [Fact]
    public async Task GetAsync_NonExistingKey_ReturnsNull()
    {
        var store = new InMemoryHistoryStore();

        var result = await store.GetAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        var store = new InMemoryHistoryStore();
        await store.AppendAsync(42, [1, 2, 3]);

        var exists = await store.ExistsAsync(42);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonExistingKey_ReturnsFalse()
    {
        var store = new InMemoryHistoryStore();

        var exists = await store.ExistsAsync(999);

        Assert.False(exists);
    }

    [Fact]
    public async Task AppendAsync_OverwriteExistingKey()
    {
        var store = new InMemoryHistoryStore();
        await store.AppendAsync(42, [1, 2, 3]);
        var newData = new byte[] { 4, 5, 6 };

        await store.AppendAsync(42, newData);
        var result = await store.GetAsync(42);

        Assert.Equal(newData, result);
    }

    [Fact]
    public async Task GetHistoryChainAsync_ReturnsChain()
    {
        var store = new InMemoryHistoryStore();
        await store.AppendAsync(1, [1]);
        await store.AppendAsync(2, [2]);
        await store.AppendAsync(3, [3]);

        var chain = await store.GetHistoryChainAsync(1, 10);

        Assert.Single(chain);
        Assert.Equal(1UL, chain.First());
    }

    [Fact]
    public async Task GetHistoryChainAsync_RespectsMaxDepth()
    {
        var store = new InMemoryHistoryStore();
        await store.AppendAsync(1, [1]);

        var chain = await store.GetHistoryChainAsync(1, 0);

        Assert.Empty(chain);
    }

    [Fact]
    public async Task GetHistoryChainAsync_NonExistingStart_ReturnsEmpty()
    {
        var store = new InMemoryHistoryStore();

        var chain = await store.GetHistoryChainAsync(999, 10);

        Assert.Empty(chain);
    }
}
