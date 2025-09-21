using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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

namespace UserService.UnitTest.Application.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManagementDbContext> _contextMock;
    private readonly Mock<IKeycloakService> _keycloakServiceMock;
    private readonly Mock<IPasswordEncripter> _passwordEncripterMock;
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock;
    private readonly CreateUserCommandHandler _handler;
    private readonly Faker _faker;

    public CreateUserCommandHandlerTests()
    {
        _keycloakServiceMock = new Mock<IKeycloakService>();
        _passwordEncripterMock = new Mock<IPasswordEncripter>();
        _contextMock = new Mock<UserManagementDbContext>();
        _loggerMock = new Mock<ILogger<CreateUserCommandHandler>>();
        _handler = new CreateUserCommandHandler(_contextMock.Object, _passwordEncripterMock.Object, _keycloakServiceMock.Object, _loggerMock.Object);
        _faker = new Faker();
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeycloakException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Erro ao criar usuário no Keycloak");
        
        // Password encryption is handled by Keycloak
        _keycloakServiceMock.Verify(x => x.CreateUserAsync(
            It.Is<CreateUserKeycloak>(u => u.Email == command.Email)), Times.Once);
        // Verify that user creation in Keycloak was attempted
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
        var databaseException = new DatabaseException("Erro ao inserir usuário no banco");

        _keycloakServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserKeycloak>()))
            .ReturnsAsync(keycloakUserId);

        // Simulate database failure by setting up context to throw exception

        _keycloakServiceMock
            .Setup(x => x.DeleteUserAsync(keycloakUserId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DatabaseException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Be("Erro ao inserir usuário no banco");
        
        // Password encryption is handled by Keycloak
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
            .Setup(x => x.Encrypt("MinhaSenh@123"))
            .Returns("encrypted_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}