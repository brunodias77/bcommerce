using Bogus;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.UnitTest.Common;

public class UserTestDataBuilder
{
    private readonly Faker<User> _faker;

    public UserTestDataBuilder()
    {
        _faker = new Faker<User>("pt_BR")
            .RuleFor(u => u.UserId, f => Guid.NewGuid())
            .RuleFor(u => u.KeycloakId, f => f.Random.Guid())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Cpf, f => GenerateValidCpf())
            .RuleFor(u => u.DateOfBirth, f => f.Date.Past(50, DateTime.Now.AddYears(-18)))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber("(##) #####-####"))
            .RuleFor(u => u.NewsletterOptIn, f => f.Random.Bool())
            .RuleFor(u => u.Status, f => f.PickRandom<UserStatus>())
            .RuleFor(u => u.Role, f => f.PickRandom<UserRole>())
            .RuleFor(u => u.CreatedAt, f => f.Date.Recent(1))
            .RuleFor(u => u.UpdatedAt, f => f.Date.Recent())
            .RuleFor(u => u.DeletedAt, f => null);
    }

    public UserTestDataBuilder WithUserId(Guid userId)
    {
        _faker.RuleFor(u => u.UserId, userId);
        return this;
    }

    public UserTestDataBuilder WithKeycloakId(Guid keycloakId)
    {
        _faker.RuleFor(u => u.KeycloakId, keycloakId);
        return this;
    }

    public UserTestDataBuilder WithFirstName(string firstName)
    {
        _faker.RuleFor(u => u.FirstName, firstName);
        return this;
    }

    public UserTestDataBuilder WithLastName(string lastName)
    {
        _faker.RuleFor(u => u.LastName, lastName);
        return this;
    }

    public UserTestDataBuilder WithEmail(string email)
    {
        _faker.RuleFor(u => u.Email, email);
        return this;
    }

    public UserTestDataBuilder WithCpf(string cpf)
    {
        _faker.RuleFor(u => u.Cpf, cpf);
        return this;
    }

    public UserTestDataBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _faker.RuleFor(u => u.DateOfBirth, dateOfBirth);
        return this;
    }

    public UserTestDataBuilder WithPhone(string phone)
    {
        _faker.RuleFor(u => u.Phone, phone);
        return this;
    }

    public UserTestDataBuilder WithNewsletterOptIn(bool optIn)
    {
        _faker.RuleFor(u => u.NewsletterOptIn, optIn);
        return this;
    }

    public UserTestDataBuilder WithStatus(UserStatus status = UserStatus.Ativo)
    {
        _faker.RuleFor(u => u.Status, status);
        return this;
    }

    public UserTestDataBuilder WithInactiveStatus()
    {
        _faker.RuleFor(u => u.Status, UserStatus.Inativo);
        return this;
    }

    public UserTestDataBuilder WithBannedStatus()
    {
        _faker.RuleFor(u => u.Status, UserStatus.Banido);
        return this;
    }

    public UserTestDataBuilder WithRole(UserRole role)
    {
        _faker.RuleFor(u => u.Role, role);
        return this;
    }

    public UserTestDataBuilder WithCreatedAt(DateTime createdAt)
    {
        _faker.RuleFor(u => u.CreatedAt, createdAt);
        return this;
    }

    public UserTestDataBuilder WithUpdatedAt(DateTime updatedAt)
    {
        _faker.RuleFor(u => u.UpdatedAt, updatedAt);
        return this;
    }

    public UserTestDataBuilder AsDeleted(DateTime? deletedAt = null)
    {
        _faker.RuleFor(u => u.DeletedAt, deletedAt ?? DateTime.UtcNow);
        return this;
    }

    public UserTestDataBuilder AsActive()
    {
        _faker.RuleFor(u => u.Status, UserStatus.Ativo);
        return this;
    }

    public UserTestDataBuilder AsInactive()
    {
        _faker.RuleFor(u => u.Status, UserStatus.Inativo);
        return this;
    }

    public UserTestDataBuilder AsBanned()
    {
        _faker.RuleFor(u => u.Status, UserStatus.Banido);
        return this;
    }

    public UserTestDataBuilder AsCustomer()
    {
        _faker.RuleFor(u => u.Role, UserRole.Customer);
        return this;
    }

    public UserTestDataBuilder AsAdmin()
    {
        _faker.RuleFor(u => u.Role, UserRole.Admin);
        return this;
    }

    public User Build()
    {
        return _faker.Generate();
    }

    public List<User> BuildMany(int count)
    {
        return _faker.Generate(count);
    }

    private static string GenerateValidCpf()
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
}