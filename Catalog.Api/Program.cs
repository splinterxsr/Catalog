using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Infrastructure.Context;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Profiles;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

#region Dependency Injection

builder.Services.AddSingleton<Mapper>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = "Host=localhost;Port=5432;Database=catalog_db;Username=catalog_user;Password=catalog_pass";

    if (!string.IsNullOrWhiteSpace(conn))
    {
        options.UseNpgsql(conn);
    }
},ServiceLifetime.Scoped);

builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IGameService, GameService>();

builder.Services.AddTransient<IGameRepository, GameRepository>();
builder.Services.AddTransient<IUserCatalogRepository, UserCatalogRepository>();

#endregion

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();