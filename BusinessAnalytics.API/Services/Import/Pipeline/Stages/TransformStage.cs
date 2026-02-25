using System.Globalization;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 3: Transform parsed rows into domain models.
/// Groups rows into Orders, resolves or creates Customers/Products/Categories.
/// </summary>
public class TransformStage : IImportPipelineStage
{
    private static readonly string[] DateFormats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd" };

    private readonly IUnitOfWork _unitOfWork;

    public TransformStage(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        var customerRepo = _unitOfWork.Repository<Customer, Guid>();
        var categoryRepo = _unitOfWork.Repository<Category, int>();
        var productRepo = _unitOfWork.Repository<Product, Guid>();

        // Load existing entities for this user
        var existingCustomers = (await customerRepo.GetAllAsync())
            .Where(c => c.UserId == context.UserId)
            .ToDictionary(c => c.Email?.ToLowerInvariant() ?? "", c => c);

        var existingCategories = (await categoryRepo.GetAllAsync())
            .Where(c => c.UserId == context.UserId)
            .ToDictionary(c => c.Name.ToLowerInvariant(), c => c);

        var existingProducts = (await productRepo.GetAllAsync())
            .Where(p => p.UserId == context.UserId)
            .ToDictionary(p => p.Name.ToLowerInvariant(), p => p);

        // Group rows by OrderDate + CustomerEmail → one Order per group
        var orderGroups = context.ParsedRows
            .GroupBy(r => new { r.OrderDate, Email = r.CustomerEmail.ToLowerInvariant() });

        foreach (var group in orderGroups)
        {
            var firstRow = group.First();
            var orderDate = DateTime.ParseExact(firstRow.OrderDate, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            // Resolve or create Customer
            var customer = ResolveCustomer(existingCustomers, firstRow, context.UserId, context.CustomersCreated);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = context.UserId,
                CustomerId = customer.Id,
                OrderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc),
                Status = Enum.Parse<OrderStatus>(firstRow.Status, ignoreCase: true),
                UpdatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var row in group)
            {
                // Resolve or create Category
                var category = ResolveCategory(existingCategories, row, context.UserId, context.CategoriesCreated);

                // Resolve or create Product
                var product = ResolveProduct(existingProducts, row, context.UserId, category, context.ProductsCreated);

                var quantity = int.Parse(row.Quantity);
                var unitPrice = decimal.Parse(row.UnitPrice, CultureInfo.InvariantCulture);

                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });

                totalAmount += quantity * unitPrice;
            }

            order.TotalAmount = totalAmount;
            context.Orders.Add(order);
        }

        return context;
    }

    private Customer ResolveCustomer(
        Dictionary<string, Customer> existing,
        Models.DTOs.OrderImportRow row,
        string userId,
        List<Customer> created)
    {
        var emailKey = row.CustomerEmail.ToLowerInvariant();

        if (existing.TryGetValue(emailKey, out var customer))
            return customer;

        // Check if we already created this customer in this import
        var alreadyCreated = created.Find(c => c.Email?.ToLowerInvariant() == emailKey);
        if (alreadyCreated != null)
            return alreadyCreated;

        var newCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = row.CustomerName,
            Email = row.CustomerEmail,
            CreatedAt = DateTime.UtcNow
        };

        existing[emailKey] = newCustomer;
        created.Add(newCustomer);
        return newCustomer;
    }

    private Category ResolveCategory(
        Dictionary<string, Category> existing,
        Models.DTOs.OrderImportRow row,
        string userId,
        List<Category> created)
    {
        var nameKey = row.CategoryName.ToLowerInvariant();

        if (existing.TryGetValue(nameKey, out var category))
            return category;

        var alreadyCreated = created.Find(c => c.Name.ToLowerInvariant() == nameKey);
        if (alreadyCreated != null)
            return alreadyCreated;

        var newCategory = new Category
        {
            UserId = userId,
            Name = row.CategoryName
        };

        existing[nameKey] = newCategory;
        created.Add(newCategory);
        return newCategory;
    }

    private Product ResolveProduct(
        Dictionary<string, Product> existing,
        Models.DTOs.OrderImportRow row,
        string userId,
        Category category,
        List<Product> created)
    {
        var nameKey = row.ProductName.ToLowerInvariant();

        if (existing.TryGetValue(nameKey, out var product))
            return product;

        var alreadyCreated = created.Find(p => p.Name.ToLowerInvariant() == nameKey);
        if (alreadyCreated != null)
            return alreadyCreated;

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = row.ProductName,
            Price = decimal.Parse(row.UnitPrice, CultureInfo.InvariantCulture),
            Category = category
        };

        existing[nameKey] = newProduct;
        created.Add(newProduct);
        return newProduct;
    }
}
