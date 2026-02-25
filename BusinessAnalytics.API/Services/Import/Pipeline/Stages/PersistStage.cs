using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 4: Persist all entities to the database in a single transaction.
/// Creates an ImportSession and links all orders to it.
/// Rolls back everything if any error occurs (atomic import).
/// </summary>
public class PersistStage : IImportPipelineStage
{
    private readonly IUnitOfWork _unitOfWork;

    public PersistStage(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        try
        {
            // Create ImportSession
            var session = new ImportSession
            {
                Id = Guid.NewGuid(),
                UserId = context.UserId,
                FileName = context.FileName,
                ImportedAt = DateTime.UtcNow,
                OrdersCount = context.Orders.Count,
                ItemsCount = context.Orders.Sum(o => o.Items.Count)
            };

            await _unitOfWork.Repository<ImportSession, Guid>().AddAsync(session);

            // Persist all entities using generic helper
            await PersistEntitiesAsync<Category, int>(context.CategoriesCreated);
            await PersistEntitiesAsync<Customer, Guid>(context.CustomersCreated);
            await PersistEntitiesAsync<Product, Guid>(context.ProductsCreated);
            await PersistEntitiesAsync<Order, Guid>(context.Orders, o => o.ImportSessionId = session.Id);

            await _unitOfWork.CompleteAsync();

            context.Session = session;
        }
        catch (Exception ex)
        {
            context.Errors.Add(new Validation.ValidationError(0, "Database",
                $"Failed to save data: {ex.Message}"));
            context.IsAborted = true;
        }

        return context;
    }

    private async Task PersistEntitiesAsync<T, TKey>(IEnumerable<T> entities, Action<T>? prepare = null) where T : class
    {
        var repo = _unitOfWork.Repository<T, TKey>();
        foreach (var entity in entities)
        {
            prepare?.Invoke(entity);
            await repo.AddAsync(entity);
        }
    }
}
