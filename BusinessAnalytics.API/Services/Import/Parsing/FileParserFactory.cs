namespace BusinessAnalytics.API.Services.Import.Parsing;

/// <summary>
/// Factory that selects the appropriate IFileParser based on file extension.
/// New parsers are auto-discovered via DI (IEnumerable&lt;IFileParser&gt;).
/// </summary>
public class FileParserFactory
{
    private readonly Dictionary<string, IFileParser> _parsers;

    public FileParserFactory(IEnumerable<IFileParser> parsers)
    {
        _parsers = parsers.ToDictionary(
            p => p.SupportedExtension.ToLowerInvariant(),
            p => p);
    }

    public IFileParser GetParser(string fileName)
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
