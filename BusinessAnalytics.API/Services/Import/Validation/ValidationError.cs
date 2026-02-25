namespace BusinessAnalytics.API.Services.Import.Validation;

public class ValidationError
{
    public int RowNumber { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    public ValidationError() { }
    
    public ValidationError(int rowNumber, string column, string message)
    {
        RowNumber = rowNumber;
        Column = column;
        Message = message;
    }
}
