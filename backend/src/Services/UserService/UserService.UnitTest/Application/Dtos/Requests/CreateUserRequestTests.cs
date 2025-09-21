using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Requests;
using Xunit;

namespace UserService.UnitTest.Application.Dtos.Requests;

public class CreateUserRequestTests
{
    private readonly Faker _faker;

    public CreateUserRequestTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void CreateUserRequest_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        // PhoneNumber removido - não existe no CreateUserRequest
        var newsletterOptIn = _faker.Random.Bool();

        // Act
        var request = new CreateUserRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            NewsletterOptIn = newsletterOptIn
        };

        // Assert
        request.FirstName.Should().Be(firstName);
        request.LastName.Should().Be(lastName);
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
        request.NewsletterOptIn.Should().Be(newsletterOptIn);
    }

    [Fact]
    public void CreateUserRequest_ComValoresPadrao_DeveInicializarPropriedadesCorretamente()
    {
        // Act
        var request = new CreateUserRequest();

        // Assert
        request.Email.Should().BeNull();
        request.Password.Should().BeNull();
        request.FirstName.Should().BeNull();
        request.LastName.Should().BeNull();
        // PhoneNumber removido - não existe no CreateUserRequest
        request.NewsletterOptIn.Should().BeFalse(); // bool default value
    }

    [Theory]
    [InlineData("user@example.com", "Password123!", "João", "Silva", true)]
    [InlineData("test@test.com", "MyPassword456@", "Maria", "Santos", false)]
    [InlineData("admin@company.net", "Admin123!", "Pedro", "Oliveira", true)]
    public void CreateUserRequest_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string email, string password, string firstName, string lastName, bool newsletterOptIn)
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName,
            // PhoneNumber removido - não existe no CreateUserRequest
            NewsletterOptIn = newsletterOptIn
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
        request.FirstName.Should().Be(firstName);
        request.LastName.Should().Be(lastName);
        // PhoneNumber removido - não existe no CreateUserRequest
        request.NewsletterOptIn.Should().Be(newsletterOptIn);
    }

    [Fact]
    public void CreateUserRequest_ComEmailNulo_DevePermitirAtribuicao()
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = null,
            Password = "password123",
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        request.Email.Should().BeNull();
        request.Password.Should().Be("password123");
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserRequest_ComSenhaNula_DevePermitirAtribuicao()
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = null,
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().BeNull();
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserRequest_ComNomeNulo_DevePermitirAtribuicao()
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = null,
            LastName = null
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
        request.FirstName.Should().BeNull();
        request.LastName.Should().BeNull();
    }

    [Fact]
    public void CreateUserRequest_ComTelefoneNulo_DevePermitirAtribuicao()
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "João",
            LastName = "Silva",
            // PhoneNumber removido - não existe no CreateUserRequest
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
        // PhoneNumber removido - não existe no CreateUserRequest
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserRequest_ComEmailVazioOuEspacos_DevePermitirAtribuicao(string email)
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = email,
            Password = "password123",
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be("password123");
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void CreateUserRequest_ComSenhaVaziaOuEspacos_DevePermitirAtribuicao(string password)
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = password,
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be(password);
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserRequest_ComEmailsEspeciais_DevePermitirAtribuicao()
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
            var request = new CreateUserRequest
            {
                Email = email,
                Password = "password123",
                FirstName = "João",
                LastName = "Silva"
            };

            // Assert
            request.Email.Should().Be(email);
        }
    }

    [Fact]
    public void CreateUserRequest_ComNomesComCaracteresEspeciais_DevePermitirAtribuicao()
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "José María",
            LastName = "da Silva-Santos"
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
        request.FirstName.Should().Be("José María");
        request.LastName.Should().Be("da Silva-Santos");
    }

    [Fact]
    public void CreateUserRequest_ComTelefonesDiferentes_DevePermitirAtribuicao()
    {
        // Teste de PhoneNumber removido - propriedade não existe no CreateUserRequest
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "João",
            LastName = "Silva",
            NewsletterOptIn = false
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.FirstName.Should().Be("João");
        request.LastName.Should().Be("Silva");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateUserRequest_ComDiferentesValoresNewsletterOptIn_DeveDefinirCorretamente(bool newsletterOptIn)
    {
        // Act
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Password = "password123",
            FirstName = "João",
            LastName = "Silva",
            NewsletterOptIn = newsletterOptIn
        };

        // Assert
        request.NewsletterOptIn.Should().Be(newsletterOptIn);
    }
}