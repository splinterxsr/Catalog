namespace Fcg.Contracts
{
    public record PaymentProcessedEvent(Guid TransactionId, string OrderId, int UserId, string GameId, PaymentStatus Status);

    public enum PaymentStatus
    {
        Approved,
        Rejected
    }
}