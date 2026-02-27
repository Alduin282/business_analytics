using BusinessAnalytics.API.Services.Import.Validation;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 2: Run the Chain of Responsibility validators.
/// Aborts pipeline if any validation errors are found.
/// </summary>
public class ValidationStage(
    HeaderValidator headerValidator,
    DataTypeValidator dataTypeValidator,
    BusinessRuleValidator businessRuleValidator) : IImportPipelineStage
{
    private readonly HeaderValidator _headerValidator = headerValidator;
    private readonly DataTypeValidator _dataTypeValidator = dataTypeValidator;
    private readonly BusinessRuleValidator _businessRuleValidator = businessRuleValidator;

    public async Task<ImportContext> ExecuteAsync(ImportContext context)
    {
        // Build the chain: Header → DataType → BusinessRule
        _headerValidator.SetNext(_dataTypeValidator);
        _dataTypeValidator.SetNext(_businessRuleValidator);

        var errors = await _headerValidator.ValidateAsync(context.ParsedRows, context.Headers);

        if (errors.Count > 0)
        {
            context.Errors.AddRange(errors);
            context.IsAborted = true;
        }

        return context;
    }
}
