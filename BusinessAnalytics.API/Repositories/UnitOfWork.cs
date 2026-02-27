using System.Collections;
using BusinessAnalytics.API.Data;

namespace BusinessAnalytics.API.Repositories;

/// <summary>
/// Если мы решим сменить EF Core на Dapper или другую библиотеку, 
/// мы просто заменим реализацию этого класса, не меняя контроллеры и сервисы.
/// </summary>
public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly Dictionary<Type, object> _repositories = new();

    public async Task<int> CompleteAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
    }

    public IRepository<T, TKey> Repository<T, TKey>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new Repository<T, TKey>(_context);
            _repositories[type] = repo;
        }

        return (IRepository<T, TKey>)repo;
    }
}
