using System.Globalization;
using BusinessAnalytics.API.Models.DTOs;
using CsvHelper;
using CsvHelper.Configuration;

namespace BusinessAnalytics.API.Services.Import.Parsing;

public class CsvFileParser : IFileParser
{
    public string SupportedExtension => ".csv";

    public async Task<List<OrderImportRow>> ParseAsync(Stream stream)
    {
        var rows = new List<OrderImportRow>();
        
        using var reader = new StreamReader(stream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            MissingFieldFound = null, // We'll handle missing fields in validation stages
            HeaderValidated = null    // We validate headers in our own ValidationStage
        };

        using var csv = new CsvReader(reader, config);
        
        int rowNumber = 1; // Header is row 1
        await foreach (var record in csv.GetRecordsAsync<OrderImportRow>())
        {
            rowNumber++;
            record.RowNumber = rowNumber;
            rows.Add(record);
        }
        
        return rows;
    }
}
