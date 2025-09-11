using BuildingBlocks.Abstractions;
using IdentityService.dtos;

namespace IdentityService.Commands.Register;

public class RegisterUserCommand : IRequest<RegisterUserResponse>
{
    public Guid KeycloakUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PhoneCountryCode { get; set; }
    public DateTime? BirthDate { get; set; }
}