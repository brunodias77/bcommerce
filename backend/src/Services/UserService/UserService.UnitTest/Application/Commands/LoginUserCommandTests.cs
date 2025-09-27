using FluentAssertions;
using UserService.Application.Commands.Users.LoginUser;
using Xunit;

namespace UserService.UnitTest.Application.Commands;

public class LoginUserCommandTests
{
    [Fact]
    public void CriarComando_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = "usuario@email.com";
        var password = "MinhaSenh@123";

        // Act
        var command = new LoginUserCommand
        {
            Email = email,
            Password = password
        };

        // Assert
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void CriarComando_ComValoresPadrao_DeveInicializarPropriedadesVazias()
    {
        // Act
        var command = new LoginUserCommand();

        // Assert
        command.Email.Should().Be(string.Empty);
        command.Password.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("joao@email.com", "Senha123@")]
    [InlineData("maria@teste.com", "OutraSenh@456")]
    [InlineData("pedro@exemplo.com", "MinhaSenh@789")]
    [InlineData("admin@sistema.com", "AdminPass@2024")]
    public void CriarComando_ComDiferentesCredenciais_DeveDefinirPropriedadesCorretamente(
        string email, string password)
    {
        // Act
        var command = new LoginUserCommand
        {
            Email = email,
            Password = password
        };

        // Assert
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void CriarComando_ComEmailNulo_DevePermitirAtribuicao()
    {
        // Arrange
        var command = new LoginUserCommand();

        // Act
        command.Email = null!;

        // Assert
        command.Email.Should().BeNull();
    }

    [Fact]
    public void CriarComando_ComSenhaNula_DevePermitirAtribuicao()
    {
        // Arrange
        var command = new LoginUserCommand();

        // Act
        command.Password = null!;

        // Assert
        command.Password.Should().BeNull();
    }

    [Fact]
    public void CriarComando_ComEmailVazio_DevePermitirAtribuicao()
    {
        // Arrange
        var command = new LoginUserCommand();

        // Act
        command.Email = "";

        // Assert
        command.Email.Should().BeEmpty();
    }

    [Fact]
    public void CriarComando_ComSenhaVazia_DevePermitirAtribuicao()
    {
        // Arrange
        var command = new LoginUserCommand();

        // Act
        command.Password = "";

        // Assert
        command.Password.Should().BeEmpty();
    }
}