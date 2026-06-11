using OrderProcessing.Api.Domain;

namespace OrderProcessing.Api.Validation;

public interface IOrderValidationHandler
{
    IOrderValidationHandler SetNext(IOrderValidationHandler handler);
    ValidationResult Handle(Order order);
}