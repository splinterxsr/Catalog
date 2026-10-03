using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
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
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? string.Empty;
var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? string.Empty;

var credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
using var client = new AmazonSecretsManagerClient(RegionEndpoint.USEast1);

var request = new GetSecretValueRequest
{
    SecretId = "fcg-secrets"
};

var response = await client.GetSecretValueAsync(request);

if (!string.IsNullOrEmpty(response.SecretString))
{
    var secretData = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
    if (secretData != null)
    {
        builder.Configuration.AddInMemoryCollection(secretData!);
    }
}

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