using Bogus;
using UserService.Application.Commands.Users.CreateUser;
using UserService.Application.Commands.Users.LoginUser;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Dtos.Requests;
using UserService.Application.Dtos.Responses;

namespace UserService.UnitTest.Application.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker _faker = new("pt_BR");

    #region Commands

    public static CreateUserCommand CreateValidCreateUserCommand()
    {
        return new CreateUserCommand(
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Internet.Email(),
            GenerateValidPassword(),
            _faker.Random.Bool()
        );
    }

    public static CreateUserCommand CreateCreateUserCommandWithEmail(string email)
    {
        return new CreateUserCommand(
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            email,
            GenerateValidPassword(),
            _faker.Random.Bool()
        );
    }

    public static CreateUserCommand CreateCreateUserCommandWithPassword(string password)
    {
        return new CreateUserCommand(
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Internet.Email(),
            password,
            _faker.Random.Bool()
        );
    }

    public static LoginUserCommand CreateValidLoginUserCommand()
    {
        return new LoginUserCommand
        {
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(8)
        };
    }

    public static LoginUserCommand CreateLoginUserCommandWithCredentials(string email, string password)
    {
        return new LoginUserCommand
        {
            Email = email,
            Password = password
        };
    }

    #endregion

    #region Request DTOs

    public static CreateUserRequest CreateValidCreateUserRequest()
    {
        return new CreateUserRequest
        {
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(8),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            NewsletterOptIn = _faker.Random.Bool()
        };
    }

    public static CreateUserRequest CreateCreateUserRequestWithEmail(string email)
    {
        var request = CreateValidCreateUserRequest();
        request.Email = email;
        return request;
    }

    public static LoginUserRequest CreateValidLoginUserRequest()
    {
        return new LoginUserRequest
        {
            Email = _faker.Internet.Email(),
            Password = _faker.Internet.Password(8)
        };
    }

    public static LoginUserRequest CreateLoginUserRequestWithCredentials(string email, string password)
    {
        return new LoginUserRequest
        {
            Email = email,
            Password = password
        };
    }

    #endregion

    #region Response DTOs

    public static CreateUserResponse CreateValidCreateUserResponse()
    {
        return new CreateUserResponse
        {
            UserId = Guid.NewGuid(),
            Email = _faker.Internet.Email(),
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            FullName = _faker.Name.FullName(),
            CreatedAt = DateTime.UtcNow,
            Success = true,
            Message = "Usuário criado com sucesso"
        };
    }

    public static CreateUserResponse CreateCreateUserResponseWithSuccess(bool success)
    {
        var response = CreateValidCreateUserResponse();
        response.Success = success;
        response.Message = success ? "Usuário criado com sucesso" : "Erro ao criar usuário";
        return response;
    }

    public static LoginUserResponse CreateValidLoginUserResponse()
    {
        return new LoginUserResponse
        {
            AccessToken = GenerateJwtToken(),
            RefreshToken = _faker.Random.AlphaNumeric(64),
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshExpiresIn = 7200,
            Scope = "openid profile email"
        };
    }

    public static LoginUserResponse CreateLoginUserResponseWithSuccess(bool success)
    {
        if (success)
        {
            return CreateValidLoginUserResponse();
        }
        else
        {
            return new LoginUserResponse
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                TokenType = string.Empty,
                ExpiresIn = 0,
                RefreshExpiresIn = 0,
                Scope = string.Empty
            };
        }
    }

    #endregion

    #region Keycloak DTOs

    public static CreateUserKeycloak CreateValidCreateUserKeycloak()
    {
        return new CreateUserKeycloak(
            _faker.Internet.UserName(),
            _faker.Internet.Email(),
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Internet.Password(8)
        );
    }

    public static CreateUserKeycloak CreateCreateUserKeycloakWithEmail(string email)
    {
        return new CreateUserKeycloak(
            _faker.Internet.UserName(),
            email,
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            _faker.Internet.Password(8)
        );
    }

    public static LoginUserKeycloak CreateValidLoginUserKeycloak()
    {
        return new LoginUserKeycloak(
            _faker.Internet.Email(),
            _faker.Internet.Password(8)
        );
    }

    public static LoginUserKeycloak CreateLoginUserKeycloakWithCredentials(string email, string password)
    {
        return new LoginUserKeycloak(email, password);
    }

    #endregion

    #region Helper Methods

    public static string GenerateValidEmail()
    {
        return _faker.Internet.Email();
    }

    public static string GenerateValidPassword(int minLength = 8, int maxLength = 20)
    {
        // Gera uma senha que atende aos requisitos de validação:
        // - Pelo menos uma letra maiúscula
        // - Pelo menos uma letra minúscula  
        // - Pelo menos um número
        // - Pelo menos um caractere especial
        var upperCase = _faker.Random.Char('A', 'Z');
        var lowerCase = _faker.Random.Char('a', 'z');
        var digit = _faker.Random.Char('0', '9');
        var special = _faker.PickRandom('@', '$', '!', '%', '*', '?', '&');
        
        var remainingLength = Math.Max(0, minLength - 4);
        var remaining = _faker.Random.String2(remainingLength, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@$!%*?&");
        
        var password = $"{upperCase}{lowerCase}{digit}{special}{remaining}";
        
        // Embaralha os caracteres para não ter um padrão fixo
        var shuffled = password.ToCharArray();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = _faker.Random.Int(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        
        return new string(shuffled);
    }

    public static string GenerateValidName()
    {
        return _faker.Name.FullName();
    }

    public static string GenerateValidPhone()
    {
        return _faker.Phone.PhoneNumber("(##) #####-####");
    }

    public static Guid GenerateValidGuid()
    {
        return _faker.Random.Guid();
    }

    public static string GenerateJwtToken()
    {
        // Simula um JWT token básico para testes
        var header = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            $"{{\"sub\":\"{_faker.Random.Guid()}\",\"email\":\"{_faker.Internet.Email()}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"));
        var signature = _faker.Random.AlphaNumeric(43);
        
        return $"{header}.{payload}.{signature}";
    }

    public static string GenerateInvalidEmail()
    {
        var invalidEmails = new[]
        {
            "invalid-email",
            "@domain.com",
            "user@",
            "user..name@domain.com",
            "user@domain",
            "",
            " ",
            "user name@domain.com"
        };

        return _faker.PickRandom(invalidEmails);
    }

    public static string GenerateWeakPassword()
    {
        var weakPasswords = new[]
        {
            "123",
            "abc",
            "password",
            "123456",
            "qwerty",
            "",
            " "
        };

        return _faker.PickRandom(weakPasswords);
    }

    public static string GenerateSpecialCharacterString(int length = 10)
    {
        var specialChars = "!@#$%^&*()_+-=[]{}|;':,.<>?";
        return new string(Enumerable.Repeat(specialChars, length)
            .Select(s => s[_faker.Random.Int(0, s.Length - 1)]).ToArray());
    }

    public static string GenerateUnicodeString(int length = 10)
    {
        var unicodeChars = "áéíóúàèìòùâêîôûãõçñüÁÉÍÓÚÀÈÌÒÙÂÊÎÔÛÃÕÇÑÜ";
        return new string(Enumerable.Repeat(unicodeChars, length)
            .Select(s => s[_faker.Random.Int(0, s.Length - 1)]).ToArray());
    }

    public static string GenerateLongString(int length)
    {
        return new string('a', length);
    }

    public static List<T> GenerateList<T>(Func<T> generator, int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => generator()).ToList();
    }

    #endregion

    #region Test Scenarios

    public static class Scenarios
    {
        public static class InvalidEmails
        {
            public static readonly string[] Values = 
            {
                null,
                "",
                " ",
                "invalid-email",
                "@domain.com",
                "user@",
                "user..name@domain.com",
                "user@domain",
                "user name@domain.com"
            };
        }

        public static class InvalidPasswords
        {
            public static readonly string[] Values = 
            {
                null,
                "",
                " ",
                "123",
                "abc",
                "1234567" // Less than 8 characters
            };
        }

        public static class ValidEmails
        {
            public static readonly string[] Values = 
            {
                "test@example.com",
                "user.name@domain.org",
                "user+tag@company.com.br",
                "admin@sub.domain.net",
                "123@numbers.info"
            };
        }

        public static class ValidPasswords
        {
            public static readonly string[] Values = 
            {
                "password123",
                "P@ssw0rd!",
                "MySecurePass",
                "senha_segura_123",
                "ComplexP@ss1"
            };
        }
    }

    #endregion
}