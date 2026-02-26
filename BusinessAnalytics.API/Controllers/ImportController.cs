using System.Security.Claims;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly ImportPipeline _pipeline;
    private readonly IUnitOfWork _uow;

    public ImportController(ImportPipeline pipeline, IUnitOfWork uow)
    {
        _pipeline = pipeline;
        _uow = uow;
    }

    [HttpPost("orders")]
    public async Task<ActionResult<ImportResult>> ImportOrders(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = new() { new Services.Import.Validation.ValidationError(0, "File", "No file uploaded") }
            });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var context = new ImportContext
        {
            FileStream = stream,
            FileName = file.FileName,
            UserId = userId
        };

        var result = await _pipeline.ExecuteAsync(context);

        var importResult = result.ToResult();
        
        if (!importResult.Success)
            return BadRequest(importResult);

        return Ok(importResult);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<ImportSessionDto>>> GetHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var sessions = await _uow.Repository<ImportSession, Guid>()
            .Query()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.ImportedAt)
            .Select(s => new ImportSessionDto
            {
                Id = s.Id,
                FileName = s.FileName,
                ImportedAt = s.ImportedAt,
                OrdersCount = s.OrdersCount,
                ItemsCount = s.ItemsCount
            })
            .ToListAsync();

        return Ok(sessions);
    }
}
