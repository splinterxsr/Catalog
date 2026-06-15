namespace Catalog.Worker.Domain.Contracts
{
    public record PaymentProcessed
    {
        public Guid TransactionId { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public PaymentStatus Status { get; set; }
    }

    public enum PaymentStatus
    {
        Approved,
        Rejected
    }
}