using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Context;
using Catalog.Worker.Infrastructure.Handlers;
using Catalog.Worker.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

#region DB Postgres

var postgreUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "catalog_user";
var postgrePassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "catalog_pass";
var postgreDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "catalog_db";
var postgreHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";

var connectionString = $"Host={postgreHost};Port=5432;Database={postgreDb};Username={postgreUser};Password={postgrePassword}";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
}, ServiceLifetime.Scoped);

#endregion

#region MassTransit (RabbitMQ)

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var user = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest";
        var password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(password);
        });

        var paymentQueue = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_NAME") ?? "payments-queue";

        cfg.ReceiveEndpoint(paymentQueue, e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(context);
        });
    });
});

#endregion

#region DI

builder.Services.AddTransient<IUserCatalogRepository, UserCatalogRepository>();

#endregion

var host = builder.Build();

Console.WriteLine("Waiting messages... Press Ctrl+C to stop.");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}