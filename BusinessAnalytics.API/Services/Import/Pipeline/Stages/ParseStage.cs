using BusinessAnalytics.API.Services.Import.Parsing;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 1: Parse the uploaded file into OrderImportRow DTOs using the appropriate parser.
/// </summary>
public class ParseStage : IImportPipelineStage
{
    private readonly FileParserFactory _parserFactory;

    public ParseStage(FileParserFactory parserFactory)
    {
        _parserFactory = parserFactory;
    }

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        try
        {
            var parser = _parserFactory.GetParser(context.FileName);
            
            // Read the header line separately before parsing to pass to validators
            context.FileStream.Position = 0;
            using var headerReader = new StreamReader(context.FileStream, leaveOpen: true);
            var headerLine = await headerReader.ReadLineAsync();
            
            if (!string.IsNullOrWhiteSpace(headerLine))
            {
                context.Headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            }
            
            // Reset stream and parse
            context.FileStream.Position = 0;
            context.ParsedRows = await parser.ParseAsync(context.FileStream);
        }
        catch (NotSupportedException ex)
        {
            context.Errors.Add(new Validation.ValidationError(0, "File", ex.Message));
            context.IsAborted = true;
        }

        return context;
    }
}
