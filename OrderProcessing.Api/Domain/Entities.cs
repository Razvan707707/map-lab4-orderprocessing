namespace OrderProcessing.Api.Domain;

public record Customer(Guid Id, string Name, string Email, int Age, bool IsTrusted);
public record OrderItem(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, bool HasAgeRestriction);