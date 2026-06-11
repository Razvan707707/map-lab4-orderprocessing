using OrderProcessing.Api.Domain;
using OrderProcessing.Api.Services;
using OrderProcessing.Api.States;

namespace OrderProcessing.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        // Helper intern pentru a returna exact structura curată cerută de SPA
        object FormatOrder(Order order) => new
        {
            id = order.Id.Value,
            status = order.CurrentState.Name,
            customer = new { order.Customer.Id, order.Customer.Name, order.Customer.Email, order.Customer.Age, order.Customer.IsTrusted },
            items = order.Items.Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.HasAgeRestriction }),
            address = order.DeliveryAddress,
            total = order.Total,
            history = order.History
        };

        // 1. POST /orders - Creare
        app.MapPost("/orders", (CreateOrderRequest request, OrderService service) =>
        {
            var (order, validation) = service.CreateOrder(request);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Errors); // HTTP 400
            }
            return Results.Created($"/orders/{order!.Id.Value}", FormatOrder(order)); // HTTP 201
        });

        // 2. GET /orders - Toate comenzile
        app.MapGet("/orders", (OrderService service) =>
            Results.Ok(service.GetAllOrders().Select(FormatOrder)));

        // 3. GET /orders/{id} - Detalii comandă
        app.MapGet("/orders/{id:guid}", (Guid id, OrderService service) =>
        {
            var order = service.GetOrder(id);
            return order != null ? Results.Ok(FormatOrder(order)) : Results.NotFound();
        });

        // Helper pentru tranziții de stare cu prinderea excepției de conflict (HTTP 409)
        IResult HandleTransition(Guid id, Func<Guid, Order?> transition)
        {
            try
            {
                var order = transition(id);
                return order != null ? Results.Ok(FormatOrder(order)) : Results.NotFound();
            }
            catch (InvalidOrderTransitionException ex)
            {
                return Results.Conflict(ex.Message); // HTTP 409 Conflict!
            }
        }

        app.MapPost("/orders/{id:guid}/pay", (Guid id, OrderService service) => HandleTransition(id, service.PayOrder));
        app.MapPost("/orders/{id:guid}/process", (Guid id, OrderService service) => HandleTransition(id, service.ProcessOrder));
        app.MapPost("/orders/{id:guid}/ship", (Guid id, OrderService service) => HandleTransition(id, service.ShipOrder));
        app.MapPost("/orders/{id:guid}/deliver", (Guid id, OrderService service) => HandleTransition(id, service.DeliverOrder));
        app.MapPost("/orders/{id:guid}/cancel", (Guid id, OrderService service) => HandleTransition(id, service.CancelOrder));
    }
}