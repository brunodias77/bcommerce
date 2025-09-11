using BuildingBlocks.Abstractions;
using BuildingBlocks.Validations;
using System.Text.RegularExpressions;

namespace IdentityService.Models;

public class PhoneNumber : ValueObject
{
    public string Number { get; }
    public string CountryCode { get; }
    public string FormattedNumber => $"{CountryCode} {Number}";
    
    private PhoneNumber() { } // EF Core constructor

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode?.Trim() ?? throw new ArgumentNullException(nameof(countryCode));
        Number = number?.Trim() ?? throw new ArgumentNullException(nameof(number));
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Número de telefone não pode ser vazio", nameof(phoneNumber));

        // Remove espaços e caracteres especiais
        var cleanNumber = Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        // Assume formato brasileiro por padrão
        if (cleanNumber.StartsWith("+55"))
        {
            return new PhoneNumber("+55", cleanNumber.Substring(3));
        }
        
        if (cleanNumber.StartsWith("55") && cleanNumber.Length >= 12)
        {
            return new PhoneNumber("+55", cleanNumber.Substring(2));
        }
        
        // Se não tem código do país, assume Brasil
        return new PhoneNumber("+55", cleanNumber);
    }

    public override bool Equals(ValueObject? other)
    {
        if (other is not PhoneNumber phone) return false;
        return Number.Equals(phone.Number, StringComparison.OrdinalIgnoreCase) &&
               CountryCode.Equals(phone.CountryCode, StringComparison.OrdinalIgnoreCase);
    }

    protected override int GetCustomHashCode()
    {
        return HashCode.Combine(
            CountryCode.ToLowerInvariant(),
            Number.ToLowerInvariant()
        );
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(CountryCode))
            handler.Add(new Error("CountryCode", "Código do país é obrigatório"));
            
        if (string.IsNullOrWhiteSpace(Number))
            handler.Add(new Error("Number", "Número de telefone é obrigatório"));
            
        if (Number?.Length < 8)
            handler.Add(new Error("Number", "Número de telefone deve ter pelo menos 8 dígitos"));
            
        if (Number?.Length > 15)
            handler.Add(new Error("Number", "Número de telefone deve ter no máximo 15 dígitos"));
    }
}