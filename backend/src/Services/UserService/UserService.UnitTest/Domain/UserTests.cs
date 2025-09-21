using FluentAssertions;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.UnitTest.Common;

namespace UserService.UnitTest.Domain;

public class UserTests : TestBase
{
    [Fact]
    public void Usuario_QuandoCriado_DeveConterValoresPadrao()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.UserId.Should().NotBeEmpty();
        user.Status.Should().Be(UserStatus.Ativo);
        user.Role.Should().Be(UserRole.Customer);
        user.NewsletterOptIn.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.DeletedAt.Should().BeNull();
        user.Version.Should().Be(1);
        user.Addresses.Should().NotBeNull().And.BeEmpty();
        user.SavedCards.Should().NotBeNull().And.BeEmpty();
        user.Tokens.Should().NotBeNull().And.BeEmpty();
        user.Consents.Should().NotBeNull().And.BeEmpty();
        user.RevokedTokens.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Usuario_QuandoCriadoComDadosValidos_DeveDefinirPropriedadesCorretamente()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var keycloakId = Guid.NewGuid();
        var firstName = "João";
        var lastName = "Silva";
        var email = "joao.silva@email.com";
        var phone = "(11) 99999-9999";
        var cpf = GenerateValidCpf();
        var dateOfBirth = new DateTime(1990, 1, 1);
        var passwordHash = "hashedPassword123";

        // Act
        var user = new User
        {
            UserId = userId,
            KeycloakId = keycloakId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Cpf = cpf,
            DateOfBirth = dateOfBirth,
            PasswordHash = passwordHash,
            NewsletterOptIn = true,
            Status = UserStatus.Ativo,
            Role = UserRole.Admin
        };

        // Assert
        user.UserId.Should().Be(userId);
        user.KeycloakId.Should().Be(keycloakId);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.Phone.Should().Be(phone);
        user.Cpf.Should().Be(cpf);
        user.DateOfBirth.Should().Be(dateOfBirth);
        user.PasswordHash.Should().Be(passwordHash);
        user.NewsletterOptIn.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Ativo);
        user.Role.Should().Be(UserRole.Admin);
    }

    [Theory]
    [InlineData(UserStatus.Ativo)]
    [InlineData(UserStatus.Inativo)]
    [InlineData(UserStatus.Banido)]
    public void Usuario_QuandoStatusDefinido_DeveAceitarStatusValidos(UserStatus status)
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();

        // Act
        user.Status = status;

        // Assert
        user.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(UserRole.Customer)]
    [InlineData(UserRole.Admin)]
    public void Usuario_QuandoPerfilDefinido_DeveAceitarPerfisValidos(UserRole role)
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();

        // Act
        user.Role = role;

        // Assert
        user.Role.Should().Be(role);
    }

    [Fact]
    public void Usuario_QuandoTentativasLoginFalhadasIncrementadas_DeveAtualizarCorretamente()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        var initialAttempts = user.FailedLoginAttempts;

        // Act
        user.FailedLoginAttempts++;

        // Assert
        user.FailedLoginAttempts.Should().Be((short)(initialAttempts + 1));
    }

    [Fact]
    public void Usuario_QuandoContaBloqueada_DeveDefinirTempoBloqueio()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        var lockoutTime = DateTime.UtcNow.AddMinutes(30);

        // Act
        user.AccountLockedUntil = lockoutTime;

        // Assert
        user.AccountLockedUntil.Should().Be(lockoutTime);
    }

    [Fact]
    public void Usuario_QuandoEmailVerificado_DeveDefinirTempoVerificacao()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        var verificationTime = DateTime.UtcNow;

        // Act
        user.EmailVerifiedAt = verificationTime;

        // Assert
        user.EmailVerifiedAt.Should().Be(verificationTime);
    }

    [Fact]
    public void Usuario_QuandoCriadoComBuilder_DeveConterDadosValidos()
    {
        // Arrange & Act
        var user = new UserTestDataBuilder()
            .WithFirstName("Maria")
            .WithLastName("Santos")
            .WithEmail("maria.santos@email.com")
            .AsActive()
            .AsCustomer()
            .Build();

        // Assert
        user.FirstName.Should().Be("Maria");
        user.LastName.Should().Be("Santos");
        user.Email.Should().Be("maria.santos@email.com");
        user.Status.Should().Be(UserStatus.Ativo);
        user.Role.Should().Be(UserRole.Customer);
        user.UserId.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromDays(1));
    }

    [Fact]
    public void Usuario_QuandoMultiplosUsuariosCriados_DeveConterIdsUnicos()
    {
        // Arrange & Act
        var users = new UserTestDataBuilder().BuildMany(5);

        // Assert
        users.Should().HaveCount(5);
        var userIds = users.Select(u => u.UserId).ToList();
        userIds.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Usuario_QuandoOptInNewsletterDefinido_DeveSerVerdadeiro()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithNewsletterOptIn(true)
            .Build();

        // Act & Assert
        user.NewsletterOptIn.Should().BeTrue();
    }

    [Fact]
    public void Usuario_QuandoOptInNewsletterNaoDefinido_DeveSerFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithNewsletterOptIn(false)
            .Build();

        // Act & Assert
        user.NewsletterOptIn.Should().BeFalse();
    }

    [Fact]
    public void Usuario_QuandoCpfDefinido_DeveAceitarFormatoValido()
    {
        // Arrange
        var validCpf = GenerateValidCpf();
        var user = new UserTestDataBuilder().Build();

        // Act
        user.Cpf = validCpf;

        // Assert
        user.Cpf.Should().Be(validCpf);
        user.Cpf.Should().HaveLength(11);
        user.Cpf.Should().MatchRegex(@"^\d{11}$");
    }

    [Fact]
    public void Usuario_QuandoTelefoneDefinido_DeveAceitarFormatoValido()
    {
        // Arrange
        var validPhone = "(11) 99999-9999";
        var user = new UserTestDataBuilder().Build();

        // Act
        user.Phone = validPhone;

        // Assert
        user.Phone.Should().Be(validPhone);
    }

    [Theory]
    [InlineData("test@email.com")]
    [InlineData("user.name@domain.com.br")]
    [InlineData("test+tag@example.org")]
    public void Usuario_QuandoEmailDefinido_DeveAceitarFormatosValidos(string email)
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();

        // Act
        user.Email = email;

        // Assert
        user.Email.Should().Be(email);
    }

    [Fact]
    public void Usuario_QuandoDataNascimentoDefinida_DeveAceitarDataValida()
    {
        // Arrange
        var birthDate = new DateTime(1985, 6, 15);
        var user = new UserTestDataBuilder().Build();

        // Act
        user.DateOfBirth = birthDate;

        // Assert
        user.DateOfBirth.Should().Be(birthDate);
    }

    [Fact]
    public void Usuario_QuandoHashSenhaDefinido_DeveArmazenarCorretamente()
    {
        // Arrange
        var passwordHash = "$2a$10$N9qo8uLOickgx2ZMRZoMye";
        var user = new UserTestDataBuilder().Build();

        // Act
        user.PasswordHash = passwordHash;

        // Assert
        user.PasswordHash.Should().Be(passwordHash);
    }
}