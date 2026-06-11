namespace OrderProcessing.Api.States;

public class InvalidOrderTransitionException : Exception
{
    public InvalidOrderTransitionException(string message) : base(message) { }
}