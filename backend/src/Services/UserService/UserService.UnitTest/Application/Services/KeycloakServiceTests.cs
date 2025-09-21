using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services;
using UserService.Domain.Exceptions;
using Xunit;

namespace UserService.UnitTest.Application.Services;

public class KeycloakServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IOptions<KeycloakSettings>> _optionsMock;
    private readonly Mock<ILogger<KeycloakService>> _loggerMock;
    private readonly HttpClient _httpClient;
    private readonly KeycloakService _keycloakService;
    private readonly Faker _faker;

    public KeycloakServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _optionsMock = new Mock<IOptions<KeycloakSettings>>();
        _loggerMock = new Mock<ILogger<KeycloakService>>();
        
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        // Setup KeycloakSettings
        var keycloakSettings = new KeycloakSettings
        {
            Url = "http://localhost:8080",
            Realm = "test-realm",
            BackendClientId = "test-client",
            BackendClientSecret = "test-secret",
            AdminUsername = "admin",
            AdminPassword = "admin-password"
        };
        _optionsMock.Setup(x => x.Value).Returns(keycloakSettings);
        
        _keycloakService = new KeycloakService(_httpClient, _optionsMock.Object, _loggerMock.Object);
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public async Task CreateUserAsync_ComDadosValidos_DeveRetornarIdDoUsuario()
    {
        // Arrange
        var username = _faker.Internet.UserName();
        var email = _faker.Internet.Email();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var password = "MinhaSenh@123";
        var expectedUserId = Guid.NewGuid().ToString();

        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };
        var userResponse = new { id = expectedUserId };

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup create user request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/admin/realms/test-realm/users") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Headers = { Location = new Uri($"http://localhost:8080/auth/admin/realms/test-realm/users/{expectedUserId}") }
            });

        // Act
        var createUserRequest = new CreateUserKeycloak(
            Username: username,
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            Password: password,
            Enabled: true,
            EmailVerified: false,
            Roles: new List<string> { "user" }
        );
        var result = await _keycloakService.CreateUserAsync(createUserRequest);

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public async Task CreateUserAsync_QuandoUsuarioJaExiste_DeveLancarKeycloakException()
    {
        // Arrange
        var username = _faker.Internet.UserName();
        var email = _faker.Internet.Email();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var password = "MinhaSenh@123";

        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };
        var errorResponse = new { error = "User exists with same username" };

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup create user request with conflict
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/admin/realms/test-realm/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var createUserRequest = new CreateUserKeycloak(
            Username: username,
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            Password: password,
            Enabled: true,
            EmailVerified: false,
            Roles: new List<string> { "user" }
        );
        var exception = await Assert.ThrowsAsync<KeycloakException>(
            () => _keycloakService.CreateUserAsync(createUserRequest));

        exception.Message.Should().Contain("Erro ao criar usuário no Keycloak");
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = "MinhaSenh@123";
        
        var tokenResponse = new LoginResponse(
            AccessToken: "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
            ExpiresIn: 3600,
            RefreshExpiresIn: 7200,
            RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            TokenType: "Bearer",
            Scope: "openid profile email"
        );

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token") &&
                    req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
            });

        // Act
        var loginRequest = new LoginUserKeycloak(email, password);
        var result = await _keycloakService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(tokenResponse.AccessToken);
        result.ExpiresIn.Should().Be(tokenResponse.ExpiresIn);
        result.RefreshExpiresIn.Should().Be(tokenResponse.RefreshExpiresIn);
        result.RefreshToken.Should().Be(tokenResponse.RefreshToken);
        result.TokenType.Should().Be(tokenResponse.TokenType);
        result.Scope.Should().Be(tokenResponse.Scope);
    }

    [Fact]
    public async Task LoginAsync_ComCredenciaisInvalidas_DeveLancarKeycloakException()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = "SenhaIncorreta";
        
        var errorResponse = new { error = "invalid_grant", error_description = "Invalid user credentials" };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var loginRequest = new LoginUserKeycloak(email, password);
        var exception = await Assert.ThrowsAsync<KeycloakException>(
            () => _keycloakService.LoginAsync(loginRequest));

        exception.Message.Should().Contain("Erro ao fazer login no Keycloak");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ComEmailExistente_DeveRetornarUsuario()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var expectedUserId = Guid.NewGuid().ToString();
        
        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };
        var usersResponse = new[]
        {
            new UserResponseKeycloak(
                Id: expectedUserId,
                Username: email,
                Email: email,
                FirstName: _faker.Name.FirstName(),
                LastName: _faker.Name.LastName(),
                Enabled: true,
                EmailVerified: true,
                CreatedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Roles: new List<string> { "user" },
                Attributes: new Dictionary<string, List<string>>()
            )
        };

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup get user request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/admin/realms/test-realm/users") &&
                    req.RequestUri.ToString().Contains($"email={email}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(usersResponse), Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _keycloakService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedUserId);
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ComEmailInexistente_DeveRetornarNull()
    {
        // Arrange
        var email = "usuario.inexistente@email.com";
        
        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };
        var usersResponse = Array.Empty<UserResponseKeycloak>();

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup get user request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/admin/realms/test-realm/users")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(usersResponse), Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _keycloakService.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteUserAsync_ComIdValido_DeveExcluirUsuario()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        
        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup delete user request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"/auth/admin/realms/test-realm/users/{userId}") &&
                    req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        await _keycloakService.DeleteUserAsync(userId);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2), // Admin token + Delete user
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserAsync_ComIdInvalido_DeveLancarKeycloakException()
    {
        // Arrange
        var userId = "id-inexistente";
        
        var adminTokenResponse = new { access_token = "admin-token", expires_in = 3600 };

        // Setup admin token request
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains("/auth/realms/test-realm/protocol/openid-connect/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
            });

        // Setup delete user request with not found
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"/auth/admin/realms/test-realm/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeycloakException>(
            () => _keycloakService.DeleteUserAsync(userId));

        exception.Message.Should().Contain("Erro ao excluir usuário no Keycloak");
    }
}