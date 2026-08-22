using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Catalog.Api.Extensions
{
    public static class MongoDbExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services)
        {
            services.AddSingleton<IMongoClient>(sp =>
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

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var database = Environment.GetEnvironmentVariable("MONGODB_DB") ?? "fcg";

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