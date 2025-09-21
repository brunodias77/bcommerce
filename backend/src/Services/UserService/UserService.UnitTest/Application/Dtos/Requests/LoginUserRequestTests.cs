using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Requests;
using Xunit;

namespace UserService.UnitTest.Application.Dtos.Requests;

public class LoginUserRequestTests
{
    private readonly Faker _faker;

    public LoginUserRequestTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void LoginUserRequest_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);

        // Act
        var request = new LoginUserRequest
        {
            Email = email,
            Password = password
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserRequest_ComValoresPadrao_DeveInicializarPropriedadesCorretamente()
    {
        // Act
        var request = new LoginUserRequest();

        // Assert
        request.Email.Should().BeNull();
        request.Password.Should().BeNull();
    }

    [Theory]
    [InlineData("test@example.com", "password123")]
    [InlineData("user@domain.org", "mypassword")]
    [InlineData("admin@company.net", "admin123")]
    [InlineData("cliente@loja.com.br", "senha456")]
    public void LoginUserRequest_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string email, string password)
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = email,
            Password = password
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserRequest_ComEmailNulo_DevePermitirAtribuicao()
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = null,
            Password = "password123"
        };

        // Assert
        request.Email.Should().BeNull();
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginUserRequest_ComSenhaNula_DevePermitirAtribuicao()
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = "test@example.com",
            Password = null
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserRequest_ComEmailVazioOuEspacos_DevePermitirAtribuicao(string email)
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = email,
            Password = "password123"
        };

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be("password123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserRequest_ComSenhaVaziaOuEspacos_DevePermitirAtribuicao(string password)
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = "test@example.com",
            Password = password
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserRequest_ComEmailsEspeciais_DevePermitirAtribuicao()
    {
        // Arrange
        var specialEmails = new[]
        {
            "user+tag@example.com",
            "user.name@example.com",
            "user_name@example.com",
            "user-name@example.com",
            "123@example.com",
            "user@sub.example.com",
            "UPPERCASE@EXAMPLE.COM",
            "MixedCase@Example.Com"
        };

        foreach (var email in specialEmails)
        {
            // Act
            var request = new LoginUserRequest
            {
                Email = email,
                Password = "password123"
            };

            // Assert
            request.Email.Should().Be(email);
        }
    }

    [Fact]
    public void LoginUserRequest_ComSenhasEspeciais_DevePermitirAtribuicao()
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
            "senha com espaços",
            "señaConCaracteresEspeciales",
            "密码", // Chinese characters
            "пароль" // Cyrillic characters
        };

        foreach (var password in specialPasswords)
        {
            // Act
            var request = new LoginUserRequest
            {
                Email = "test@example.com",
                Password = password
            };

            // Assert
            request.Password.Should().Be(password);
        }
    }

    [Fact]
    public void LoginUserRequest_ComSenhaLonga_DevePermitirAtribuicao()
    {
        // Arrange
        var longPassword = new string('a', 1000); // Senha com 1000 caracteres

        // Act
        var request = new LoginUserRequest
        {
            Email = "test@example.com",
            Password = longPassword
        };

        // Assert
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be(longPassword);
        request.Password.Length.Should().Be(1000);
    }

    [Fact]
    public void LoginUserRequest_ComEmailLongo_DevePermitirAtribuicao()
    {
        // Arrange
        var longLocalPart = new string('a', 64); // Parte local máxima permitida
        var longEmail = $"{longLocalPart}@example.com";

        // Act
        var request = new LoginUserRequest
        {
            Email = longEmail,
            Password = "password123"
        };

        // Assert
        request.Email.Should().Be(longEmail);
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginUserRequest_ComCaracteresUnicode_DevePermitirAtribuicao()
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = "usuário@domínio.com.br",
            Password = "señaConAcentos123ç"
        };

        // Assert
        request.Email.Should().Be("usuário@domínio.com.br");
        request.Password.Should().Be("señaConAcentos123ç");
    }

    [Fact]
    public void LoginUserRequest_ComAmbosValoresNulos_DevePermitirAtribuicao()
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = null,
            Password = null
        };

        // Assert
        request.Email.Should().BeNull();
        request.Password.Should().BeNull();
    }

    [Fact]
    public void LoginUserRequest_ComAmbosValoresVazios_DevePermitirAtribuicao()
    {
        // Act
        var request = new LoginUserRequest
        {
            Email = "",
            Password = ""
        };

        // Assert
        request.Email.Should().Be("");
        request.Password.Should().Be("");
    }

    [Fact]
    public void LoginUserRequest_ComModificacaoAposInicializacao_DevePermitirAlteracao()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Email = "email_inicial@example.com",
            Password = "senha_inicial"
        };

        // Act
        request.Email = "email_modificado@example.com";
        request.Password = "senha_modificada";

        // Assert
        request.Email.Should().Be("email_modificado@example.com");
        request.Password.Should().Be("senha_modificada");
    }

    [Fact]
    public void LoginUserRequest_ComDadosGeradosPorFaker_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);

        // Act
        var request = new LoginUserRequest
        {
            Email = email,
            Password = password
        };

        // Assert
        request.Email.Should().Be(email);
        request.Email.Should().Contain("@");
        request.Password.Should().Be(password);
        request.Password.Length.Should().BeInRange(8, 20);
    }
}