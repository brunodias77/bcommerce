using Bogus;
using Moq;
using Microsoft.Extensions.Logging;
using UserService.Application.Services.Interfaces;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;
using UserService.UnitTest.Application.Helpers;
using Xunit;

namespace UserService.UnitTest.Application.Helpers;

public abstract class ApplicationTestBase : IDisposable
{
    protected readonly Faker Faker;
    protected readonly Mock<IKeycloakService> MockKeycloakService;
    protected readonly Mock<IPasswordEncripter> MockPasswordEncripter;
    protected readonly Mock<ILogger> MockLogger;
    protected readonly CancellationTokenSource CancellationTokenSource;
    protected readonly CancellationToken CancellationToken;

    protected ApplicationTestBase()
    {
        Faker = new Faker("pt_BR");
        MockKeycloakService = new Mock<IKeycloakService>();
        MockPasswordEncripter = new Mock<IPasswordEncripter>();
        MockLogger = new Mock<ILogger>();
        CancellationTokenSource = new CancellationTokenSource();
        CancellationToken = CancellationTokenSource.Token;

        SetupDefaultMocks();
    }

    protected virtual void SetupDefaultMocks()
    {
        // Setup default behavior for PasswordEncripter
        MockPasswordEncripter
            .Setup(x => x.Encrypt(It.IsAny<string>()))
            .Returns<string>(password => $"encrypted_{password}");

        MockPasswordEncripter
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((password, hash) => hash == $"encrypted_{password}");

        // Setup default behavior for KeycloakService
        MockKeycloakService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(string.Empty);

        MockKeycloakService
            .Setup(x => x.LoginAsync(It.IsAny<LoginUserKeycloak>()))
            .ReturnsAsync(new LoginResponseKeycloak(
                AccessToken: "fake_token",
                ExpiresIn: 3600,
                RefreshExpiresIn: 86400,
                RefreshToken: "fake_refresh_token",
                TokenType: "Bearer",
                Scope: "openid"
            ));

        MockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserResponseKeycloak?)null);

        MockKeycloakService
            .Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    protected void SetupKeycloakCreateUserSuccess(string expectedUserId = null)
    {
        var userId = expectedUserId ?? Faker.Random.Guid().ToString();
        MockKeycloakService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(userId);
    }

    protected void SetupKeycloakCreateUserFailure(Exception exception = null)
    {
        var ex = exception ?? new InvalidOperationException("Erro ao criar usuário no Keycloak");
        MockKeycloakService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ThrowsAsync(ex);
    }

    protected void SetupKeycloakLoginSuccess(string expectedToken = null)
    {
        var token = expectedToken ?? TestDataBuilder.GenerateJwtToken();
        MockKeycloakService
            .Setup(x => x.LoginAsync(It.IsAny<LoginUserKeycloak>()))
            .ReturnsAsync(new LoginResponseKeycloak(
                AccessToken: token,
                ExpiresIn: 3600,
                RefreshExpiresIn: 86400,
                RefreshToken: "fake_refresh_token",
                TokenType: "Bearer",
                Scope: "openid"
            ));
    }

    protected void SetupKeycloakLoginFailure(Exception exception = null)
    {
        var ex = exception ?? new UnauthorizedAccessException("Credenciais inválidas");
        MockKeycloakService
            .Setup(x => x.LoginAsync(It.IsAny<LoginUserKeycloak>()))
            .ThrowsAsync(ex);
    }

    protected void SetupKeycloakGetUserSuccess(string expectedUserId = null)
    {
        var userId = expectedUserId ?? Faker.Random.Guid().ToString();
        MockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserResponseKeycloak(
                Id: userId,
                Username: "testuser",
                Email: "test@example.com",
                FirstName: "Test",
                LastName: "User",
                Enabled: true,
                EmailVerified: true,
                CreatedTimestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Roles: new List<string> { "user" },
                Attributes: new Dictionary<string, List<string>>()
            ));
    }

    protected void SetupKeycloakGetUserNotFound()
    {
        MockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserResponseKeycloak?)null);
    }

    protected void SetupKeycloakGetUserFailure(Exception exception = null)
    {
        var ex = exception ?? new InvalidOperationException("Erro ao buscar usuário no Keycloak");
        MockKeycloakService
            .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(ex);
    }

    protected void SetupKeycloakDeleteUserSuccess()
    {
        MockKeycloakService
            .Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    protected void SetupKeycloakDeleteUserFailure(Exception exception = null)
    {
        var ex = exception ?? new InvalidOperationException("Erro ao excluir usuário no Keycloak");
        MockKeycloakService
            .Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
            .ThrowsAsync(ex);
    }

    protected void SetupPasswordEncryptionSuccess(string expectedHash = null)
    {
        MockPasswordEncripter
            .Setup(x => x.Encrypt(It.IsAny<string>()))
            .Returns<string>(password => expectedHash ?? $"encrypted_{password}");
    }

    protected void SetupPasswordEncryptionFailure(Exception exception = null)
    {
        var ex = exception ?? new InvalidOperationException("Erro ao criptografar senha");
        MockPasswordEncripter
            .Setup(x => x.Encrypt(It.IsAny<string>()))
            .Throws(ex);
    }

    protected void SetupPasswordVerificationSuccess(bool isValid = true)
    {
        MockPasswordEncripter
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(isValid);
    }

    protected void SetupPasswordVerificationFailure(Exception exception = null)
    {
        var ex = exception ?? new InvalidOperationException("Erro ao verificar senha");
        MockPasswordEncripter
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(ex);
    }

    protected void VerifyKeycloakCreateUserCalled(Times times)
    {
        MockKeycloakService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()),
            times);
    }

    protected void VerifyKeycloakCreateUserCalled(string email, string password, string firstName, string lastName)
    {
        MockKeycloakService.Verify(
            x => x.CreateUserAsync(It.Is<CreateUserKeycloak>(u => u.Email == email && u.Password == password && u.FirstName == firstName && u.LastName == lastName)),
            Times.Once);
    }

    protected void VerifyKeycloakLoginCalled(Times times)
    {
        MockKeycloakService.Verify(
            x => x.LoginAsync(It.IsAny<LoginUserKeycloak>()),
            times);
    }

    protected void VerifyKeycloakLoginCalled(string email, string password)
    {
        MockKeycloakService.Verify(
            x => x.LoginAsync(It.Is<LoginUserKeycloak>(l => l.Email == email && l.Password == password)),
            Times.Once);
    }

    protected void VerifyKeycloakGetUserCalled(Times times)
    {
        MockKeycloakService.Verify(
            x => x.GetUserByEmailAsync(It.IsAny<string>()),
            times);
    }

    protected void VerifyKeycloakGetUserCalled(string email)
    {
        MockKeycloakService.Verify(
            x => x.GetUserByEmailAsync(email),
            Times.Once);
    }

    protected void VerifyKeycloakDeleteUserCalled(Times times)
    {
        MockKeycloakService.Verify(
            x => x.DeleteUserAsync(It.IsAny<string>()),
            times);
    }

    protected void VerifyKeycloakDeleteUserCalled(string userId)
    {
        MockKeycloakService.Verify(
            x => x.DeleteUserAsync(userId),
            Times.Once);
    }

    protected void VerifyPasswordEncryptCalled(Times times)
    {
        MockPasswordEncripter.Verify(
            x => x.Encrypt(It.IsAny<string>()),
            times);
    }

    protected void VerifyPasswordEncryptCalled(string password)
    {
        MockPasswordEncripter.Verify(
            x => x.Encrypt(password),
            Times.Once);
    }

    protected void VerifyPasswordVerifyCalled(Times times)
    {
        MockPasswordEncripter.Verify(
            x => x.Verify(It.IsAny<string>(), It.IsAny<string>()),
            times);
    }

    protected void VerifyPasswordVerifyCalled(string password, string hash)
    {
        MockPasswordEncripter.Verify(
            x => x.Verify(password, hash),
            Times.Once);
    }

    protected void VerifyNoKeycloakInteractions()
    {
        MockKeycloakService.VerifyNoOtherCalls();
    }

    protected void VerifyNoPasswordEncripterInteractions()
    {
        MockPasswordEncripter.VerifyNoOtherCalls();
    }

    protected void ResetAllMocks()
    {
        MockKeycloakService.Reset();
        MockPasswordEncripter.Reset();
        MockLogger.Reset();
        SetupDefaultMocks();
    }

    protected static void AssertValidGuid(Guid guid)
    {
        Assert.NotEqual(Guid.Empty, guid);
    }

    protected static void AssertValidEmail(string email)
    {
        Assert.NotNull(email);
        Assert.NotEmpty(email);
        Assert.Contains("@", email);
    }

    protected static void AssertValidJwtToken(string token)
    {
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens have dots separating parts
        var parts = token.Split('.');
        Assert.True(parts.Length >= 2); // At least header and payload
    }

    protected static void AssertSuccessResponse<T>(T response) where T : class
    {
        Assert.NotNull(response);
        
        // Use reflection to check for Success property
        var successProperty = typeof(T).GetProperty("Success");
        if (successProperty != null)
        {
            var success = (bool)successProperty.GetValue(response);
            Assert.True(success);
        }
    }

    protected static void AssertFailureResponse<T>(T response) where T : class
    {
        Assert.NotNull(response);
        
        // Use reflection to check for Success property
        var successProperty = typeof(T).GetProperty("Success");
        if (successProperty != null)
        {
            var success = (bool)successProperty.GetValue(response);
            Assert.False(success);
        }
    }

    public virtual void Dispose()
    {
        CancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}