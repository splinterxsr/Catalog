using static Catalog.Api.Infrastructure.Clients.UserClientService;

namespace Catalog.Api.Infrastructure.Clients
{
    public interface IUserClientService
    {
        Task<UsuarioResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}