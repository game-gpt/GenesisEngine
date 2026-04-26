namespace Genesis.Persistence;

public interface IRepository<T> where T : class
{
    Task<T?> FindAsync(string id);
    Task SaveAsync(T entity);
    Task DeleteAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
}
