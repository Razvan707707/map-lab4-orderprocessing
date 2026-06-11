using OrderProcessing.Api.Domain;

namespace OrderProcessing.Api.Validation;

public abstract class BaseValidationHandler : IOrderValidationHandler
{
    private IOrderValidationHandler? _nextHandler;

    public IOrderValidationHandler SetNext(IOrderValidationHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public ValidationResult Handle(Order order)
    {
        // 1. Rulează logica din clasa curentă
        var result = Validate(order);

        // 2. Short-circuit: dacă a picat aici, ne oprim și dăm eroarea înapoi!
        if (!result.IsValid)
        {
            return result;
        }

        // 3. Dacă e totul OK, o trimitem la următorul filtru (dacă există)
        if (_nextHandler != null)
        {
            return _nextHandler.Handle(order);
        }

        return ValidationResult.Success();
    }

    // Subclasele vor implementa doar asta
    protected abstract ValidationResult Validate(Order order);
}