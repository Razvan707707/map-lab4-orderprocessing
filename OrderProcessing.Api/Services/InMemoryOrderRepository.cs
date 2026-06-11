using System.Collections.Concurrent;
using OrderProcessing.Api.Domain;

namespace OrderProcessing.Api.Services;

public interface IOrderRepository
{
    void Save(Order order);
    Order? GetById(OrderId id);
    IEnumerable<Order> GetAll();
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public void Save(Order order)
    {
        _orders[order.Id.Value] = order;
    }

    public Order? GetById(OrderId id)
    {
        _orders.TryGetValue(id.Value, out var order);
        return order;
    }

    public IEnumerable<Order> GetAll()
    {
        return _orders.Values.OrderByDescending(o => o.Id.Value);
    }
}