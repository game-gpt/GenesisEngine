namespace Genesis.Persistence;

public interface IIncrementalSave
{
    Task SaveAsync();
    Task SaveAsync(string checkpointName);
    Task<bool> LoadAsync(string checkpointName);
    Task<IEnumerable<string>> GetCheckpointsAsync();
    Task DeleteCheckpointAsync(string checkpointName);
}
