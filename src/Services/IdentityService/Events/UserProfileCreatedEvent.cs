using BuildingBlocks.Abstractions;

namespace IdentityService.Events;

public record UserProfileCreatedEvent(Guid ProfileId, Guid KeycloakUserId, string FullName) : DomainEvent;
