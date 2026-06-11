namespace OrderProcessing.Api.Validation;

public record ValidationResult(bool IsValid, List<string> Errors)
{
    public static ValidationResult Success() => new(true, new());
    public static ValidationResult Failed(string error) => new(false, new List<string> { error });
}