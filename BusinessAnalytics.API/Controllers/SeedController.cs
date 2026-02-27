using Microsoft.AspNetCore.Mvc;
using BusinessAnalytics.API.Data;

namespace BusinessAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController(DbSeeder seeder) : ControllerBase
{
    private readonly DbSeeder _seeder = seeder;

    [HttpPost]
    public async Task<IActionResult> Seed([FromQuery] string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("UserId is required.");
        }

        try
        {
            await _seeder.SeedAsync(userId);
            return Ok($"Database seeded successfully with sports goods data for user {userId}!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
