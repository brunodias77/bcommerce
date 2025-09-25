using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserService.Application.Commands.Users.CreateUser;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Responses;
using UserService.Application.Services;
using UserService.Application.Services.Interfaces;
using UserService.UnitTest.Application.Helpers;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Exceptions;
using UserService.Infrastructure.Data;
using Xunit;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;

namespace UserService.UnitTest.Application.Commands;

public class CreateUserCommandHandlerTests : IDisposable
{
    private readonly Mock<IKeycloakService> _keycloakServiceMock;
    private readonly Mock<IPasswordEncripter> _passwordEncripterMock;
    private readonly UserManagementDbContext _context;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;
    private readonly Faker _faker;

    public CreateUserCommandHandlerTests()
    {
        _keycloakServiceMock = new Mock<IKeycloakService>();
        _passwordEncripterMock = new Mock<IPasswordEncripter>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        
        // Configure InMemory Database
        var options = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new UserManagementDbContext(options);
        
        _handler = new CreateUserCommandHandler(_context, _passwordEncripterMock.Object, _keycloakServiceMock.Object, _emailServiceMock.Object, _loggerMock.Object);
        _faker = new Faker();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarUsuarioComSucesso()
    {
        // Arrange
        var command = new CreateUserCommand(
            _faker.Internet.Email(),
            "MinhaSenh@123",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            true
        );

        var userId = Guid.NewGuid();
        var keycloakUserId = Guid.NewGuid().ToString();

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(keycloakUserId);

        _keycloakServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((UserResponseKeycloak)null);
            
        _keycloakServiceMock
            .Setup(x => x.GetUserByIdAsync(keycloakUserId))
            .ReturnsAsync(new UserResponseKeycloak(keycloakUserId, command.Email, command.Email, command.FirstName, command.LastName, true, true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new List<string> { "user" }, new Dictionary<string, List<string>>()));
            
        _passwordEncripterMock
            .Setup(x => x.Encrypt(command.Password))
            .Returns("encrypted_password");
            
        _emailServiceMock
            .Setup(x => x.SendAccountActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Debug: Capture error details
        var errorMessage = result.IsSuccess ? "Success" : result.FirstError.ToString();
        
        result.IsSuccess.Should().BeTrue($"Expected success but got error: {errorMessage}");
        result.Value.Should().NotBeEmpty();

        _keycloakServiceMock.Verify(x => x.CreateUserAsync(
            It.Is<CreateUserKeycloak>(u => 
                u.Email == command.Email &&
                u.FirstName == command.FirstName &&
                u.LastName == command.LastName &&
                u.Password == command.Password)), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoKeycloakFalha_DeveRetornarErroSemCriarUsuarioNoBanco()
    {
        // Arrange
        var command = new CreateUserCommand(
            _faker.Internet.Email(),
            "MinhaSenh@123",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            false
        );

        var keycloakException = new KeycloakException("Erro ao criar usuário no Keycloak");

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ThrowsAsync(keycloakException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be("Falha na criação do usuário no sistema de autenticação. Tente novamente.");
        
        // Verify that user creation in Keycloak was attempted
        _keycloakServiceMock.Verify(x => x.CreateUserAsync(
            It.Is<CreateUserKeycloak>(u => u.Email == command.Email)), Times.Once);
        
        // Verify no user was created in database
        var usersInDb = await _context.Users.CountAsync();
        usersInDb.Should().Be(0);
    }

    [Fact]
    public async Task Handle_QuandoBancoDadosFalha_DeveExcluirUsuarioDoKeycloakERetornarErro()
    {
        // Arrange
        var command = new CreateUserCommand(
            _faker.Internet.Email(),
            "MinhaSenh@123",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            true
        );

        var keycloakUserId = Guid.NewGuid().ToString();

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(keycloakUserId);
            
        _keycloakServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((UserResponseKeycloak)null);
            
        _keycloakServiceMock
            .Setup(x => x.GetUserByIdAsync(keycloakUserId))
            .ReturnsAsync(new UserResponseKeycloak(keycloakUserId, command.Email, command.Email, command.FirstName, command.LastName, true, true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new List<string> { "user" }, new Dictionary<string, List<string>>()));
            
        _passwordEncripterMock
            .Setup(x => x.Encrypt(command.Password))
            .Returns("encrypted_password");

        // Simulate database failure by disposing the context to cause SaveChanges to fail
        _context.Dispose();

        _keycloakServiceMock
            .Setup(x => x.DeleteUserAsync(keycloakUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.FirstError.Should().Be("Erro interno do servidor ao criar usuário. Tente novamente.");
        
        // Verify that user creation in Keycloak was attempted
        _keycloakServiceMock.Verify(x => x.CreateUserAsync(
            It.Is<CreateUserKeycloak>(u => u.Email == command.Email)), Times.Once);
        // Verify rollback was attempted
        _keycloakServiceMock.Verify(x => x.DeleteUserAsync(keycloakUserId), Times.Once);
    }

    [Fact]
    public async Task Handle_ComNewsletterOptInFalse_DeveCriarUsuarioSemNewsletter()
    {
        // Arrange
        var command = new CreateUserCommand(
            _faker.Internet.Email(),
            "MinhaSenh@123",
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            false
        );

        var userId = Guid.NewGuid();
        var keycloakUserId = Guid.NewGuid().ToString();

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(keycloakUserId);

        _keycloakServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((UserResponseKeycloak)null);
            
        _keycloakServiceMock
            .Setup(x => x.GetUserByIdAsync(keycloakUserId))
            .ReturnsAsync(new UserResponseKeycloak(keycloakUserId, command.Email, command.Email, command.FirstName, command.LastName, true, true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new List<string> { "user" }, new Dictionary<string, List<string>>()));
            
        _passwordEncripterMock
            .Setup(x => x.Encrypt(command.Password))
            .Returns("encrypted_password");
            
        _emailServiceMock
            .Setup(x => x.SendAccountActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("João", "Silva", "joao@email.com")]
    [InlineData("Maria", "Santos", "maria@teste.com")]
    [InlineData("Pedro", "Oliveira", "pedro@exemplo.com")]
    public async Task Handle_ComDiferentesNomes_DeveCriarUsuarioCorretamente(
        string firstName, string lastName, string email)
    {
        // Arrange
        var command = new CreateUserCommand(
            email,
            "MinhaSenh@123",
            firstName,
            lastName,
            true
        );

        var userId = Guid.NewGuid();
        var keycloakUserId = Guid.NewGuid().ToString();

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(keycloakUserId);

        _keycloakServiceMock
            .Setup(x => x.GetUserByEmailAsync(command.Email))
            .ReturnsAsync((UserResponseKeycloak)null);
            
        _keycloakServiceMock
            .Setup(x => x.GetUserByIdAsync(keycloakUserId))
            .ReturnsAsync(new UserResponseKeycloak(keycloakUserId, command.Email, command.Email, command.FirstName, command.LastName, true, true, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), new List<string> { "user" }, new Dictionary<string, List<string>>()));
            
        _passwordEncripterMock
            .Setup(x => x.Encrypt(command.Password))
            .Returns("encrypted_password");
            
        _emailServiceMock
            .Setup(x => x.SendAccountActivationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}