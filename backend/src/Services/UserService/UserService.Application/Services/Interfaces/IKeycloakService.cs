
using UserService.Application.Dtos.Keycloak;

namespace UserService.Application.Services.Interfaces;

public interface IKeycloakService
{
    Task<string> CreateUserAsync(CreateUserKeycloak request);
    Task<UserResponseKeycloak?> GetUserByEmailAsync(string email);
    Task<UserResponseKeycloak?> GetUserByIdAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
    Task<LoginResponse> LoginAsync(LoginUserKeycloak request);
    
    Task<bool> SendEmailVerificationAsync(string userId);

    
    // Task<LoginResponse> LoginAsync(LoginRequest request);
    // Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    // Task LogoutAsync(string refreshToken);
    // Task<string> CreateUserAsync(CreateUserRequest request);
    // Task<UserResponse?> GetUserByIdAsync(string userId);
    // Task<UserResponse?> GetUserByEmailAsync(string email);
    // Task<UsersPagedResponse> GetUsersAsync(int page = 0, int size = 20, string? search = null);
    // Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);
    // Task<bool> DeleteUserAsync(string userId);
    // Task<bool> EnableUserAsync(string userId, bool enabled);
    // Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    // Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    // Task<List<string>> GetUserRolesAsync(string userId);
    // Task<bool> AssignRolesToUserAsync(string userId, List<string> roles);
    // Task<bool> RemoveRolesFromUserAsync(string userId, List<string> roles);
}