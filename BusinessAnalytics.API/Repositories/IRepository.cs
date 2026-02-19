namespace BusinessAnalytics.API.Repositories;

public interface IRepository<T, TKey> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(TKey id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
