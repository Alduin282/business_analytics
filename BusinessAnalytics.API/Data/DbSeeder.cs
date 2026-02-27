using Microsoft.EntityFrameworkCore;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Data;

public class DbSeeder
{
    // Generation Parameters
    public const int DaysToHistory = 365;
    public const int MinOrdersPerDay = 1;
    public const int MaxOrdersPerDay = 10;
    public const double SeasonalWaveIntensity = 0.2;
    public const double GrowthTrendMultiplier = 1.0;
    public const int MinItemsPerOrder = 1;
    public const int MaxItemsPerOrder = 2;
    public const int TotalCustomersToGenerate = 1000;
    
    // Status Distribution
    public const int SuccessRate = 80;
    public const int ShippingRate = 10;
    public const int ProcessingRate = 5;

    private readonly ApplicationDbContext _context;
    private readonly Random _random;

    public DbSeeder(ApplicationDbContext context, Random? random = null)
    {
        _context = context;
        _random = random ?? new Random();
    }

    public async Task SeedAsync(string userId)
    {
        await CleanupAsync(userId);

        var categories = await CreateCategoriesAndProductsAsync(userId);

        var customers = await CreateCustomersAsync(userId, TotalCustomersToGenerate);
        
        await GenerateOrdersAsync(userId, categories, customers);

        await _context.SaveChangesAsync();
    }

    private async Task CleanupAsync(string userId)
    {
        var orders = _context.Orders.Where(o => o.UserId == userId);
        _context.Orders.RemoveRange(orders);

        var products = _context.Products.Where(p => p.UserId == userId);
        _context.Products.RemoveRange(products);

        var customers = _context.Customers.Where(c => c.UserId == userId);
        _context.Customers.RemoveRange(customers);

        var categories = _context.Categories.Where(c => c.UserId == userId);
        _context.Categories.RemoveRange(categories);
    }

    private async Task<List<Category>> CreateCategoriesAndProductsAsync(string userId)
    {
        var equipment = new Category { Name = "Fitness Equipment", UserId = userId, Description = "Heavy gym machines and equipment" };
        var apparel = new Category { Name = "Sports Apparel", UserId = userId, Description = "Clothing for various sports activities" };
        var accessories = new Category { Name = "Accessories", UserId = userId, Description = "Small sports gear and accessories" };

        _context.Categories.AddRange(equipment, apparel, accessories);
        await _context.SaveChangesAsync();

        var products = new List<Product>
        {
            // Equipment (3000-5000)
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Treadmill X100", Price = 5000, Category = equipment },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Professional Dumbbells Set", Price = 3500, Category = equipment },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Exercise Bike Pro", Price = 4500, Category = equipment },
            
            // Apparel (1500-3000)
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Premium Yoga Pants", Price = 2200, Category = apparel },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Pro Running Jersey", Price = 1800, Category = apparel },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "All-Weather Windbreaker", Price = 2900, Category = apparel },

            // Accessories (500-1500)
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Smart Water Bottle", Price = 800, Category = accessories },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "High-Density Gym Mat", Price = 1200, Category = accessories },
            new() { Id = Guid.NewGuid(), UserId = userId, Name = "Professional Skipping Rope", Price = 600, Category = accessories }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        return new List<Category> { equipment, apparel, accessories };
    }

    private async Task<List<Customer>> CreateCustomersAsync(string userId, int count)
    {
        var customers = new List<Customer>();
        string[] firstNames = ["James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "William", "Elizabeth"];
        string[] lastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez"];

        for (int i = 0; i < count; i++)
        {
            var firstName = firstNames[_random.Next(firstNames.Length)];
            var lastName = lastNames[_random.Next(lastNames.Length)];
            
            customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullName = $"{firstName} {lastName}",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, DaysToHistory))
            });
        }

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
        return customers;
    }

    private async Task GenerateOrdersAsync(string userId, List<Category> categories, List<Customer> customers)
    {
        var allProducts = await _context.Products.Where(p => p.UserId == userId).ToListAsync();
        DateTime startDate = DateTime.UtcNow.AddDays(-DaysToHistory);

        for (int day = 0; day <= DaysToHistory; day++)
        {
            DateTime currentDate = startDate.AddDays(day);
            
            int baseCount = _random.Next(MinOrdersPerDay, MaxOrdersPerDay + 1);

            double growthFactor = 1.0 + (double)day / DaysToHistory * GrowthTrendMultiplier;
            double seasonalFactor = 1.0 + Math.Sin(day / 30.0) * SeasonalWaveIntensity;
            double weekendFactor = (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday) ? 1.5 : 1.0;

            int dailyOrderCount = (int)(baseCount * growthFactor * seasonalFactor * weekendFactor);

            for (int i = 0; i < dailyOrderCount; i++)
            {
                var customer = customers[_random.Next(customers.Count)];
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CustomerId = customer.Id,
                    OrderDate = currentDate.AddHours(_random.Next(8, 22)).AddMinutes(_random.Next(0, 60)),
                    Status = GetRandomStatus(),
                    UpdatedAt = currentDate
                };

                int itemsInOrderCount = _random.Next(MinItemsPerOrder, MaxItemsPerOrder + 1);
                decimal orderTotal = 0;
                
                for (int j = 0; j < itemsInOrderCount; j++)
                {
                    var product = allProducts[_random.Next(allProducts.Count)];
                    var quantity = _random.Next(1, 3);
                    
                    order.Items.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    });
                    orderTotal += product.Price * quantity;
                }
                order.TotalAmount = orderTotal;
                _context.Orders.Add(order);
            }
        }
    }

    private OrderStatus GetRandomStatus()
    {
        int roll = _random.Next(0, 100);
        if (roll < SuccessRate) return OrderStatus.Delivered;
        if (roll < SuccessRate + ShippingRate) return OrderStatus.Shipped;
        if (roll < SuccessRate + ShippingRate + ProcessingRate) return OrderStatus.Processing;
        return OrderStatus.Cancelled;
    }
}
