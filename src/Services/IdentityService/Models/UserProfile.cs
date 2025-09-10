using BuildingBlocks.Abstractions;
using BuildingBlocks.Validations;

namespace IdentityService.Models;

public class UserProfile : AggregateRoot
{
    public Guid KeycloakUserId { get; private set; }
    public PersonName Name { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public DateTime? BirthDate { get; private set; }

    private UserProfile() : base() { } // EF Core constructor
    
    private UserProfile(Guid keycloakUserId, PersonName name) : base()
    {
        KeycloakUserId = keycloakUserId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        
     //   RaiseEvent(new UserProfileCreatedEvent(Id, keycloakUserId, name.FullName));
    }

    public static UserProfile Create(Guid keycloakUserId, string firstName, string lastName, PhoneNumber? phone = null, DateTime? birthDate = null)
    {
        if (keycloakUserId == Guid.Empty)
            throw new ArgumentException("KeycloakUserId inválido", nameof(keycloakUserId));

        var personName = PersonName.Create(firstName, lastName);

        var profile = new UserProfile(keycloakUserId, personName)
        {
            Phone = phone,
            BirthDate = birthDate
        };

        return profile;
    }
    
    public void UpdateProfile(PersonName name, PhoneNumber? phone = null, DateTime? birthDate = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        BirthDate = birthDate;
        MarkAsUpdated();
        
    //    RaiseEvent(new UserProfileUpdatedEvent(Id, KeycloakUserId, name.FullName));
    }
    public override void Validate(IValidationHandler handler)
    {
        throw new NotImplementedException();
    }
}