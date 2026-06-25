using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Extensions;
using Catalog.Api.Infrastructure.Context;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Profiles;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddPolicies();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<Mapper>();

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

builder.Services.AddHealthChecks()
    .AddCheck("Self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddNpgSql(connectionString, name: "postgres-sql", tags: new[] { "ready" });

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
builder.Services.AddTransient<IUserCatalogRepository, UserCatalogRepository>();

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

app.ApplyMigrations();

app.Run();