namespace Genesis.Persistence;

public interface IRepository<T> : SolidDB.Core.IRepository<T> where T : class
{
}
