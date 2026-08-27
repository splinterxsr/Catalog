using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Extensions;
using Catalog.Api.Infrastructure.Cache;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddJwtSecurity();
builder.Services.AddPolicies();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<Mapper>();

#region MongoDb

builder.Services.AddMongoDb();

#endregion

#region Redis

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var conn = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect($"{conn},abortConnect=false");
});

#endregion

#region MassTransit (AWS SQS / LocalStack)
builder.Services.AddMassTransit(x =>
{
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

        cfg.ConfigureEndpoints(context);
    });
});
#endregion

#region Dependency Injection

builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IGameService, GameService>();

builder.Services.AddTransient<GameRepository>();
builder.Services.AddTransient<IGameRepository>(sp =>
    new CachedGameRepository(sp.GetRequiredService<GameRepository>(), sp.GetRequiredService<IRedisCacheService>()));

builder.Services.AddTransient<CatalogRepository>();
builder.Services.AddTransient<ICatalogRepository>(sp =>
    new CachedCatalogRepository(sp.GetRequiredService<CatalogRepository>(), sp.GetRequiredService<IRedisCacheService>()));

builder.Services.AddTransient<IOrderRepository, OrderRepository>();

#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

#region Health Checks

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }
});

#endregion

app.UseForwardedHeaders();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();