using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.UnitTest.Common;

public abstract class TestBase
{
    protected readonly Faker Faker;
    protected readonly Mock<ILogger> MockLogger;

    protected TestBase()
    {
        Faker = new Faker("pt_BR");
        MockLogger = new Mock<ILogger>();
    }

    protected static string GenerateValidCpf()
    {
        var random = new Random();
        var cpf = new int[11];

        // Gera os 9 primeiros dígitos
        for (int i = 0; i < 9; i++)
        {
            cpf[i] = random.Next(0, 10);
        }

        // Calcula o primeiro dígito verificador
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += cpf[i] * (10 - i);
        }
        int remainder = sum % 11;
        cpf[9] = remainder < 2 ? 0 : 11 - remainder;

        // Calcula o segundo dígito verificador
        sum = 0;
        for (int i = 0; i < 10; i++)
        {
            sum += cpf[i] * (11 - i);
        }
        remainder = sum % 11;
        cpf[10] = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", cpf);
    }

    protected static string GenerateValidEmail()
    {
        var faker = new Faker();
        return faker.Internet.Email();
    }

    protected static string GenerateValidPassword()
    {
        // Senha que atende aos critérios: pelo menos 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 especial
        return "MinhaSenh@123";
    }

    protected static DateTime GetValidBirthDate()
    {
        return DateTime.Now.AddYears(-25); // 25 anos atrás
    }

    protected static string GenerateValidPhone()
    {
        var faker = new Faker("pt_BR");
        return faker.Phone.PhoneNumber("(##) #####-####");
    }
}