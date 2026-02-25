using System.Security.Claims;
using BusinessAnalytics.API.Models.DTOs;
using BusinessAnalytics.API.Services.Import.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly ImportPipeline _pipeline;

    public ImportController(ImportPipeline pipeline)
    {
        _pipeline = pipeline;
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
}
