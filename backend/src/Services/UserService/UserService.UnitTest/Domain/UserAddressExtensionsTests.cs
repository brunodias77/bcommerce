using FluentAssertions;
using UserService.Domain.Common;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.UnitTest.Common;

namespace UserService.UnitTest.Domain;

public class UserAddressExtensionsTests : TestBase
{
    [Fact]
    public void ObterEnderecoFormatado_QuandoTodosCamposPreenchidos_DeveRetornarEnderecoCompletoFormatado()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Rua das Flores",
            StreetNumber = "123",
            Complement = "Apto 45",
            Neighborhood = "Centro",
            City = "São Paulo",
            StateCode = "SP",
            PostalCode = "01234567"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Rua das Flores, 123, Apto 45 - Centro, São Paulo/SP - CEP: 01234567");
    }

    [Fact]
    public void ObterEnderecoFormatado_QuandoComplementoVazio_DeveRetornarEnderecoFormatadoSemComplemento()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Avenida Paulista",
            StreetNumber = "1000",
            Complement = "",
            Neighborhood = "Bela Vista",
            City = "São Paulo",
            StateCode = "SP",
            PostalCode = "01310100"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Avenida Paulista, 1000 - Bela Vista, São Paulo/SP - CEP: 01310100");
    }

    [Fact]
    public void ObterEnderecoFormatado_QuandoComplementoNulo_DeveRetornarEnderecoFormatadoSemComplemento()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Rua Augusta",
            StreetNumber = "500",
            Complement = null!,
            Neighborhood = "Consolação",
            City = "São Paulo",
            StateCode = "SP",
            PostalCode = "01305000"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Rua Augusta, 500 - Consolação, São Paulo/SP - CEP: 01305000");
    }

    [Fact]
    public void ObterEnderecoFormatado_QuandoComplementoEspacosVazios_DeveRetornarEnderecoFormatadoSemComplemento()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Rua Oscar Freire",
            StreetNumber = "200",
            Complement = "   ",
            Neighborhood = "Jardins",
            City = "São Paulo",
            StateCode = "SP",
            PostalCode = "01426000"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Rua Oscar Freire, 200,     - Jardins, São Paulo/SP - CEP: 01426000");
    }

    [Fact]
    public void ObterEnderecoFormatado_QuandoEstadoECidadeDiferentes_DeveRetornarFormatoCorreto()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Rua das Palmeiras",
            StreetNumber = "789",
            Complement = "Casa 2",
            Neighborhood = "Copacabana",
            City = "Rio de Janeiro",
            StateCode = "RJ",
            PostalCode = "22070000"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Rua das Palmeiras, 789, Casa 2 - Copacabana, Rio de Janeiro/RJ - CEP: 22070000");
    }

    [Fact]
    public void ObterEnderecoFormatado_QuandoNumeroRuaAlfanumerico_DeveTratarCorretamente()
    {
        // Arrange
        var address = new UserAddress
        {
            Street = "Rua Beta",
            StreetNumber = "123A",
            Complement = "Loja 1",
            Neighborhood = "Vila Madalena",
            City = "São Paulo",
            StateCode = "SP",
            PostalCode = "05433000"
        };

        // Act
        var result = address.GetFormattedAddress();

        // Assert
        result.Should().Be("Rua Beta, 123A, Loja 1 - Vila Madalena, São Paulo/SP - CEP: 05433000");
    }
}