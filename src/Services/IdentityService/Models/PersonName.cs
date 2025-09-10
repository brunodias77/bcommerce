using BuildingBlocks.Abstractions;
using BuildingBlocks.Validations;

namespace IdentityService.Models;

public class PersonName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";
    
    private PersonName(){}

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName?.Trim() ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName?.Trim() ?? throw new ArgumentNullException(nameof(lastName));
    }

    public static PersonName Create(string firstName, string lastName)
        => new PersonName(firstName, lastName);
    public override bool Equals(ValueObject? other)
    {
        if (other is not PersonName name) return false;
        return FirstName.Equals(name.FirstName, StringComparison.OrdinalIgnoreCase) &&
               LastName.Equals(name.LastName, StringComparison.OrdinalIgnoreCase);
        
    }

    protected override int GetCustomHashCode()
    {
        return HashCode.Combine(
            FirstName.ToLowerInvariant(),
            LastName.ToLowerInvariant()
        );
        
    }

    public override void Validate(IValidationHandler handler)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            handler.Add(new Error("FirstName", "Nome é obrigatório"));
        if (FirstName?.Length > 100)
            handler.Add(new Error("FirstName", "Nome deve ter no máximo 100 caracteres"));
        if (string.IsNullOrWhiteSpace(LastName))
            handler.Add(new Error("LastName", "Sobrenome é obrigatório"));
        if (LastName?.Length > 100)
            handler.Add(new Error("LastName", "Sobrenome deve ter no máximo 100 caracteres"));
        
    }
}