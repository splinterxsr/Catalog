using System.Net;
using System.Net.Http.Json;
using Catalog.Api.Infrastructure.Clients;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace Catalog.Tests.Api.Clients
{
    public class UserClientServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly UserClientService _userClientService;

        public UserClientServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8080")
            };

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            _userClientService = new UserClientService(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WhenValid_ReturnsUser()
        {
            // Arrange
            var email = "admin@fiapcloud.com.br";
            var authResponse = new { Token = "fake-jwt-token" };
            var userResponse = new UserClientService.UsuarioResponse(1, email);

            // Mock da chamada POST para autenticação (/user/auth)
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath == "/user/auth"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(authResponse)
                });

            // Mock da chamada GET para buscar o utilizador (/user/{email})
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.AbsolutePath == $"/user/{email}"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(userResponse)
                });

            // Act
            var result = await _userClientService.GetUserByEmailAsync(email, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Email.Should().Be(email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_WhenAuthFails_ThrowsException()
        {
            // Arrange
            var email = "admin@fiapcloud.com.br";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri!.AbsolutePath == "/user/auth"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            // Act
            Func<Task> act = async () => await _userClientService.GetUserByEmailAsync(email, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }
    }
}