using FluentAssertions;
using UserService.Application.Services;
using Xunit;

namespace UserService.UnitTest.Application.Services;

public class PasswordEncripterTests
{
    private readonly PasswordEncripter _passwordEncripter;

    public PasswordEncripterTests()
    {
        _passwordEncripter = new PasswordEncripter();
    }

    [Fact]
    public void EncryptPassword_ComSenhaValida_DeveRetornarHashBCrypt()
    {
        // Arrange
        var password = "MinhaSenh@123";

        // Act
        var hashedPassword = _passwordEncripter.Encrypt(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$"); // BCrypt hash prefix
        hashedPassword.Length.Should().Be(60); // BCrypt hash length
        hashedPassword.Should().NotBe(password); // Hash should be different from original
    }

    [Fact]
    public void EncryptPassword_ComMesmaSenha_DeveGerarHashesDiferentes()
    {
        // Arrange
        var password = "MinhaSenh@123";

        // Act
        var hash1 = _passwordEncripter.Encrypt(password);
        var hash2 = _passwordEncripter.Encrypt(password);

        // Assert
        hash1.Should().NotBe(hash2); // Different salts should produce different hashes
        hash1.Should().StartWith("$2a$");
        hash2.Should().StartWith("$2a$");
    }

    [Theory]
    [InlineData("senha123")]
    [InlineData("MinhaSenh@123")]
    [InlineData("P@ssw0rd!")]
    [InlineData("123456789")]
    [InlineData("SenhaComCaracteresEspeciais!@#$%^&*()")]
    public void EncryptPassword_ComDiferentesSenhas_DeveRetornarHashesValidos(string password)
    {
        // Act
        var hashedPassword = _passwordEncripter.Encrypt(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$");
        hashedPassword.Length.Should().Be(60);
        hashedPassword.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_ComSenhaCorreta_DeveRetornarTrue()
    {
        // Arrange
        var password = "MinhaSenh@123";
        var hashedPassword = _passwordEncripter.Encrypt(password);

        // Act
        var isValid = _passwordEncripter.Verify(password, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ComSenhaIncorreta_DeveRetornarFalse()
    {
        // Arrange
        var correctPassword = "MinhaSenh@123";
        var incorrectPassword = "SenhaIncorreta";
        var hashedPassword = _passwordEncripter.Encrypt(correctPassword);

        // Act
        var isValid = _passwordEncripter.Verify(incorrectPassword, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("senha123", "senha123", true)]
    [InlineData("MinhaSenh@123", "MinhaSenh@123", true)]
    [InlineData("P@ssw0rd!", "P@ssw0rd!", true)]
    [InlineData("senha123", "senha124", false)]
    [InlineData("MinhaSenh@123", "MinhaSenh@124", false)]
    [InlineData("P@ssw0rd!", "P@ssw0rd?", false)]
    public void VerifyPassword_ComDiferentesCombinacoes_DeveRetornarResultadoCorreto(
        string originalPassword, string testPassword, bool expectedResult)
    {
        // Arrange
        var hashedPassword = _passwordEncripter.Encrypt(originalPassword);

        // Act
        var isValid = _passwordEncripter.Verify(testPassword, hashedPassword);

        // Assert
        isValid.Should().Be(expectedResult);
    }

    [Fact]
    public void VerifyPassword_ComHashInvalido_DeveRetornarFalse()
    {
        // Arrange
        var password = "MinhaSenh@123";
        var invalidHash = "hash_invalido";

        // Act
        var isValid = _passwordEncripter.Verify(password, invalidHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ComSenhaVazia_DeveRetornarFalse()
    {
        // Arrange
        var password = "MinhaSenh@123";
        var hashedPassword = _passwordEncripter.Encrypt(password);
        var emptyPassword = "";

        // Act
        var isValid = _passwordEncripter.Verify(emptyPassword, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ComHashVazio_DeveRetornarFalse()
    {
        // Arrange
        var password = "MinhaSenh@123";
        var emptyHash = "";

        // Act
        var isValid = _passwordEncripter.Verify(password, emptyHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void EncryptPassword_ComSenhaVazia_DeveRetornarHashValido()
    {
        // Arrange
        var emptyPassword = "";

        // Act
        var hashedPassword = _passwordEncripter.Encrypt(emptyPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$");
        hashedPassword.Length.Should().Be(60);
    }

    [Fact]
    public void EncryptPassword_ComSenhaNula_DeveLancarExcecao()
    {
        // Arrange
        string? nullPassword = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _passwordEncripter.Encrypt(nullPassword!));
        
        exception.ParamName.Should().Be("password");
    }

    [Fact]
    public void VerifyPassword_ComSenhaNula_DeveLancarExcecao()
    {
        // Arrange
        string? nullPassword = null;
        var hashedPassword = _passwordEncripter.Encrypt("MinhaSenh@123");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _passwordEncripter.Verify(nullPassword!, hashedPassword));
        
        exception.ParamName.Should().Be("password");
    }

    [Fact]
    public void VerifyPassword_ComHashNulo_DeveLancarExcecao()
    {
        // Arrange
        var password = "MinhaSenh@123";
        string? nullHash = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => _passwordEncripter.Verify(password, nullHash!));
        
        exception.ParamName.Should().Be("hashedPassword");
    }

    [Fact]
    public void EncryptPassword_ComSenhaLonga_DeveProcessarCorretamente()
    {
        // Arrange
        var longPassword = new string('a', 1000); // Senha com 1000 caracteres

        // Act
        var hashedPassword = _passwordEncripter.Encrypt(longPassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$");
        hashedPassword.Length.Should().Be(60);
        
        // Verify the hash works
        var isValid = _passwordEncripter.Verify(longPassword, hashedPassword);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void EncryptPassword_ComCaracteresUnicode_DeveProcessarCorretamente()
    {
        // Arrange
        var unicodePassword = "Señ@123çãoÜñíçødé";

        // Act
        var hashedPassword = _passwordEncripter.Encrypt(unicodePassword);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().StartWith("$2a$");
        hashedPassword.Length.Should().Be(60);
        
        // Verify the hash works
        var isValid = _passwordEncripter.Verify(unicodePassword, hashedPassword);
        isValid.Should().BeTrue();
    }
}