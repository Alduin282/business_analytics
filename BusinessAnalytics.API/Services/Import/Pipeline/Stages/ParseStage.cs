using BusinessAnalytics.API.Services.Import.Parsing;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

public class ParseStage(FileParserFactory parserFactory) : IImportPipelineStage
{
    private readonly FileParserFactory _parserFactory = parserFactory;

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        try
        {
            var parser = _parserFactory.GetParser(context.FileName);
            
            context.FileStream.Position = 0;
            using var headerReader = new StreamReader(context.FileStream, leaveOpen: true);
            var headerLine = await headerReader.ReadLineAsync();
            
            if (!string.IsNullOrWhiteSpace(headerLine))
            {
                context.Headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            }
            
            context.FileStream.Position = 0;
            context.ParsedRows = await parser.ParseAsync(context.FileStream);
        }
        catch (Exception ex)
        {
            context.Errors.Add(new Validation.ValidationError(0, "File", ex.Message));
            context.IsAborted = true;
        }

        return context;
    }
}
