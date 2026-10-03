using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Catalog.Api.Extensions
{
    public static class MongoDbExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration["DocumentDbConnectionString"] ?? string.Empty;
                    
                var settings = MongoClientSettings.FromConnectionString(connectionString);

                return new MongoClient(settings);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var database = Environment.GetEnvironmentVariable("MONGODB_DB") ?? string.Empty;

                var client = sp.GetRequiredService<IMongoClient>();

                return client.GetDatabase(database);
            });

            services.AddHealthChecks()
                .AddCheck("Self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
                .AddMongoDb(name: "mongodb", tags: new[] { "ready" }, timeout: TimeSpan.FromSeconds(90));

            return services;
        }
    }
}