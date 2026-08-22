namespace Fcg.Contracts
{
    public record PaymentProcessedEvent
    {
        public Guid TransactionId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string GameId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
    }

    public enum PaymentStatus
    {
        Approved,
        Rejected
    }
}