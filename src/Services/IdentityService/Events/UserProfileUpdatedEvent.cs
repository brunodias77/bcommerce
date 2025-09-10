using BuildingBlocks.Abstractions;

namespace IdentityService.Events;

public record UserProfileUpdatedEvent(Guid ProfileId, Guid KeycloakUserId, string FullName) : DomainEvent;
