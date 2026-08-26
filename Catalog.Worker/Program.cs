using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Handlers;
using Catalog.Worker.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

#region MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var user = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME") ?? "root";
    var pass = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD") ?? "r00tp@ss";
    var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "127.0.0.1:27017";

    var connectionString = $"mongodb://{user}:{pass}@{host}/?authSource=admin";

    var settings = MongoClientSettings.FromConnectionString(connectionString);
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

#region Redis

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var conn = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(conn);
});

builder.Services.AddScoped<IDatabase>(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

#endregion

#region MassTransit (AWS SQS / LocalStack)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host("us-east-1", h =>
        {
            h.AccessKey("test");
            h.SecretKey("test");

            var awsEndpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT") ?? "http://localhost:4566";

            h.Config(new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsEndpoint });
            h.Config(new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig { ServiceURL = awsEndpoint });
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