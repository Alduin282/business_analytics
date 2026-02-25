using BusinessAnalytics.API.Services.Import.Validation;

namespace BusinessAnalytics.API.Services.Import.Pipeline.Stages;

/// <summary>
/// Stage 2: Run the Chain of Responsibility validators.
/// Aborts pipeline if any validation errors are found.
/// </summary>
public class ValidationStage : IImportPipelineStage
{
    private readonly HeaderValidator _headerValidator;
    private readonly DataTypeValidator _dataTypeValidator;
    private readonly BusinessRuleValidator _businessRuleValidator;

    public ValidationStage(
        HeaderValidator headerValidator,
        DataTypeValidator dataTypeValidator,
        BusinessRuleValidator businessRuleValidator)
    {
        _headerValidator = headerValidator;
        _dataTypeValidator = dataTypeValidator;
        _businessRuleValidator = businessRuleValidator;
    }

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
