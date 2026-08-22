using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Handlers;
using Catalog.Worker.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

var builder = Host.CreateApplicationBuilder(args);

#region MongoDB

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var user = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME") ?? "root";
    var pass = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD") ?? "r00tp@ss";
    var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "127.0.0.1:27017";

    var credential = MongoCredential.CreateCredential(
        databaseName: "admin",
        username: user,
        password: pass
    );

    var settings = MongoClientSettings.FromConnectionString($"mongodb://{host}");
    settings.Credential = credential;
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(90);

    return new MongoClient(settings);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var database = Environment.GetEnvironmentVariable("MONGODB_DB") ?? "fcg";

    var client = sp.GetRequiredService<IMongoClient>();

    return client.GetDatabase(database);
});

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

        var paymentQueue = Environment.GetEnvironmentVariable("PAYMENT_QUEUE_NAME") ?? "payments-1-queue";

        cfg.ReceiveEndpoint(paymentQueue, e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(context);
        });
    });
});

#endregion

#region DI

builder.Services.AddTransient<IUserCatalogRepository, UserCatalogRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();

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