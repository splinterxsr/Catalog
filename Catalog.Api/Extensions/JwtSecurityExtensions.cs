using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Catalog.Api.Extensions
{
    public static class JwtSecurityExtensions
    {
        private static readonly string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "M72gsn/ZtfvDSgyBgWNBOYf8HAHBO7FGIwiKPtwipmdMY6ZbdgrJ66yBLnXKk9O8P6fjj7G+BoE5+Ou/xfEgNQ==";
        private static readonly string[] jwtIssuer = [Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "FiapCloudGames"];

        public static IServiceCollection AddJwtSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                var bytes = Encoding.UTF8.GetBytes(jwtKey);
                var symmetricSecurityKey = new SymmetricSecurityKey(bytes);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidIssuers = jwtIssuer,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context => { return Task.CompletedTask; },
                    OnAuthenticationFailed = context => { return Task.CompletedTask; },
                    OnChallenge = context => { return Task.CompletedTask; }
                };
            });

            return services;
        }
    }
}