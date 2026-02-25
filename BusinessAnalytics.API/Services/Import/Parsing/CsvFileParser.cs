using BusinessAnalytics.API.Models.DTOs;

namespace BusinessAnalytics.API.Services.Import.Parsing;

/// <summary>
/// CSV file parser strategy.
/// Parses CSV files with header row into OrderImportRow DTOs.
/// </summary>
public class CsvFileParser : IFileParser
{
    public string SupportedExtension => ".csv";

    private static readonly string[] ExpectedHeaders =
    {
        "OrderDate", "CustomerName", "CustomerEmail", "ProductName",
        "CategoryName", "Quantity", "UnitPrice", "Status"
    };

    public async Task<List<OrderImportRow>> ParseAsync(Stream stream)
    {
        var rows = new List<OrderImportRow>();
        
        using var reader = new StreamReader(stream);
        
        // Read and validate header
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
            return rows;

        var headers = ParseCsvLine(headerLine);
        var columnIndex = BuildColumnIndex(headers);
        
        int rowNumber = 1;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            rowNumber++;
            
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);
            
            var row = new OrderImportRow
            {
                RowNumber = rowNumber,
                OrderDate = GetValue(values, columnIndex, "OrderDate"),
                CustomerName = GetValue(values, columnIndex, "CustomerName"),
                CustomerEmail = GetValue(values, columnIndex, "CustomerEmail"),
                ProductName = GetValue(values, columnIndex, "ProductName"),
                CategoryName = GetValue(values, columnIndex, "CategoryName"),
                Quantity = GetValue(values, columnIndex, "Quantity"),
                UnitPrice = GetValue(values, columnIndex, "UnitPrice"),
                Status = GetValue(values, columnIndex, "Status")
            };
            
            rows.Add(row);
        }
        
        return rows;
    }

    /// <summary>
    /// Build a mapping of column name -> index from the header row.
    /// </summary>
    private Dictionary<string, int> BuildColumnIndex(string[] headers)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Length; i++)
        {
            index[headers[i].Trim()] = i;
        }
        return index;
    }

    private string GetValue(string[] values, Dictionary<string, int> columnIndex, string column)
    {
        if (columnIndex.TryGetValue(column, out var idx) && idx < values.Length)
            return values[idx].Trim();
        return string.Empty;
    }

    /// <summary>
    /// Parse a CSV line, handling quoted fields with commas inside.
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // skip escaped quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }
        
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
