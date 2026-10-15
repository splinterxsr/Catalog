namespace Catalog.Api.Infrastructure.Clients;

public class UserClientService : IUserClientService
{
    private readonly HttpClient _httpClient;

    public UserClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://localhost:8080");
    }

    public async Task<UsuarioResponse?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var token = await ObterTokenJwtAsync(cancellationToken);

        using var requisicao = new HttpRequestMessage(
            HttpMethod.Get,
            $"/user/{email}");

        requisicao.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UsuarioResponse>(
            cancellationToken)
            ?? throw new InvalidOperationException(
                "O endpoint não retornou os dados do usuário.");
    }

    private async Task<string?> ObterTokenJwtAsync(CancellationToken cancellationToken = default)
    {
        var requisicao = new
        {
            email = "admin@fiapcloud.com.br",
            password = "admin"
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/user/auth",
            requisicao,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<TokenResponse>(
            cancellationToken);

        return resultado?.Token
            ?? throw new InvalidOperationException("O endpoint não retornou um token JWT.");
    }

    private sealed record TokenResponse(string Token);

    public sealed record UsuarioResponse(int Id, string Email);
}