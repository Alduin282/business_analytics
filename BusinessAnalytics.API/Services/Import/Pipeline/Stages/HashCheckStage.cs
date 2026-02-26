using Microsoft.EntityFrameworkCore;
using BusinessAnalytics.API.Models;
using BusinessAnalytics.API.Repositories;
using BusinessAnalytics.API.Services.Import.Validation;
using System.Security.Cryptography;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 0: Calculate file hash and check for duplicates.
/// Prevents importing the same file multiple times by the same user.
/// </summary>
public class HashCheckStage : IImportPipelineStage
{
    private readonly IUnitOfWork _unitOfWork;

    public HashCheckStage(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        if (context.FileStream == null || context.FileStream.Length == 0)
        {
            return context;
        }

        context.FileStream.Position = 0;
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(context.FileStream);
        context.FileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        
        // Check for existing import session with the same hash and user
        var repository = _unitOfWork.Repository<ImportSession, Guid>();
        
        var duplicate = await repository.Query()
            .FirstOrDefaultAsync(s => 
                s.UserId == context.UserId && 
                s.FileHash == context.FileHash &&
                !s.IsRolledBack);

        if (duplicate != null)
        {
            context.Errors.Add(new ValidationError(0, "File", 
                $"This file has already been imported on {duplicate.ImportedAt:yyyy-MM-dd HH:mm} (Session ID: {duplicate.Id})"));
            context.IsAborted = true;
        }

        return context;
    }
}
