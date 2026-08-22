using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Extensions;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
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

#region Masstransit (RabbitMQ)

builder.Services.AddMassTransit(x =>
{
    var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    var user = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER") ?? "guest";
    var password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS") ?? "guest";

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

#endregion

#region Dependency Injection

builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IGameService, GameService>();

builder.Services.AddTransient<IGameRepository, GameRepository>();
builder.Services.AddTransient<ICatalogRepository, CatalogRepository>();
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