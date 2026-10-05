using Azure.Identity;
using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Handlers;
using Catalog.Worker.Infrastructure.Repositories;
using Fcg.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

var vaultUriStr = Environment.GetEnvironmentVariable("KeyVaultUri") ?? string.Empty;
var vaultUri = new Uri(vaultUriStr);

builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());

#region MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["DocumentDbConnectionString"] ?? string.Empty;

    var settings = MongoClientSettings.FromConnectionString(connectionString);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(90);

    return new MongoClient(settings);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var database = Environment.GetEnvironmentVariable("MONGODB_DB") ?? throw new InvalidOperationException("Variável 'MONGODB_DB' não encontrada.");

    var client = sp.GetRequiredService<IMongoClient>();

    return client.GetDatabase(database);
});
#endregion

#region Redis

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var conn = Environment.GetEnvironmentVariable("REDIS_HOST") ?? string.Empty;
    return ConnectionMultiplexer.Connect(conn);
});

builder.Services.AddScoped<IDatabase>(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

#endregion

#region MassTransit (Azure Service Bus)

builder.Services.AddMassTransit(x =>
{
    var topicName = Environment.GetEnvironmentVariable("PAYMENTS_TOPIC") ?? string.Empty;
    var subName = Environment.GetEnvironmentVariable("PAYMENTS_SUBSCRIPTION") ?? string.Empty;

    x.AddConsumer<PaymentConsumer>();

    x.UsingAzureServiceBus((context, cfg) =>
    {
        var connectionString = builder.Configuration["ServiceBusConnectionString"] ?? string.Empty;

        cfg.Host(connectionString);

        cfg.Message<PaymentProcessedEvent>(e => e.SetEntityName(topicName));

        cfg.SubscriptionEndpoint<PaymentProcessedEvent>(subName, e =>
        {
            e.ConfigureConsumer<PaymentConsumer>(context);
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
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