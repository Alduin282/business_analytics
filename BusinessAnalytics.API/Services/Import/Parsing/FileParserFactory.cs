namespace BusinessAnalytics.API.Services.Import.Parsing;

public class FileParserFactory(IEnumerable<IFileParser> parsers)
{
    private readonly Dictionary<string, IFileParser> _parsers = parsers.ToDictionary(
            p => p.SupportedExtension.ToLowerInvariant(),
            p => p);

    public virtual IFileParser GetParser(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() 
            ?? string.Empty;

        if (_parsers.TryGetValue(extension, out var parser))
            return parser;

        var supported = string.Join(", ", _parsers.Keys);
        throw new NotSupportedException(
            $"File format '{extension}' is not supported. Supported formats: {supported}");
    }
}
