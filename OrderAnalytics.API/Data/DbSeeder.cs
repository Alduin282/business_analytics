using Microsoft.EntityFrameworkCore;
using BusinessAnalytics.API.Models;

namespace BusinessAnalytics.API.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();

    public DbSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(string userId)
    {
        // 1. Cleanup existing data for this user to avoid duplication
        await CleanupAsync(userId);

        // 2. Create Categories & Products
        var categories = await CreateCategoriesAndProductsAsync(userId);

        // 3. Create Customers
        var customers = await CreateCustomersAsync(userId, 1000);

        // 4. Generate Orders for the past year
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
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Treadmill X100", Price = 5000, Category = equipment },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Professional Dumbbells Set", Price = 3500, Category = equipment },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Exercise Bike Pro", Price = 4500, Category = equipment },
            
            // Apparel (1500-3000)
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Premium Yoga Pants", Price = 2200, Category = apparel },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Pro Running Jersey", Price = 1800, Category = apparel },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "All-Weather Windbreaker", Price = 2900, Category = apparel },

            // Accessories (500-1500)
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Smart Water Bottle", Price = 800, Category = accessories },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "High-Density Gym Mat", Price = 1200, Category = accessories },
            new Product { Id = Guid.NewGuid(), UserId = userId, Name = "Professional Skipping Rope", Price = 600, Category = accessories }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        return new List<Category> { equipment, apparel, accessories };
    }

    private async Task<List<Customer>> CreateCustomersAsync(string userId, int count)
    {
        var customers = new List<Customer>();
        string[] firstNames = { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", "Michael", "Linda", "William", "Elizabeth" };
        string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

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
                CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365))
            });
        }

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
        return customers;
    }

    private async Task GenerateOrdersAsync(string userId, List<Category> categories, List<Customer> customers)
    {
        var allProducts = await _context.Products.Where(p => p.UserId == userId).ToListAsync();
        DateTime startDate = DateTime.UtcNow.AddDays(-365);

        for (int day = 0; day <= 365; day++)
        {
            DateTime currentDate = startDate.AddDays(day);
            
            // 1. Determine base order count (1-10)
            int baseCount = _random.Next(1, 11);

            // 2. Trends: Growth + Sine wave (seasonal)
            double growthTrend = 1.0 + (double)day / 365.0; // Slow growth from 1x to 2x
            double seasonalWave = 1.0 + Math.Sin(day / 30.0) * 0.2; // 20% wave every ~month
            double weekendBoost = (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday) ? 1.5 : 1.0;

            int dailyOrderCount = (int)(baseCount * growthTrend * seasonalWave * weekendBoost);

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

                // 3. Add 1-2 random products
                int itemsCount = _random.Next(1, 3);
                decimal total = 0;
                for (int j = 0; j < itemsCount; j++)
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
                    total += product.Price * quantity;
                }
                order.TotalAmount = total;
                _context.Orders.Add(order);
            }
        }
    }

    private OrderStatus GetRandomStatus()
    {
        int roll = _random.Next(0, 100);
        if (roll < 80) return OrderStatus.Delivered; // 80% success
        if (roll < 90) return OrderStatus.Shipped;   // 10% in transit
        if (roll < 95) return OrderStatus.Processing; // 5% processing
        return OrderStatus.Cancelled;                 // 5% cancelled
    }
}
