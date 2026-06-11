using OrderProcessing.Api.Domain;

namespace OrderProcessing.Api.States;

public class PendingState : IOrderState
{
    public string Name => "Pending";

    public void Pay(Order order)
    {
        order.CurrentState = new ConfirmedState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
    public void Process(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Process' în starea '{Name}'.");
    public void Ship(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Ship' în starea '{Name}'.");
    public void Deliver(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Deliver' în starea '{Name}'.");
    public void Cancel(Order order)
    {
        order.CurrentState = new CancelledState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
}

public class ConfirmedState : IOrderState
{
    public string Name => "Confirmed";

    public void Pay(Order order) => throw new InvalidOrderTransitionException($"Comanda este deja plătită.");
    public void Process(Order order)
    {
        order.CurrentState = new ProcessingState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
    public void Ship(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Ship' în starea '{Name}'.");
    public void Deliver(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Deliver' în starea '{Name}'.");
    public void Cancel(Order order)
    {
        order.CurrentState = new CancelledState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
}

public class ProcessingState : IOrderState
{
    public string Name => "Processing";

    public void Pay(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Pay' în starea '{Name}'.");
    public void Process(Order order) => throw new InvalidOrderTransitionException($"Este deja în procesare.");
    public void Ship(Order order)
    {
        order.CurrentState = new ShippedState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
    public void Deliver(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Deliver' în starea '{Name}'.");
    public void Cancel(Order order)
    {
        order.CurrentState = new CancelledState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
}

public class ShippedState : IOrderState
{
    public string Name => "Shipped";

    public void Pay(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Pay' în starea '{Name}'.");
    public void Process(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Process' în starea '{Name}'.");
    public void Ship(Order order) => throw new InvalidOrderTransitionException($"Este deja expediată.");
    public void Deliver(Order order)
    {
        order.CurrentState = new DeliveredState();
        order.AddHistory(Name, order.CurrentState.Name);
    }
    public void Cancel(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Cancel' în starea '{Name}'. Comanda a plecat deja!");
}

public class DeliveredState : IOrderState
{
    public string Name => "Delivered";

    public void Pay(Order order) => throw new InvalidOrderTransitionException($"Stare terminală. Nu sunt permise tranziții.");
    public void Process(Order order) => throw new InvalidOrderTransitionException($"Stare terminală. Nu sunt permise tranziții.");
    public void Ship(Order order) => throw new InvalidOrderTransitionException($"Stare terminală. Nu sunt permise tranziții.");
    public void Deliver(Order order) => throw new InvalidOrderTransitionException($"Stare terminală. Nu sunt permise tranziții.");
    public void Cancel(Order order) => throw new InvalidOrderTransitionException($"Nu pot executa 'Cancel' în starea '{Name}'.");
}

public class CancelledState : IOrderState
{
    public string Name => "Cancelled";

    public void Pay(Order order) => throw new InvalidOrderTransitionException($"Comanda este anulată.");
    public void Process(Order order) => throw new InvalidOrderTransitionException($"Comanda este anulată.");
    public void Ship(Order order) => throw new InvalidOrderTransitionException($"Comanda este anulată.");
    public void Deliver(Order order) => throw new InvalidOrderTransitionException($"Comanda este anulată.");
    public void Cancel(Order order) => throw new InvalidOrderTransitionException($"Comanda este deja anulată.");
}