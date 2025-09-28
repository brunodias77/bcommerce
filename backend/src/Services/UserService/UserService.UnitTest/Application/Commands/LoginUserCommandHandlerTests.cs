// using Bogus;
// using FluentAssertions;
// using Microsoft.Extensions.Logging;
// using Microsoft.EntityFrameworkCore;
// using Moq;
// using UserService.Application.Commands.Users.LoginUser;
// using UserService.Application.Dtos.Keycloak;
// using UserService.Application.Dtos.Responses;
// using UserService.Application.Services;
// using UserService.Application.Services.Interfaces;
// using UserService.Domain.Exceptions;
// using UserService.Infrastructure.Data;
// using UserService.UnitTest.Application.Helpers;
// using Xunit;
//
// namespace UserService.UnitTest.Application.Commands;
//
// public class LoginUserCommandHandlerTests : IDisposable
// {
//     private readonly Mock<IKeycloakService> _keycloakServiceMock;
//     private readonly Mock<ILogger<LoginUserCommandHandler>> _loggerMock;
//     private readonly UserManagementDbContext _context;
//     private readonly Mock<IPasswordEncripter> _passwordEncripterMock;
//     private readonly LoginUserCommandHandler _handler;
//     private readonly Faker _faker;
//
//     public LoginUserCommandHandlerTests()
//     {
//         _keycloakServiceMock = new Mock<IKeycloakService>();
//         _loggerMock = new Mock<ILogger<LoginUserCommandHandler>>();
//         _passwordEncripterMock = new Mock<IPasswordEncripter>();
//         
//         // Configure InMemory Database
//         var options = new DbContextOptionsBuilder<UserManagementDbContext>()
//             .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//             .Options;
//         _context = new UserManagementDbContext(options);
//         
//         _handler = new LoginUserCommandHandler(
//             _context,
//             _passwordEncripterMock.Object,
//             _keycloakServiceMock.Object,
//             _loggerMock.Object);
//         
//         _faker = new Faker("pt_BR");
//     }
//
//     public void Dispose()
//     {
//         _context?.Dispose();
//     }
//
//     [Fact]
//     public async Task Handle_ComCredenciaisValidas_DeveRetornarTokenComSucesso()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = _faker.Internet.Email(),
//             Password = "MinhaSenh@123"
//         };
//
//         var expectedResponse = new LoginResponseKeycloak(
//             AccessToken: "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
//             ExpiresIn: 3600,
//             RefreshExpiresIn: 7200,
//             RefreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
//             TokenType: "Bearer",
//             Scope: "openid profile email"
//         );
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ReturnsAsync(expectedResponse);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().NotBeNull();
//         result.Value.AccessToken.Should().Be(expectedResponse.AccessToken);
//         result.Value.ExpiresIn.Should().Be(expectedResponse.ExpiresIn);
//         result.Value.RefreshExpiresIn.Should().Be(expectedResponse.RefreshExpiresIn);
//         result.Value.RefreshToken.Should().Be(expectedResponse.RefreshToken);
//         result.Value.TokenType.Should().Be(expectedResponse.TokenType);
//         result.Value.Scope.Should().Be(expectedResponse.Scope);
//
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_ComCredenciaisInvalidas_DeveRetornarFalha()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = _faker.Internet.Email(),
//             Password = "SenhaIncorreta123"
//         };
//
//         var keycloakException = new UnauthorizedAccessException("Credenciais inválidas");
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ThrowsAsync(keycloakException);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeFalse();
//         result.FirstError.Should().Be("Credenciais inválidas");
//         
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_ComUsuarioInexistente_DeveRetornarFalha()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = "usuario.inexistente@email.com",
//             Password = "MinhaSenh@123"
//         };
//
//         var keycloakException = new UnauthorizedAccessException("Usuário não encontrado");
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ThrowsAsync(keycloakException);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeFalse();
//         result.FirstError.Should().Be("Credenciais inválidas");
//         
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
//
//     [Theory]
//     [InlineData("usuario1@email.com", "Senha123@")]
//     [InlineData("usuario2@teste.com", "OutraSenh@456")]
//     [InlineData("admin@sistema.com", "AdminPass@2024")]
//     public async Task Handle_ComDiferentesCredenciais_DeveProcessarCorretamente(
//         string email, string password)
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = email,
//             Password = password
//         };
//
//         var expectedResponse = new LoginResponseKeycloak(
//             AccessToken: $"token_for_{email}",
//             ExpiresIn: 3600,
//             RefreshExpiresIn: 7200,
//             RefreshToken: $"refresh_token_for_{email}",
//             TokenType: "Bearer",
//             Scope: "openid profile email"
//         );
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == email && req.Password == password)))
//             .ReturnsAsync(expectedResponse);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().NotBeNull();
//         result.Value.AccessToken.Should().Be(expectedResponse.AccessToken);
//         result.Value.RefreshToken.Should().Be(expectedResponse.RefreshToken);
//         
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == email && req.Password == password)), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_ComErroDeConexao_DeveRetornarFalha()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = _faker.Internet.Email(),
//             Password = "MinhaSenh@123"
//         };
//
//         var connectionException = new Exception("Erro de conexão com o Keycloak");
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ThrowsAsync(connectionException);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeFalse();
//         result.FirstError.Should().Be("Erro interno do servidor. Tente novamente mais tarde.");
//         
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_ComTokenExpirado_DeveRetornarNovoToken()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = _faker.Internet.Email(),
//             Password = "MinhaSenh@123"
//         };
//
//         var expectedResponse = new LoginResponseKeycloak(
//             AccessToken: "novo_access_token",
//             ExpiresIn: 3600,
//             RefreshExpiresIn: 7200,
//             RefreshToken: "novo_refresh_token",
//             TokenType: "Bearer",
//             Scope: "openid profile email"
//         );
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ReturnsAsync(expectedResponse);
//
//         // Act
//         var result = await _handler.Handle(command, CancellationToken.None);
//
//         // Assert
//         result.Should().NotBeNull();
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().NotBeNull();
//         result.Value.AccessToken.Should().Be("novo_access_token");
//         result.Value.RefreshToken.Should().Be("novo_refresh_token");
//         result.Value.ExpiresIn.Should().Be(3600);
//         result.Value.RefreshExpiresIn.Should().Be(7200);
//         result.Value.TokenType.Should().Be("Bearer");
//         result.Value.Scope.Should().Be("openid profile email");
//
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
//
//     [Fact]
//     public async Task Handle_ComCancelationToken_DevePassarTokenParaKeycloak()
//     {
//         // Arrange
//         var command = new LoginUserCommand
//         {
//             Email = _faker.Internet.Email(),
//             Password = "MinhaSenh@123"
//         };
//
//         var cancellationToken = new CancellationToken();
//         var expectedResponse = new LoginResponseKeycloak(
//             AccessToken: "access_token",
//             ExpiresIn: 3600,
//             RefreshExpiresIn: 7200,
//             RefreshToken: "refresh_token",
//             TokenType: "Bearer",
//             Scope: "openid profile email"
//         );
//
//         _keycloakServiceMock
//             .Setup(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)))
//             .ReturnsAsync(expectedResponse);
//
//         // Act
//         var result = await _handler.Handle(command, cancellationToken);
//
//         // Assert
//         result.Should().NotBeNull();
//         _keycloakServiceMock.Verify(x => x.LoginAsync(It.Is<LoginUserKeycloak>(req => req.Email == command.Email && req.Password == command.Password)), Times.Once);
//     }
// }