using Catalog.Worker.Handlers;
using MassTransit;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payments-queue", e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(context);
        });
    });
});

var host = builder.Build();

Console.WriteLine("=== CONSUMER RABBITMQ COM MASSTRANSIT ===");
Console.WriteLine("Aguardando mensagens... Pressione Ctrl+C para parar.");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}