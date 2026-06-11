using MassTransit;
using Microsoft.Extensions.Logging;
using Catalog.Worker.Contracts;

namespace Catalog.Worker.Handlers
{
    public class PaymentConsumer : IConsumer<PaymentProcessed>
    {
        private readonly ILogger<PaymentConsumer> _logger;

        public PaymentConsumer(ILogger<PaymentConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessed> context)
        {
            var order = context.Message;

            _logger.LogInformation("💳 Payment Processed:");
            _logger.LogInformation("   Transaction ID: {TransactionId}", order.TransactionId);
            _logger.LogInformation("   User: {UserId}", order.UserId);
            _logger.LogInformation("   Game: {GameId}", order.GameId);
            _logger.LogInformation("   Status: {Status}", order.Status);

            if (order.Status == PaymentStatus.Approved)
            {
                _logger.LogInformation("✅ Payment approved succesfully!");
                _logger.LogInformation("---");
            }
            else
            {
                _logger.LogInformation("❌ Payment rejected!");
                _logger.LogInformation("---");
            }
        }
    }
}