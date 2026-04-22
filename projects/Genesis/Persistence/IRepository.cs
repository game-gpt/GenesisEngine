namespace Genesis.Persistence.Interfaces;

public interface IRepository<T> : SolidDB.Core.IRepository<T> where T : class
{
}
