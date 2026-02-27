using System.Security.Claims;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Pipeline;
using BusinessAnalytics.API.Services.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController(IImportPipeline pipeline, IUnitOfWork uow, IImportEventDispatcher dispatcher) : ControllerBase
{
    private readonly IImportPipeline _pipeline = pipeline;
    private readonly IUnitOfWork _uow = uow;
    private readonly IImportEventDispatcher _dispatcher = dispatcher;

    [HttpPost("orders")]
    public async Task<ActionResult<ImportResult>> ImportOrders(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ImportResult
            {
                Success = false,
                Errors = [new Services.Import.Validation.ValidationError(0, "File", "No file uploaded")]
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
                ItemsCount = s.ItemsCount,
                IsRolledBack = s.IsRolledBack
            })
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpPost("rollback/{id}")]
    public async Task<IActionResult> Rollback(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var session = await _uow.Repository<ImportSession, Guid>()
            .Query()
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session == null)
            return NotFound();

        session.IsRolledBack = !session.IsRolledBack;
        await _uow.CompleteAsync();

        await _dispatcher.NotifyAsync(new ImportActivityEvent(
            userId,
            session.IsRolledBack ? ImportAction.RolledBack : ImportAction.Imported,
            session.Id,
            session.FileName,
            DateTime.UtcNow,
            session.OrdersCount,
            session.IsRolledBack ? "User rolled back the session" : "User restored the session"
        ));

        return Ok(new { success = true, isRolledBack = session.IsRolledBack });
    }
}
