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
            var customerRepo = _unitOfWork.Repository<Customer, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, int>();
            var productRepo = _unitOfWork.Repository<Product, Guid>();
            var orderRepo = _unitOfWork.Repository<Order, Guid>();
            var sessionRepo = _unitOfWork.Repository<ImportSession, Guid>();

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

            await sessionRepo.AddAsync(session);

            // Persist new Categories
            foreach (var category in context.CategoriesCreated)
            {
                await categoryRepo.AddAsync(category);
            }

            // Persist new Customers
            foreach (var customer in context.CustomersCreated)
            {
                await customerRepo.AddAsync(customer);
            }

            // Persist new Products
            foreach (var product in context.ProductsCreated)
            {
                await productRepo.AddAsync(product);
            }

            // Persist Orders with ImportSessionId
            foreach (var order in context.Orders)
            {
                order.ImportSessionId = session.Id;
                await orderRepo.AddAsync(order);
            }

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
}
