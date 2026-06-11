using OrderProcessing.Api.States;

namespace OrderProcessing.Api.Domain;

public class Order
{
    public OrderId Id { get; }
    public Customer Customer { get; }
    public Address DeliveryAddress { get; }
    public List<OrderItem> Items { get; }

    // Starea curentă este expusă read-only spre exterior
    public IOrderState CurrentState { get; internal set; }
    public List<string> History { get; } = new();

    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);

    public Order(OrderId id, Customer customer, Address deliveryAddress, List<OrderItem> items)
    {
        Id = id;
        Customer = customer;
        DeliveryAddress = deliveryAddress;
        Items = items;
        CurrentState = new PendingState(); // Starea inițială
    }

    public void AddHistory(string fromState, string toState)
    {
        History.Add($"{fromState} -> {toState} {DateTime.Now:HH:mm:ss}");
    }

    // Delegăm toate operațiunile către starea curentă! (State Pattern)
    public void Pay() => CurrentState.Pay(this);
    public void Process() => CurrentState.Process(this);
    public void Ship() => CurrentState.Ship(this);
    public void Deliver() => CurrentState.Deliver(this);
    public void Cancel() => CurrentState.Cancel(this);
}