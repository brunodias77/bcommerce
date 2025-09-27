using FluentAssertions;
using UserService.Domain.Common;
using UserService.Domain.Enums;
using UserService.UnitTest.Common;

namespace UserService.UnitTest.Domain;

public class UserExtensionsTests : TestBase
{
    [Fact]
    public void EstaAtivo_QuandoUsuarioAtivoENaoExcluido_DeveRetornarVerdadeiro()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithStatus(UserStatus.Ativo)
            .Build();
        user.DeletedAt = null;

        // Act
        var result = user.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EstaAtivo_QuandoUsuarioInativo_DeveRetornarFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithStatus(UserStatus.Inativo)
            .Build();
        user.DeletedAt = null;

        // Act
        var result = user.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EstaAtivo_QuandoUsuarioBanido_DeveRetornarFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithStatus(UserStatus.Banido)
            .Build();
        user.DeletedAt = null;

        // Act
        var result = user.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EstaAtivo_QuandoUsuarioAtivoMasExcluido_DeveRetornarFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithStatus(UserStatus.Ativo)
            .AsDeleted()
            .Build();

        // Act
        var result = user.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContaEstaBloqueada_QuandoContaNaoBloqueada_DeveRetornarFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        user.AccountLockedUntil = null;

        // Act
        var result = user.IsAccountLocked();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ContaEstaBloqueada_QuandoContaBloqueadaNoFuturo_DeveRetornarVerdadeiro()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(30);

        // Act
        var result = user.IsAccountLocked();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContaEstaBloqueada_QuandoBloqueioContaExpirado_DeveRetornarFalso()
    {
        // Arrange
        var user = new UserTestDataBuilder().Build();
        user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = user.IsAccountLocked();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ObterNomeCompleto_QuandoAmbosNomesPreenchidos_DeveRetornarNomesConcatenados()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithFirstName("João")
            .WithLastName("Silva")
            .Build();

        // Act
        var result = user.GetFullName();

        // Assert
        result.Should().Be("João Silva");
    }

    [Fact]
    public void ObterNomeCompleto_QuandoApenasPrimeiroNome_DeveRetornarPrimeiroNomeLimpo()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithFirstName("João")
            .WithLastName("")
            .Build();

        // Act
        var result = user.GetFullName();

        // Assert
        result.Should().Be("João");
    }

    [Fact]
    public void ObterNomeCompleto_QuandoApenasUltimoNome_DeveRetornarUltimoNomeLimpo()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithFirstName("")
            .WithLastName("Silva")
            .Build();

        // Act
        var result = user.GetFullName();

        // Assert
        result.Should().Be("Silva");
    }

    [Fact]
    public void ObterNomeCompleto_QuandoAmbosNomesVazios_DeveRetornarStringVazia()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithFirstName("")
            .WithLastName("")
            .Build();

        // Act
        var result = user.GetFullName();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ObterNomeCompleto_QuandoNomesComEspacosExtras_DeveRetornarResultadoLimpo()
    {
        // Arrange
        var user = new UserTestDataBuilder()
            .WithFirstName(" João ")
            .WithLastName(" Silva ")
            .Build();

        // Act
        var result = user.GetFullName();

        // Assert
        result.Should().Be(" João   Silva ".Trim());
    }
}