using FluentAssertions;
using UserService.Application.Commands.Users.CreateUser;
using Xunit;

namespace UserService.UnitTest.Application.Commands;

public class CreateUserCommandTests
{
    [Fact]
    public void CriarComando_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var firstName = "João";
        var lastName = "Silva";
        var email = "joao.silva@email.com";
        var password = "MinhaSenh@123";
        var newsletterOptIn = true;

        // Act
        var command = new CreateUserCommand(
            firstName,
            lastName,
            email,
            password,
            newsletterOptIn
        );

        // Assert
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
        command.NewsletterOptIn.Should().Be(newsletterOptIn);
    }

    [Fact]
    public void CriarComando_ComValoresPadrao_DeveInicializarPropriedadesVazias()
    {
        // Act
        var command = new CreateUserCommand(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false
        );

        // Assert
        command.FirstName.Should().Be(string.Empty);
        command.LastName.Should().Be(string.Empty);
        command.Email.Should().Be(string.Empty);
        command.Password.Should().Be(string.Empty);
        command.NewsletterOptIn.Should().BeFalse();
    }

    [Theory]
    [InlineData("João", "Silva", "joao@email.com", "Senha@123", true)]
    [InlineData("Maria", "Santos", "maria@teste.com", "OutraSenh@456", false)]
    [InlineData("Pedro", "Oliveira", "pedro@exemplo.com", "MinhaSenh@789", true)]
    public void CriarComando_ComDiferentesCombinacoes_DeveDefinirPropriedadesCorretamente(
        string firstName, string lastName, string email, string password, bool newsletterOptIn)
    {
        // Act
        var command = new CreateUserCommand(
            firstName,
            lastName,
            email,
            password,
            newsletterOptIn
        );

        // Assert
        command.FirstName.Should().Be(firstName);
        command.LastName.Should().Be(lastName);
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
        command.NewsletterOptIn.Should().Be(newsletterOptIn);
    }

    [Fact]
    public void CriarComando_ComEmailNulo_DevePermitirAtribuicao()
    {
        // Act
        var command = new CreateUserCommand(
            "Nome",
            "Sobrenome",
            null!,
            "Senha@123",
            false
        );

        // Assert
        command.Email.Should().BeNull();
    }

    [Fact]
    public void CriarComando_ComSenhaNula_DevePermitirAtribuicao()
    {
        // Act
        var command = new CreateUserCommand(
            "Nome",
            "Sobrenome",
            "email@teste.com",
            null!,
            false
        );

        // Assert
        command.Password.Should().BeNull();
    }
}