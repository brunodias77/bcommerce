using Bogus;
using FluentAssertions;
using UserService.Application.Dtos.Responses;
using Xunit;

namespace UserService.UnitTest.Application.Dtos.Responses;

public class LoginUserResponseTests
{
    private readonly Faker _faker;

    public LoginUserResponseTests()
    {
        _faker = new Faker("pt_BR");
    }

    [Fact]
    public void LoginUserResponse_ComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var accessToken = _faker.Random.AlphaNumeric(100);
        var refreshToken = _faker.Random.AlphaNumeric(100);
        var tokenType = "Bearer";
        var expiresIn = _faker.Random.Int(3600, 7200);

        // Act
        var response = new LoginUserResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn
        };

        // Assert
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be(refreshToken);
        response.TokenType.Should().Be(tokenType);
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact]
    public void LoginUserResponse_ComValoresPadrao_DeveInicializarPropriedadesCorretamente()
    {
        // Act
        var response = new LoginUserResponse();

        // Assert
        response.AccessToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
        response.TokenType.Should().BeNull();
        response.ExpiresIn.Should().Be(0);
    }

    [Theory]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", "refresh_token_123", "Bearer", 3600)]
    [InlineData("access_token_456", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", "bearer", 7200)]
    [InlineData("token_789", "refresh_789", "JWT", 1800)]
    public void LoginUserResponse_ComDiferentesCombinacoesDeDados_DeveDefinirPropriedadesCorretamente(
        string accessToken, string refreshToken, string tokenType, int expiresIn)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn
        };

        // Assert
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be(refreshToken);
        response.TokenType.Should().Be(tokenType);
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact]
    public void LoginUserResponse_ComAccessTokenNulo_DevePermitirAtribuicao()
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = null,
            RefreshToken = "refresh_token_123",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().BeNull();
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public void LoginUserResponse_ComRefreshTokenNulo_DevePermitirAtribuicao()
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = null,
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().BeNull();
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public void LoginUserResponse_ComTokenTypeNulo_DevePermitirAtribuicao()
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            TokenType = null,
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().BeNull();
        response.ExpiresIn.Should().Be(3600);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserResponse_ComAccessTokenVazioOuEspacos_DevePermitirAtribuicao(string accessToken)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = accessToken,
            RefreshToken = "refresh_token_123",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserResponse_ComRefreshTokenVazioOuEspacos_DevePermitirAtribuicao(string refreshToken)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be(refreshToken);
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void LoginUserResponse_ComTokenTypeVazioOuEspacos_DevePermitirAtribuicao(string tokenType)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            TokenType = tokenType,
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be(tokenType);
        response.ExpiresIn.Should().Be(3600);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-3600)]
    [InlineData(int.MinValue)]
    public void LoginUserResponse_ComExpiresInNegativoOuZero_DevePermitirAtribuicao(int expiresIn)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            TokenType = "Bearer",
            ExpiresIn = expiresIn
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(3600)]
    [InlineData(7200)]
    [InlineData(86400)]
    [InlineData(int.MaxValue)]
    public void LoginUserResponse_ComExpiresInPositivo_DevePermitirAtribuicao(int expiresIn)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            TokenType = "Bearer",
            ExpiresIn = expiresIn
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(expiresIn);
    }

    [Fact]
    public void LoginUserResponse_ComTokensJWT_DevePermitirAtribuicao()
    {
        // Arrange
        var jwtAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var jwtRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyNDI2MjJ9.4Adcj3UFYzPUVaVF43FmMab6RlaQD8A9V8wFzzht-KM";

        // Act
        var response = new LoginUserResponse
        {
            AccessToken = jwtAccessToken,
            RefreshToken = jwtRefreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be(jwtAccessToken);
        response.AccessToken.Should().StartWith("eyJ");
        response.RefreshToken.Should().Be(jwtRefreshToken);
        response.RefreshToken.Should().StartWith("eyJ");
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public void LoginUserResponse_ComTokensLongos_DevePermitirAtribuicao()
    {
        // Arrange
        var longAccessToken = new string('a', 2000);
        var longRefreshToken = new string('r', 2000);

        // Act
        var response = new LoginUserResponse
        {
            AccessToken = longAccessToken,
            RefreshToken = longRefreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be(longAccessToken);
        response.AccessToken.Length.Should().Be(2000);
        response.RefreshToken.Should().Be(longRefreshToken);
        response.RefreshToken.Length.Should().Be(2000);
        response.TokenType.Should().Be("Bearer");
        response.ExpiresIn.Should().Be(3600);
    }

    [Theory]
    [InlineData("Bearer")]
    [InlineData("bearer")]
    [InlineData("BEARER")]
    [InlineData("JWT")]
    [InlineData("jwt")]
    [InlineData("Basic")]
    [InlineData("Token")]
    public void LoginUserResponse_ComDiferentesTiposDeToken_DevePermitirAtribuicao(string tokenType)
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            TokenType = tokenType,
            ExpiresIn = 3600
        };

        // Assert
        response.AccessToken.Should().Be("access_token_123");
        response.RefreshToken.Should().Be("refresh_token_123");
        response.TokenType.Should().Be(tokenType);
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public void LoginUserResponse_ComTodosValoresNulos_DevePermitirAtribuicao()
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = null,
            RefreshToken = null,
            TokenType = null,
            ExpiresIn = 0
        };

        // Assert
        response.AccessToken.Should().BeNull();
        response.RefreshToken.Should().BeNull();
        response.TokenType.Should().BeNull();
        response.ExpiresIn.Should().Be(0);
    }

    [Fact]
    public void LoginUserResponse_ComTodosValoresVazios_DevePermitirAtribuicao()
    {
        // Act
        var response = new LoginUserResponse
        {
            AccessToken = "",
            RefreshToken = "",
            TokenType = "",
            ExpiresIn = 0
        };

        // Assert
        response.AccessToken.Should().Be("");
        response.RefreshToken.Should().Be("");
        response.TokenType.Should().Be("");
        response.ExpiresIn.Should().Be(0);
    }

    [Fact]
    public void LoginUserResponse_ComModificacaoAposInicializacao_DevePermitirAlteracao()
    {
        // Arrange
        var response = new LoginUserResponse
        {
            AccessToken = "access_inicial",
            RefreshToken = "refresh_inicial",
            TokenType = "Bearer",
            ExpiresIn = 3600
        };

        // Act
        response.AccessToken = "access_modificado";
        response.RefreshToken = "refresh_modificado";
        response.TokenType = "JWT";
        response.ExpiresIn = 7200;

        // Assert
        response.AccessToken.Should().Be("access_modificado");
        response.RefreshToken.Should().Be("refresh_modificado");
        response.TokenType.Should().Be("JWT");
        response.ExpiresIn.Should().Be(7200);
    }

    [Fact]
    public void LoginUserResponse_ComDadosGeradosPorFaker_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var accessToken = _faker.Random.AlphaNumeric(100);
        var refreshToken = _faker.Random.AlphaNumeric(100);
        var tokenType = _faker.PickRandom("Bearer", "JWT", "Token");
        var expiresIn = _faker.Random.Int(1800, 7200);

        // Act
        var response = new LoginUserResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn
        };

        // Assert
        response.AccessToken.Should().Be(accessToken);
        response.AccessToken.Length.Should().Be(100);
        response.RefreshToken.Should().Be(refreshToken);
        response.RefreshToken.Length.Should().Be(100);
        response.TokenType.Should().BeOneOf("Bearer", "JWT", "Token");
        response.ExpiresIn.Should().BeInRange(1800, 7200);
    }
}