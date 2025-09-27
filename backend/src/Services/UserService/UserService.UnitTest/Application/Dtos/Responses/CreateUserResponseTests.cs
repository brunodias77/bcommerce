using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Responses;
using Xunit;

namespace UserService.UnitTest.Application.Dtos.Responses;

public class CreateUserResponseTests
{
    private readonly Faker _faker;

    public CreateUserResponseTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void CreateUserResponse_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var id = _faker.Random.Guid();
        var email = _faker.Internet.Email();
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var phoneNumber = _faker.Phone.PhoneNumber();
        // Removed newsletterOptIn and isActive as they are not part of CreateUserResponse
        var createdAt = _faker.Date.Recent();

        // Act
        var response = new CreateUserResponse
        {
            UserId = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Message = "Usuário criado com sucesso",
            Success = true,
            CreatedAt = createdAt
        };

        // Assert
        response.UserId.Should().Be(id);
        response.Email.Should().Be(email);
        response.FirstName.Should().Be(firstName);
        response.LastName.Should().Be(lastName);
        response.FullName.Should().Be($"{firstName} {lastName}");
        response.Message.Should().Be("Usuário criado com sucesso");
        response.Success.Should().BeTrue();
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void CreateUserResponse_ComValoresPadrao_DeveInicializarPropriedadesCorretamente()
    {
        // Act
        var response = new CreateUserResponse();

        // Assert
        response.UserId.Should().Be(Guid.Empty);
        response.Email.Should().BeNull();
        response.FirstName.Should().BeNull();
        response.LastName.Should().BeNull();
        response.FullName.Should().BeNull();
        // NewsletterOptIn and IsActive are not part of CreateUserResponse
        response.CreatedAt.Should().Be(default(DateTime));
    }

    [Theory]
    [InlineData("test@example.com", "João", "Silva", "11999999999")]
    [InlineData("user@domain.org", "Maria", "Santos", "21888888888")]
    [InlineData("admin@company.net", "Pedro", "Oliveira", "31777777777")]
    public void CreateUserResponse_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string email, string firstName, string lastName, string phoneNumber)
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var response = new CreateUserResponse
        {
            UserId = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Message = "Usuário criado com sucesso",
            Success = true,
            CreatedAt = createdAt
        };

        // Assert
        response.UserId.Should().Be(id);
        response.Email.Should().Be(email);
        response.FirstName.Should().Be(firstName);
        response.LastName.Should().Be(lastName);
        response.FullName.Should().Be($"{firstName} {lastName}");
        response.Message.Should().Be("Usuário criado com sucesso");
        response.Success.Should().BeTrue();
        // NewsletterOptIn and IsActive are not part of CreateUserResponse
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void CreateUserResponse_ComIdGuidVazio_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.Empty,
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        response.UserId.Should().Be(Guid.Empty);
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().Be("João");
        response.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserResponse_ComEmailNulo_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = null,
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        response.UserId.Should().NotBe(Guid.Empty);
        response.Email.Should().BeNull();
        response.FirstName.Should().Be("João");
        response.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserResponse_ComNomesNulos_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = null,
            LastName = null
        };

        // Assert
        response.UserId.Should().NotBe(Guid.Empty);
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().BeNull();
        response.LastName.Should().BeNull();
    }

    [Fact]
    public void CreateUserResponse_ComTelefoneNulo_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva"
        };

        // Assert
        response.UserId.Should().NotBe(Guid.Empty);
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().Be("João");
        response.LastName.Should().Be("Silva");
    }

    [Fact]
    public void CreateUserResponse_ComDataCreatedAtMinValue_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva",
            CreatedAt = DateTime.MinValue
        };

        // Assert
        response.UserId.Should().NotBe(Guid.Empty);
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().Be("João");
        response.LastName.Should().Be("Silva");
        response.CreatedAt.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void CreateUserResponse_ComDataCreatedAtMaxValue_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva",
            CreatedAt = DateTime.MaxValue
        };

        // Assert
        response.UserId.Should().NotBe(Guid.Empty);
        response.Email.Should().Be("test@example.com");
        response.FirstName.Should().Be("João");
        response.LastName.Should().Be("Silva");
        response.CreatedAt.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void CreateUserResponse_ComPropriedadesBasicas_DeveDefinirCorretamente()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva",
            // NewsletterOptIn and IsActive are not part of CreateUserResponse
        };

        // Assert
        // NewsletterOptIn and IsActive are not part of CreateUserResponse
    }

    [Fact]
    public void CreateUserResponse_ComNomesComCaracteresEspeciais_DevePermitirAtribuicao()
    {
        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "José María",
            LastName = "da Silva-Santos"
        };

        // Assert
        response.FirstName.Should().Be("José María");
        response.LastName.Should().Be("da Silva-Santos");
    }

    [Fact]
    public void CreateUserResponse_ComEmailsEspeciais_DevePermitirAtribuicao()
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
            var response = new CreateUserResponse
            {
                UserId = Guid.NewGuid(),
                Email = email,
                FirstName = "João",
                LastName = "Silva"
            };

            // Assert
            response.Email.Should().Be(email);
        }
    }



    [Fact]
    public void CreateUserResponse_ComDataAtual_DeveDefinirCorretamente()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "João",
            LastName = "Silva",
            CreatedAt = now
        };

        // Assert
        response.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateUserResponse_ComModificacaoAposInicializacao_DevePermitirAlteracao()
    {
        // Arrange
        var response = new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = "email_inicial@example.com",
            FirstName = "Nome",
            LastName = "Inicial"
        };

        var novoId = Guid.NewGuid();
        var novaData = DateTime.UtcNow.AddDays(1);

        // Act
        response.UserId = novoId;
        response.Email = "email_modificado@example.com";
        response.FirstName = "Nome Modificado";
        response.LastName = "Sobrenome Modificado";
        response.CreatedAt = novaData;
        // IsActive and NewsletterOptIn are not part of CreateUserResponse

        // Assert
        response.UserId.Should().Be(novoId);
        response.Email.Should().Be("email_modificado@example.com");
        response.FirstName.Should().Be("Nome Modificado");
        response.LastName.Should().Be("Sobrenome Modificado");
        response.CreatedAt.Should().Be(novaData);
        // IsActive and NewsletterOptIn are not part of CreateUserResponse
    }
}