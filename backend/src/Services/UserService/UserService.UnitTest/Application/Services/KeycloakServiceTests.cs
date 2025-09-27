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
        
        // Setup a generic mock that handles all HTTP requests
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
            {
                var uri = request.RequestUri?.ToString() ?? "";
                Console.WriteLine($"Mock called with URI: {uri}");
                Console.WriteLine($"Method: {request.Method}");
                
                // Admin token request
                if (uri.Contains("/protocol/openid-connect/token") && request.Method == HttpMethod.Post && uri.Contains("client_id=admin-cli"))
                {
                    Console.WriteLine("Handling admin token request");
                    var adminTokenResponse = new LoginResponseKeycloak(
                        AccessToken: "admin-token-123",
                        ExpiresIn: 3600,
                        RefreshExpiresIn: 7200,
                        RefreshToken: "admin-refresh-token",
                        TokenType: "Bearer",
                        Scope: "openid profile email"
                    );
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(adminTokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                
                // User login request (non-admin)
                if (uri.Contains("/protocol/openid-connect/token") && request.Method == HttpMethod.Post)
                {
                    Console.WriteLine("Handling user login request");
                    // Check if it's invalid credentials by looking at the request content
                    var requestContent = request.Content?.ReadAsStringAsync().Result ?? "";
                    Console.WriteLine($"Login request content: {requestContent}");
                    
                    if (requestContent.Contains("password=SenhaIncorreta"))
                    {
                        Console.WriteLine("Returning 401 for invalid credentials");
                        var errorResponse = new { error = "invalid_grant", error_description = "Invalid user credentials" };
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
                        };
                    }
                    
                    Console.WriteLine("Returning successful login response");
                    var loginResponse = new LoginResponseKeycloak(
                        AccessToken: "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
                        ExpiresIn: 3600,
                        RefreshExpiresIn: 7200,
                        RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                        TokenType: "Bearer",
                        Scope: "openid profile email"
                    );
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(loginResponse), Encoding.UTF8, "application/json")
                    };
                }
                
                // Create user request
                if (uri.Contains("/admin/realms/test-realm/users") && request.Method == HttpMethod.Post)
                {
                    // Check if it's a conflict scenario by looking at the request content
                    var requestContent = request.Content?.ReadAsStringAsync().Result ?? "";
                    if (requestContent.Contains("\"email\":\"QuandoUsuarioJaExiste@email.com\""))
                    {
                        var errorResponse = new { errorMessage = "User exists with same username" };
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Conflict,
                            Content = new StringContent(JsonSerializer.Serialize(errorResponse), Encoding.UTF8, "application/json")
                        };
                    }
                    
                    var userId = Guid.NewGuid().ToString();
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Headers = { Location = new Uri($"http://localhost:8080/auth/admin/realms/test-realm/users/{userId}") }
                    };
                }
                
                // Get user by email request
                if (uri.Contains("/admin/realms/test-realm/users") && uri.Contains("email=") && request.Method == HttpMethod.Get)
                {
                    // Check if it's a non-existent email scenario
                    if (uri.Contains("EmailInexistente") || uri.Contains("nonexistent") || 
                        uri.Contains("usuario.inexistente%40email.com") || uri.Contains("usuario.inexistente@email.com"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("[]", Encoding.UTF8, "application/json")
                        };
                    }
                    
                    var usersResponse = new[]
                    {
                        new UserResponseKeycloak(
                            Id: Guid.NewGuid().ToString(),
                            Username: "test@email.com",
                            Email: "test@email.com",
                            FirstName: "Test",
                            LastName: "User",
                            Enabled: true,
                            EmailVerified: true,
                            CreatedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Roles: new List<string> { "user" },
                            Attributes: new Dictionary<string, List<string>>()
                        )
                    };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(usersResponse), Encoding.UTF8, "application/json")
                    };
                }
                
                // Delete user request
                if (uri.Contains("/admin/realms/test-realm/users/") && request.Method == HttpMethod.Delete)
                {
                    // Check if it's an invalid user ID scenario
                    if (uri.Contains("IdInvalido") || uri.Contains("invalid-id") || uri.Contains("00000000-0000-0000-0000-000000000000"))
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent("{\"error\": \"User not found\"}", Encoding.UTF8, "application/json")
                        };
                    }
                    
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NoContent
                    };
                }
                
                // Default response for other requests
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            });
        
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        // Setup KeycloakSettings
        var keycloakSettings = new KeycloakSettings
        {
            Url = "http://localhost:8080",
            Realm = "test-realm",
            BackendClientId = "test-client",
            BackendClientSecret = "test-secret",
            FrontendClientId = "frontend-client",
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
        var email = _faker.Internet.Email();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var password = _faker.Internet.Password();
        
        var createUserRequest = new CreateUserKeycloak(
            Username: email,
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            Password: password,
            Enabled: true,
            EmailVerified: false,
            Roles: new List<string> { "user" }
        );

        // Mock will be handled by the generic setup in constructor

        // Act
        var result = await _keycloakService.CreateUserAsync(createUserRequest);

        // Assert
        result.Should().NotBeNullOrEmpty();
        Guid.TryParse(result, out _).Should().BeTrue("O resultado deve ser um GUID válido");
    }

    [Fact]
    public async Task CreateUserAsync_QuandoUsuarioJaExiste_DeveLancarKeycloakException()
    {
        // Arrange
        var username = "QuandoUsuarioJaExiste";
        var email = "QuandoUsuarioJaExiste@email.com";
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var password = "MinhaSenh@123";

        // Mock will be handled by the generic setup in constructor

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
        
        // Mock will be handled by the generic setup in constructor
        var tokenResponse = new LoginResponseKeycloak(
            AccessToken: "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
            ExpiresIn: 3600,
            RefreshExpiresIn: 7200,
            RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            TokenType: "Bearer",
            Scope: "openid profile email"
        );

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
        
        // Mock will be handled by the generic setup in constructor

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
        var email = "test@email.com";
        
        // Mock will be handled by the generic setup in constructor

        // Act
        var result = await _keycloakService.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Username.Should().Be(email);
        result.FirstName.Should().Be("Test");
        result.LastName.Should().Be("User");
    }

    [Fact]
    public async Task GetUserByEmailAsync_ComEmailInexistente_DeveRetornarNull()
    {
        // Arrange
        var email = "usuario.inexistente@email.com";
        
        // Mock will be handled by the generic setup in constructor

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
        
        // Mock will be handled by the generic setup in constructor

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
        var userId = "IdInvalido";
        
        // Mock will be handled by the generic setup in constructor

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeycloakException>(
            () => _keycloakService.DeleteUserAsync(userId));

        exception.Message.Should().Contain("Erro ao excluir usuário no Keycloak");
    }
}