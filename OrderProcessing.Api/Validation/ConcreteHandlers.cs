using OrderProcessing.Api.Domain;

namespace OrderProcessing.Api.Validation;

public class StockValidationHandler : BaseValidationHandler
{
    // Baza de date in-memory cerută pentru stocuri
    private readonly Dictionary<Guid, int> _stockDatabase = new()
    {
        { Guid.Parse("11111111-1111-1111-1111-111111111111"), 50 },  // Laptop
        { Guid.Parse("22222222-2222-2222-2222-222222222222"), 100 }, // Bere (18+)
        { Guid.Parse("33333333-3333-3333-3333-333333333333"), 0 }    // Consolă Epuizată
    };

    protected override ValidationResult Validate(Order order)
    {
        foreach (var item in order.Items)
        {
            // Verificăm dacă produsul există în sistem și dacă avem suficient
            if (!_stockDatabase.ContainsKey(item.ProductId))
                return ValidationResult.Failed($"Produsul '{item.ProductName}' nu există în baza de date.");

            if (_stockDatabase[item.ProductId] < item.Quantity)
                return ValidationResult.Failed($"Stoc insuficient pentru '{item.ProductName}'.");
        }
        return ValidationResult.Success();
    }
}

public class PriceValidationHandler : BaseValidationHandler
{
    protected override ValidationResult Validate(Order order)
    {
        foreach (var item in order.Items)
        {
            if (item.UnitPrice <= 0)
                return ValidationResult.Failed($"Prețul pentru '{item.ProductName}' trebuie să fie strict pozitiv.");
        }

        // Verificăm dacă totalul e calculat corect (suma prețurilor * cantitate)
        var calculatedTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        if (order.Total != calculatedTotal)
            return ValidationResult.Failed("Eroare de sistem: Totalul calculat nu coincide cu suma produselor.");

        return ValidationResult.Success();
    }
}

public class FraudDetectionHandler : BaseValidationHandler
{
    protected override ValidationResult Validate(Order order)
    {
        // Regula 1: refuză comenzile peste 10.000 RON dacă omul nu e de încredere
        if (!order.Customer.IsTrusted && order.Total > 10000)
            return ValidationResult.Failed("Tentativă de fraudă: Comanda depășește limita de 10.000 RON pentru clienți neverificați.");

        // Regula 2: Nu mai mult de 50 de chestii distincte
        var distinctItemsCount = order.Items.Select(i => i.ProductId).Distinct().Count();
        if (distinctItemsCount > 50)
            return ValidationResult.Failed("O comandă nu poate conține mai mult de 50 de produse distincte.");

        return ValidationResult.Success();
    }
}

public class AgeVerificationHandler : BaseValidationHandler
{
    protected override ValidationResult Validate(Order order)
    {
        var hasRestrictedItems = order.Items.Any(i => i.HasAgeRestriction);
        if (hasRestrictedItems && order.Customer.Age < 18)
        {
            return ValidationResult.Failed("Restricție: Clientul trebuie să aibă minim 18 ani pentru a comanda acest produs.");
        }

        return ValidationResult.Success();
    }
}