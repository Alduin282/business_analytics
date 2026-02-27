using System.Diagnostics;

namespace BusinessAnalytics.API.Services.Import.Pipeline;

public class PerformanceImportPipelineDecorator(IImportPipeline inner, ILogger<PerformanceImportPipelineDecorator> logger) : IImportPipeline
{
    private readonly IImportPipeline _inner = inner;
    private readonly ILogger<PerformanceImportPipelineDecorator> _logger = logger;

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var sessionId = context.Session?.Id ?? Guid.NewGuid();
        
        try
        {
            _logger.LogInformation("[PERFORMANCE] Starting import {SessionId} for file {FileName}", 
                sessionId, context.FileName);

            var result = await _inner.ExecuteAsync(context);
            
            stopwatch.Stop();
            
            var ordersCount = result.Session?.OrdersCount ?? 0;
            var itemsCount = result.Session?.ItemsCount ?? 0;
            
            _logger.LogInformation("[PERFORMANCE] Import {SessionId} completed in {Duration}ms. Orders: {Orders}, Items: {Items}, Success: {Success}", 
                sessionId, stopwatch.ElapsedMilliseconds, ordersCount, itemsCount, !result.HasErrors && !result.IsAborted);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[PERFORMANCE] Import {SessionId} failed after {Duration}ms", 
                sessionId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
