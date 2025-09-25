using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Infrastructure.Data;

namespace UserService.Application.Services;

public interface ITokenService
{
    Task<Guid?> ExtractUserIdFromRefreshTokenAsync(string refreshToken);
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt);
    Task RevokeRefreshTokensAsync(Guid userId, string? excludeToken = null);
}

public class TokenService : ITokenService
{
    private readonly UserManagementDbContext _context;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _jwtHandler;

    public TokenService(UserManagementDbContext context, ILogger<TokenService> logger)
    {
        _context = context;
        _logger = logger;
        _jwtHandler = new JwtSecurityTokenHandler();
    }

    public async Task<Guid?> ExtractUserIdFromRefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Primeiro, tenta buscar o token no banco de dados
            var tokenFromDb = await _context.UserTokens
                .Where(t => t.TokenValue == refreshToken && 
                           t.TokenType == UserTokenType.Refresh && 
                           t.RevokedAt == null &&
                           t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (tokenFromDb != null)
            {
                _logger.LogDebug("Refresh token encontrado no banco de dados para UserId: {UserId}", tokenFromDb.UserId);
                return tokenFromDb.UserId;
            }

            // Se não encontrou no banco, tenta decodificar o JWT
            if (_jwtHandler.CanReadToken(refreshToken))
            {
                var jwtToken = _jwtHandler.ReadJwtToken(refreshToken);
                
                // Busca por diferentes claims que podem conter o user_id
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == "sub" || 
                    c.Type == "user_id" || 
                    c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    _logger.LogDebug("UserId extraído do JWT: {UserId}", userId);
                    return userId;
                }

                // Se o claim contém email, busca o usuário pelo email
                var emailClaim = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == "email" || 
                    c.Type == ClaimTypes.Email);

                if (emailClaim != null)
                {
                    var user = await _context.Users
                        .Where(u => u.Email == emailClaim.Value)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        _logger.LogDebug("UserId encontrado pelo email: {UserId}", user.UserId);
                        return user.UserId;
                    }
                }
            }

            _logger.LogWarning("Não foi possível extrair UserId do refresh token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair UserId do refresh token");
            return null;
        }
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt)
    {
        try
        {
            var userToken = new UserToken
            {
                TokenId = Guid.NewGuid(),
                UserId = userId,
                TokenType = UserTokenType.Refresh,
                TokenValue = refreshToken,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                Version = 1
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token salvo para UserId: {UserId}, TokenId: {TokenId}", userId, userToken.TokenId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar refresh token para UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task RevokeRefreshTokensAsync(Guid userId, string? excludeToken = null)
    {
        try
        {
            var tokensToRevoke = await _context.UserTokens
                .Where(t => t.UserId == userId && 
                           t.TokenType == UserTokenType.Refresh && 
                           t.RevokedAt == null &&
                           (excludeToken == null || t.TokenValue != excludeToken))
                .ToListAsync();

            if (tokensToRevoke.Any())
            {
                var revokedAt = DateTime.UtcNow;
                foreach (var token in tokensToRevoke)
                {
                    token.RevokedAt = revokedAt;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Revogados {Count} refresh tokens para UserId: {UserId}", tokensToRevoke.Count, userId);
            }
            else
            {
                _logger.LogDebug("Nenhum refresh token encontrado para revogar para UserId: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar refresh tokens para UserId: {UserId}", userId);
            throw;
        }
    }
}