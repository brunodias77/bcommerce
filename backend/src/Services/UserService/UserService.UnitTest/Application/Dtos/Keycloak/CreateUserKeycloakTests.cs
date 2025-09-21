using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Keycloak;
using Xunit;
using System.Collections.Generic;

namespace UserService.UnitTest.Application.Dtos.Keycloak;

public class CreateUserKeycloakTests
{
    private readonly Faker _faker;

    public CreateUserKeycloakTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void CreateUserKeycloak_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var enabled = _faker.Random.Bool();

        // Act
        var keycloakUser = new CreateUserKeycloak(email, email, firstName, lastName, password, enabled);

        // Assert
        keycloakUser.Email.Should().Be(email);
        keycloakUser.Password.Should().Be(password);
        keycloakUser.FirstName.Should().Be(firstName);
        keycloakUser.LastName.Should().Be(lastName);
        keycloakUser.Enabled.Should().Be(enabled);
    }

    [Theory]
    [InlineData("test@example.com", "password123", "João", "Silva", true)]
    [InlineData("user@domain.org", "mypassword", "Maria", "Santos", false)]
    [InlineData("admin@company.net", "admin123", "Pedro", "Oliveira", true)]
    public void CreateUserKeycloak_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string email, string password, string firstName, string lastName, bool enabled)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(email, email, firstName, lastName, password, enabled);

        // Assert
        keycloakUser.Email.Should().Be(email);
        keycloakUser.Password.Should().Be(password);
        keycloakUser.FirstName.Should().Be(firstName);
        keycloakUser.LastName.Should().Be(lastName);
        keycloakUser.Enabled.Should().Be(enabled);
    }

    [Fact]
    public void CreateUserKeycloak_ComEmailNulo_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(null, null, "João", "Silva", "password123");

        // Assert
        keycloakUser.Email.Should().BeNull();
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComSenhaNula_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", "Silva", null);

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().BeNull();
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComNomeNulo_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", null, "Silva", "password123");

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().BeNull();
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComSobrenomeNulo_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", null, "password123");

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().BeNull();
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserKeycloak_ComEmailVazioOuEspacos_DevePermitirCriacao(string email)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(email, email, "João", "Silva", "password123");

        // Assert
        keycloakUser.Email.Should().Be(email);
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserKeycloak_ComSenhaVaziaOuEspacos_DevePermitirCriacao(string password)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", "Silva", password);

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be(password);
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserKeycloak_ComNomeVazioOuEspacos_DevePermitirCriacao(string firstName)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", firstName, "Silva", "password123");

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be(firstName);
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserKeycloak_ComSobrenomeVazioOuEspacos_DevePermitirCriacao(string lastName)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", lastName, "password123");

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be(lastName);
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateUserKeycloak_ComDiferentesValoresEnabled_DeveDefinirCorretamente(bool enabled)
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", "Silva", "password123", enabled);

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().Be(enabled);
    }

    [Fact]
    public void CreateUserKeycloak_ComEmailsEspeciais_DevePermitirCriacao()
    {
        // Arrange
        var specialEmails = new[]
        {
            "user+tag@example.com",
            "user.name@example.com",
            "user_name@example.com",
            "user-name@example.com",
            "123@example.com",
            "user@sub.example.com"
        };

        foreach (var email in specialEmails)
        {
            // Act
            var keycloakUser = new CreateUserKeycloak(email, email, "João", "Silva", "password123");

            // Assert
            keycloakUser.Email.Should().Be(email);
        }
    }

    [Fact]
    public void CreateUserKeycloak_ComNomesComCaracteresEspeciais_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(
            "test@example.com",
            "test@example.com", 
            "José María", 
            "da Silva-Santos", 
            "password123",
            true,
            false,
            new List<string>());

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be("José María");
        keycloakUser.LastName.Should().Be("da Silva-Santos");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComSenhasEspeciais_DevePermitirCriacao()
    {
        // Arrange
        var specialPasswords = new[]
        {
            "P@ssw0rd!",
            "senha_com_underscore",
            "senha-com-hifen",
            "senha123",
            "SENHA_MAIUSCULA",
            "SenhaMixta123!",
            "señaConCaracteresEspeciales"
        };

        foreach (var password in specialPasswords)
        {
            // Act
            var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", "Silva", password);

            // Assert
            keycloakUser.Password.Should().Be(password);
        }
    }

    [Fact]
    public void CreateUserKeycloak_ComTodosValoresNulos_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(null, null, null, null, null, false);

        // Assert
        keycloakUser.Email.Should().BeNull();
        keycloakUser.Password.Should().BeNull();
        keycloakUser.FirstName.Should().BeNull();
        keycloakUser.LastName.Should().BeNull();
        keycloakUser.Enabled.Should().BeFalse();
    }

    [Fact]
    public void CreateUserKeycloak_ComTodosValoresVazios_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak("", "", "", "", "", false);

        // Assert
        keycloakUser.Email.Should().Be("");
        keycloakUser.Password.Should().Be("");
        keycloakUser.FirstName.Should().Be("");
        keycloakUser.LastName.Should().Be("");
        keycloakUser.Enabled.Should().BeFalse();
    }

    [Fact]
    public void CreateUserKeycloak_ComSenhaLonga_DevePermitirCriacao()
    {
        // Arrange
        var longPassword = new string('a', 1000);

        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", "João", "Silva", longPassword);

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be(longPassword);
        keycloakUser.Password.Length.Should().Be(1000);
        keycloakUser.FirstName.Should().Be("João");
        keycloakUser.LastName.Should().Be("Silva");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComNomesLongos_DevePermitirCriacao()
    {
        // Arrange
        var longFirstName = new string('J', 100);
        var longLastName = new string('S', 100);

        // Act
        var keycloakUser = new CreateUserKeycloak("test@example.com", "test@example.com", longFirstName, longLastName, "password123");

        // Assert
        keycloakUser.Email.Should().Be("test@example.com");
        keycloakUser.Password.Should().Be("password123");
        keycloakUser.FirstName.Should().Be(longFirstName);
        keycloakUser.FirstName.Length.Should().Be(100);
        keycloakUser.LastName.Should().Be(longLastName);
        keycloakUser.LastName.Length.Should().Be(100);
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComCaracteresUnicode_DevePermitirCriacao()
    {
        // Act
        var keycloakUser = new CreateUserKeycloak(
            "usuário@domínio.com.br",
            "usuário@domínio.com.br", 
            "José María", 
            "González", 
            "señaConAcentos123ç");

        // Assert
        keycloakUser.Email.Should().Be("usuário@domínio.com.br");
        keycloakUser.Password.Should().Be("señaConAcentos123ç");
        keycloakUser.FirstName.Should().Be("José María");
        keycloakUser.LastName.Should().Be("González");
        keycloakUser.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CreateUserKeycloak_ComDadosGeradosPorFaker_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var enabled = _faker.Random.Bool();

        // Act
        var keycloakUser = new CreateUserKeycloak(email, email, firstName, lastName, password, enabled);

        // Assert
        keycloakUser.Email.Should().Be(email);
        keycloakUser.Email.Should().Contain("@");
        keycloakUser.Password.Should().Be(password);
        keycloakUser.Password.Length.Should().BeInRange(8, 20);
        keycloakUser.FirstName.Should().Be(firstName);
        keycloakUser.FirstName.Should().NotBeNullOrEmpty();
        keycloakUser.LastName.Should().Be(lastName);
        keycloakUser.LastName.Should().NotBeNullOrEmpty();
        keycloakUser.Enabled.Should().Be(enabled);
    }

    [Fact]
    public void CreateUserKeycloak_DeveSerRecord_ComIgualdadeCorreta()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";
        var firstName = "João";
        var lastName = "Silva";
        var enabled = true;

        // Act
        var keycloakUser1 = new CreateUserKeycloak(email, email, firstName, lastName, password, enabled);
        var keycloakUser2 = new CreateUserKeycloak(email, email, firstName, lastName, password, enabled);
        var keycloakUser3 = new CreateUserKeycloak(email, email, firstName, lastName, password, false);

        // Assert
        keycloakUser1.Should().Be(keycloakUser2); // Records with same values should be equal
        keycloakUser1.Should().NotBe(keycloakUser3); // Records with different values should not be equal
        keycloakUser1.GetHashCode().Should().Be(keycloakUser2.GetHashCode());
        keycloakUser1.GetHashCode().Should().NotBe(keycloakUser3.GetHashCode());
    }
}