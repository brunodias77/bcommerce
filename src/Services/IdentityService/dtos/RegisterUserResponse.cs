namespace IdentityService.dtos;

public record RegisterUserResponse(
    Guid Id,
    Guid KeycloakUserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? PhoneCountryCode,
    DateTime? BirthDate,
    DateTime CreatedAt
);