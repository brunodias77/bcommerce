using System.Text.Json.Serialization;

namespace UserService.Application.Dtos.Keycloak;


internal record KeycloakRoleRepresentation(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description
);