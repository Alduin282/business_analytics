namespace BusinessAnalytics.API.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<T, TKey> Repository<T, TKey>() where T : class;
    
    Task<int> CompleteAsync();
}
