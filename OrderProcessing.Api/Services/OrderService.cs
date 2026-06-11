using OrderProcessing.Api.Domain;
using OrderProcessing.Api.Validation;

namespace OrderProcessing.Api.Services;

// DTO-uri pentru cererea de la Frontend
public record CreateOrderRequest(CustomerDto Customer, AddressDto Address, List<OrderItemDto> Items);
public record CustomerDto(string Name, string Email, int Age, bool IsTrusted);
public record AddressDto(string Street, string City, string ZipCode, string Country);
public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, bool HasAgeRestriction);

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IOrderValidationHandler _validationChain;

    public OrderService(IOrderRepository repository, IOrderValidationHandler validationChain)
    {
        _repository = repository;
        _validationChain = validationChain;
    }

    public Tuple<Order?, ValidationResult> CreateOrder(CreateOrderRequest request)
    {
        // Convertim DTO-ul în obiecte de domeniu
        var customer = new Customer(Guid.NewGuid(), request.Customer.Name, request.Customer.Email, request.Customer.Age, request.Customer.IsTrusted);
        var address = new Address(request.Address.Street, request.Address.City, request.Address.ZipCode, request.Address.Country);
        var items = request.Items.Select(i => new OrderItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.HasAgeRestriction)).ToList();

        var order = new Order(new OrderId(Guid.NewGuid()), customer, address, items);

        // Rulăm Chain of Responsibility
        var validationResult = _validationChain.Handle(order);
        if (!validationResult.IsValid)
        {
            return Tuple.Create<Order?, ValidationResult>(null, validationResult);
        }

        _repository.Save(order);
        return Tuple.Create<Order?, ValidationResult>(order, validationResult);
    }

    public Order? PayOrder(Guid id) => ExecuteTransition(id, o => o.Pay());
    public Order? ProcessOrder(Guid id) => ExecuteTransition(id, o => o.Process());
    public Order? ShipOrder(Guid id) => ExecuteTransition(id, o => o.Ship());
    public Order? DeliverOrder(Guid id) => ExecuteTransition(id, o => o.Deliver());
    public Order? CancelOrder(Guid id) => ExecuteTransition(id, o => o.Cancel());

    public Order? GetOrder(Guid id) => _repository.GetById(new OrderId(id));
    public IEnumerable<Order> GetAllOrders() => _repository.GetAll();

    private Order? ExecuteTransition(Guid id, Action<Order> transition)
    {
        var order = _repository.GetById(new OrderId(id));
        if (order == null) return null;

        transition(order); // Lansează InvalidOrderTransitionException dacă e ilegală
        _repository.Save(order);
        return order;
    }
}