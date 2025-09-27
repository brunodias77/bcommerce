using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Keycloak;
using Xunit;

namespace UserService.UnitTest.Application.Dtos.Keycloak;

public class LoginUserKeycloakTests
{
    private readonly Faker _faker;

    public LoginUserKeycloakTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void LoginUserKeycloak_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);

        // Act
        var loginKeycloak = new LoginUserKeycloak(email, password);

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("test@example.com", "password123")]
    [InlineData("user@domain.org", "mypassword")]
    [InlineData("admin@company.net", "admin123")]
    [InlineData("usuario@empresa.com.br", "senha456")]
    public void LoginUserKeycloak_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string email, string password)
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(email, password);

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserKeycloak_ComEmailNulo_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(null, "password123");

        // Assert
        loginKeycloak.Email.Should().BeNull();
        loginKeycloak.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginUserKeycloak_ComSenhaNula_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak("test@example.com", null);

        // Assert
        loginKeycloak.Email.Should().Be("test@example.com");
        loginKeycloak.Password.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserKeycloak_ComEmailVazioOuEspacos_DevePermitirCriacao(string email)
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(email, "password123");

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Password.Should().Be("password123");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserKeycloak_ComSenhaVaziaOuEspacos_DevePermitirCriacao(string password)
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak("test@example.com", password);

        // Assert
        loginKeycloak.Email.Should().Be("test@example.com");
        loginKeycloak.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserKeycloak_ComEmailsEspeciais_DevePermitirCriacao()
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
            "user@example-domain.com",
            "user@123.456.789.012"
        };

        foreach (var email in specialEmails)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak(email, "password123");

            // Assert
            loginKeycloak.Email.Should().Be(email);
            loginKeycloak.Password.Should().Be("password123");
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComSenhasEspeciais_DevePermitirCriacao()
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
            "señaConCaracteresEspeciales",
            "password with spaces",
            "!@#$%^&*()_+",
            "123456789",
            "abcdefghijklmnopqrstuvwxyz"
        };

        foreach (var password in specialPasswords)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak("test@example.com", password);

            // Assert
            loginKeycloak.Email.Should().Be("test@example.com");
            loginKeycloak.Password.Should().Be(password);
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComTodosValoresNulos_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(null, null);

        // Assert
        loginKeycloak.Email.Should().BeNull();
        loginKeycloak.Password.Should().BeNull();
    }

    [Fact]
    public void LoginUserKeycloak_ComTodosValoresVazios_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak("", "");

        // Assert
        loginKeycloak.Email.Should().Be("");
        loginKeycloak.Password.Should().Be("");
    }

    [Fact]
    public void LoginUserKeycloak_ComSenhaLonga_DevePermitirCriacao()
    {
        // Arrange
        var longPassword = new string('a', 1000);

        // Act
        var loginKeycloak = new LoginUserKeycloak("test@example.com", longPassword);

        // Assert
        loginKeycloak.Email.Should().Be("test@example.com");
        loginKeycloak.Password.Should().Be(longPassword);
        loginKeycloak.Password.Length.Should().Be(1000);
    }

    [Fact]
    public void LoginUserKeycloak_ComEmailLongo_DevePermitirCriacao()
    {
        // Arrange
        var longEmail = new string('a', 100) + "@example.com";

        // Act
        var loginKeycloak = new LoginUserKeycloak(longEmail, "password123");

        // Assert
        loginKeycloak.Email.Should().Be(longEmail);
        loginKeycloak.Email.Length.Should().Be(112); // 100 + "@example.com".Length
        loginKeycloak.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginUserKeycloak_ComCaracteresUnicode_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(
            "usuário@domínio.com.br", 
            "señaConAcentos123ç");

        // Assert
        loginKeycloak.Email.Should().Be("usuário@domínio.com.br");
        loginKeycloak.Password.Should().Be("señaConAcentos123ç");
    }

    [Fact]
    public void LoginUserKeycloak_ComCaracteresEspeciaisNoEmail_DevePermitirCriacao()
    {
        // Arrange
        var specialEmails = new[]
        {
            "test+label@example.com",
            "test.email@example.com",
            "test_email@example.com",
            "test-email@example.com",
            "test123@example.com",
            "123test@example.com",
            "test@sub.example.com",
            "test@example-site.com"
        };

        foreach (var email in specialEmails)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak(email, "password123");

            // Assert
            loginKeycloak.Email.Should().Be(email);
            loginKeycloak.Password.Should().Be("password123");
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComSenhasComCaracteresEspeciais_DevePermitirCriacao()
    {
        // Arrange
        var specialPasswords = new[]
        {
            "P@ssw0rd!",
            "#MyPassword123",
            "password$with%symbols",
            "pass&word*with+chars",
            "(password)with[brackets]",
            "password{with}braces",
            "password|with|pipes",
            "password\\with\\backslashes",
            "password/with/slashes",
            "password?with?questions",
            "password<with>angles",
            "password=with=equals",
            "password~with~tildes",
            "password`with`backticks"
        };

        foreach (var password in specialPasswords)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak("test@example.com", password);

            // Assert
            loginKeycloak.Email.Should().Be("test@example.com");
            loginKeycloak.Password.Should().Be(password);
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComDadosGeradosPorFaker_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var email = _faker.Internet.Email();
        var password = _faker.Internet.Password(8);

        // Act
        var loginKeycloak = new LoginUserKeycloak(email, password);

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Email.Should().Contain("@");
        loginKeycloak.Password.Should().Be(password);
        loginKeycloak.Password.Length.Should().BeInRange(8, 20);
    }

    [Fact]
    public void LoginUserKeycloak_DeveSerRecord_ComIgualdadeCorreta()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password123";

        // Act
        var loginKeycloak1 = new LoginUserKeycloak(email, password);
        var loginKeycloak2 = new LoginUserKeycloak(email, password);
        var loginKeycloak3 = new LoginUserKeycloak(email, "differentPassword");

        // Assert
        loginKeycloak1.Should().Be(loginKeycloak2); // Records with same values should be equal
        loginKeycloak1.Should().NotBe(loginKeycloak3); // Records with different values should not be equal
        loginKeycloak1.GetHashCode().Should().Be(loginKeycloak2.GetHashCode());
        loginKeycloak1.GetHashCode().Should().NotBe(loginKeycloak3.GetHashCode());
    }

    [Fact]
    public void LoginUserKeycloak_ComEmailsComDominiosVariados_DevePermitirCriacao()
    {
        // Arrange
        var emailsWithDomains = new[]
        {
            "user@gmail.com",
            "user@yahoo.com",
            "user@hotmail.com",
            "user@outlook.com",
            "user@company.com.br",
            "user@organization.org",
            "user@government.gov",
            "user@education.edu",
            "user@network.net",
            "user@info.info"
        };

        foreach (var email in emailsWithDomains)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak(email, "password123");

            // Assert
            loginKeycloak.Email.Should().Be(email);
            loginKeycloak.Password.Should().Be("password123");
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComSenhasDeComprimentosVariados_DevePermitirCriacao()
    {
        // Arrange
        var passwords = new[]
        {
            "a", // 1 character
            "ab", // 2 characters
            "abc", // 3 characters
            "password", // 8 characters
            "verylongpassword", // 16 characters
            "extremelylongpasswordwithmanychars", // 34 characters
            new string('x', 100) // 100 characters
        };

        foreach (var password in passwords)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak("test@example.com", password);

            // Assert
            loginKeycloak.Email.Should().Be("test@example.com");
            loginKeycloak.Password.Should().Be(password);
            loginKeycloak.Password.Length.Should().Be(password.Length);
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComEmailsDeComprimentosVariados_DevePermitirCriacao()
    {
        // Arrange
        var emails = new[]
        {
            "a@b.c", // Minimum valid email
            "test@example.com", // Normal email
            "verylongemailaddress@verylongdomainname.com", // Long email
            new string('a', 50) + "@" + new string('b', 50) + ".com" // Very long email
        };

        foreach (var email in emails)
        {
            // Act
            var loginKeycloak = new LoginUserKeycloak(email, "password123");

            // Assert
            loginKeycloak.Email.Should().Be(email);
            loginKeycloak.Email.Length.Should().Be(email.Length);
            loginKeycloak.Password.Should().Be("password123");
        }
    }

    [Fact]
    public void LoginUserKeycloak_ComMultiplosArroba_DevePermitirCriacao()
    {
        // Arrange - Emails with multiple @ symbols (technically invalid but should be allowed by the record)
        var email = "user@@example.com";

        // Act
        var loginKeycloak = new LoginUserKeycloak(email, "password123");

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginUserKeycloak_ComEspacosNoMeio_DevePermitirCriacao()
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak("user name@example.com", "pass word");

        // Assert
        loginKeycloak.Email.Should().Be("user name@example.com");
        loginKeycloak.Password.Should().Be("pass word");
    }

    [Theory]
    [InlineData("\t", "\t")] // Tab characters
    [InlineData("\n", "\n")] // Newline characters
    [InlineData("\r", "\r")] // Carriage return
    public void LoginUserKeycloak_ComCaracteresDeControle_DevePermitirCriacao(string email, string password)
    {
        // Act
        var loginKeycloak = new LoginUserKeycloak(email, password);

        // Assert
        loginKeycloak.Email.Should().Be(email);
        loginKeycloak.Password.Should().Be(password);
    }
}