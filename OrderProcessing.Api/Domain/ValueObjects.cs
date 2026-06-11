namespace OrderProcessing.Api.Domain;

public record OrderId(Guid Value);
public record Money(decimal Amount, string Currency = "RON");
public record Address(string Street, string City, string ZipCode, string Country);