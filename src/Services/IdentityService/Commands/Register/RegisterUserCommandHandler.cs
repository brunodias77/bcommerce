using BuildingBlocks.Abstractions;
using IdentityService.dtos;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Commands.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IdentityDbContext _context;

    public RegisterUserCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<RegisterUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe um perfil para este usuário do Keycloak
        var existingProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.KeycloakUserId == request.KeycloakUserId, cancellationToken);

        if (existingProfile != null)
        {
            throw new InvalidOperationException($"Já existe um perfil para o usuário Keycloak {request.KeycloakUserId}");
        }

        // Criar PhoneNumber se fornecido
        PhoneNumber? phone = null;
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var fullPhoneNumber = !string.IsNullOrEmpty(request.PhoneCountryCode) 
                ? $"{request.PhoneCountryCode}{request.PhoneNumber}"
                : request.PhoneNumber;
            phone = PhoneNumber.Create(fullPhoneNumber);
        }

        // Criar o perfil do usuário
        var userProfile = UserProfile.Create(
            request.KeycloakUserId,
            request.FirstName,
            request.LastName,
            phone,
            request.BirthDate
        );

        // Adicionar ao contexto e salvar
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync(cancellationToken);

        // Retornar o response
        return new RegisterUserResponse(
            userProfile.Id,
            userProfile.KeycloakUserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.PhoneCountryCode,
            userProfile.BirthDate,
            userProfile.CreatedAt
        );
    }
}