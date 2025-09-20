using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Application.Dtos.Keycloak;
using UserService.Application.Services.Interfaces;

namespace UserService.Application.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public KeycloakService(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var tokenRequest = new KeycloakTokenRequest
            {
                ClientId = _settings.FrontendClientId,
                Username = request.Email,
                Password = request.Password
            };

            var content = CreateFormContent(tokenRequest);
            var url = $"{_settings.Url}/realms/{_settings.Realm}/protocol/openid-connect/token";

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<KeycloakErrorResponse>(responseContent, _jsonOptions);
                throw new UnauthorizedAccessException($"Login failed: {error?.ErrorDescription ?? "Invalid credentials"}");
            }

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
            return loginResponse ?? throw new InvalidOperationException("Failed to deserialize login response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            throw;
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _settings.FrontendClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            var url = $"{_settings.Url}/realms/{_settings.Realm}/protocol/openid-connect/token";
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<KeycloakErrorResponse>(responseContent, _jsonOptions);
                throw new UnauthorizedAccessException($"Token refresh failed: {error?.ErrorDescription}");
            }

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
            return loginResponse ?? throw new InvalidOperationException("Failed to deserialize refresh response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    public async Task LogoutAsync(string refreshToken)
    {
        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _settings.FrontendClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            var url = $"{_settings.Url}/realms/{_settings.Realm}/protocol/openid-connect/logout";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Logout request failed with status {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw;
        }
    }

    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            
            var keycloakUser = new KeycloakUserRepresentation
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Enabled = request.Enabled,
                EmailVerified = request.EmailVerified,
                Credentials = new List<KeycloakCredential>
                {
                    new()
                    {
                        Type = "password",
                        Value = request.Password,
                        Temporary = false
                    }
                }
            };

            var json = JsonSerializer.Serialize(keycloakUser, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            
            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var location = response.Headers.Location?.ToString();
                var userId = location?.Split('/').LastOrDefault();
                
                if (!string.IsNullOrEmpty(userId) && request.Roles.Any())
                {
                    await AssignRolesToUserAsync(userId, request.Roles);
                }

                return userId ?? string.Empty;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create user {Username}: {Error}", request.Username, errorContent);
            throw new InvalidOperationException($"Failed to create user: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", request.Username);
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                
                throw new InvalidOperationException($"Failed to get user: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var keycloakUser = JsonSerializer.Deserialize<KeycloakUserRepresentation>(content, _jsonOptions);
            
            if (keycloakUser == null) return null;

            var roles = await GetUserRolesAsync(userId);
            
            return MapToUserResponse(keycloakUser, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users?email={Uri.EscapeDataString(email)}&exact=true";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to search user: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(content, _jsonOptions);
            
            var user = users?.FirstOrDefault();
            if (user == null) return null;

            var roles = await GetUserRolesAsync(user.Id!);
            
            return MapToUserResponse(user, roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            throw;
        }
    }

    public async Task<UsersPagedResponse> GetUsersAsync(int page = 0, int size = 20, string? search = null)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var queryParams = new List<string>
            {
                $"first={page * size}",
                $"max={size}"
            };

            if (!string.IsNullOrEmpty(search))
            {
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            }

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to get users: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var keycloakUsers = JsonSerializer.Deserialize<List<KeycloakUserRepresentation>>(content, _jsonOptions) ?? new();

            var users = new List<UserResponse>();
            foreach (var keycloakUser in keycloakUsers)
            {
                var roles = await GetUserRolesAsync(keycloakUser.Id!);
                users.Add(MapToUserResponse(keycloakUser, roles));
            }

            // Get total count
            var countUrl = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/count";
            var countResponse = await _httpClient.GetAsync(countUrl);
            var totalCount = 0;
            
            if (countResponse.IsSuccessStatusCode)
            {
                var countContent = await countResponse.Content.ReadAsStringAsync();
                int.TryParse(countContent, out totalCount);
            }

            return new UsersPagedResponse
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = size,
                TotalPages = (int)Math.Ceiling((double)totalCount / size)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var updateData = new Dictionary<string, object>();
            
            if (request.FirstName != null) updateData["firstName"] = request.FirstName;
            if (request.LastName != null) updateData["lastName"] = request.LastName;
            if (request.Email != null) updateData["email"] = request.Email;
            if (request.Enabled.HasValue) updateData["enabled"] = request.Enabled.Value;

            var json = JsonSerializer.Serialize(updateData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}";
            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode && request.Roles != null)
            {
                // Update roles
                var currentRoles = await GetUserRolesAsync(userId);
                var rolesToAdd = request.Roles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(request.Roles).ToList();

                if (rolesToAdd.Any())
                    await AssignRolesToUserAsync(userId, rolesToAdd);
                
                if (rolesToRemove.Any())
                    await RemoveRolesFromUserAsync(userId, rolesToRemove);
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}";
            var response = await _httpClient.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> EnableUserAsync(string userId, bool enabled)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var updateData = new { enabled };
            var json = JsonSerializer.Serialize(updateData, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}";
            var response = await _httpClient.PutAsync(url, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling/disabling user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var credential = new KeycloakCredential
            {
                Type = "password",
                Value = request.NewPassword,
                Temporary = false
            };

            var json = JsonSerializer.Serialize(credential, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}/reset-password";
            var response = await _httpClient.PutAsync(url, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await GetUserByEmailAsync(request.Email);
            if (user == null) return false;

            // Send reset password email
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var actions = new[] { "UPDATE_PASSWORD" };
            var json = JsonSerializer.Serialize(actions, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{user.Id}/execute-actions-email";
            var response = await _httpClient.PutAsync(url, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {Email}", request.Email);
            throw;
        }
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}/role-mappings/realm";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var content = await response.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<List<KeycloakRoleRepresentation>>(content, _jsonOptions);
            
            return roles?.Select(r => r.Name).ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> AssignRolesToUserAsync(string userId, List<string> roles)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Get role representations
            var roleRepresentations = new List<KeycloakRoleRepresentation>();
            foreach (var roleName in roles)
            {
                var roleUrl = $"{_settings.Url}/admin/realms/{_settings.Realm}/roles/{roleName}";
                var roleResponse = await _httpClient.GetAsync(roleUrl);
                
                if (roleResponse.IsSuccessStatusCode)
                {
                    var roleContent = await roleResponse.Content.ReadAsStringAsync();
                    var role = JsonSerializer.Deserialize<KeycloakRoleRepresentation>(roleContent, _jsonOptions);
                    if (role != null)
                        roleRepresentations.Add(role);
                }
            }

            if (!roleRepresentations.Any()) return true;

            var json = JsonSerializer.Serialize(roleRepresentations, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}/role-mappings/realm";
            var response = await _httpClient.PostAsync(url, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RemoveRolesFromUserAsync(string userId, List<string> roles)
    {
        try
        {
            var adminToken = await GetAdminTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Get role representations
            var roleRepresentations = new List<KeycloakRoleRepresentation>();
            foreach (var roleName in roles)
            {
                var roleUrl = $"{_settings.Url}/admin/realms/{_settings.Realm}/roles/{roleName}";
                var roleResponse = await _httpClient.GetAsync(roleUrl);
                
                if (roleResponse.IsSuccessStatusCode)
                {
                    var roleContent = await roleResponse.Content.ReadAsStringAsync();
                    var role = JsonSerializer.Deserialize<KeycloakRoleRepresentation>(roleContent, _jsonOptions);
                    if (role != null)
                        roleRepresentations.Add(role);
                }
            }

            if (!roleRepresentations.Any()) return true;

            var json = JsonSerializer.Serialize(roleRepresentations, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{_settings.Url}/admin/realms/{_settings.Realm}/users/{userId}/role-mappings/realm";
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing roles from user {UserId}", userId);
            throw;
        }
    }

    private async Task<string> GetAdminTokenAsync()
    {
        try
        {
            var tokenRequest = new KeycloakAdminTokenRequest
            {
                Username = _settings.AdminUsername,
                Password = _settings.AdminPassword
            };

            var content = CreateFormContent(tokenRequest);
            var url = $"{_settings.Url}/realms/master/protocol/openid-connect/token";

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<KeycloakErrorResponse>(responseContent, _jsonOptions);
                throw new UnauthorizedAccessException($"Admin token request failed: {error?.ErrorDescription}");
            }

            var tokenResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
            return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Failed to get admin token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin token");
            throw;
        }
    }

    private static FormUrlEncodedContent CreateFormContent(object obj)
    {
        var properties = obj.GetType().GetProperties();
        var keyValuePairs = new List<KeyValuePair<string, string>>();

        foreach (var property in properties)
        {
            var jsonPropertyName = property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
                .Cast<JsonPropertyNameAttribute>()
                .FirstOrDefault()?.Name ?? property.Name.ToLowerInvariant();

            var value = property.GetValue(obj)?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                keyValuePairs.Add(new KeyValuePair<string, string>(jsonPropertyName, value));
            }
        }

        return new FormUrlEncodedContent(keyValuePairs);
    }

    private static UserResponse MapToUserResponse(KeycloakUserRepresentation keycloakUser, List<string> roles)
    {
        return new UserResponse
        {
            Id = keycloakUser.Id ?? string.Empty,
            Username = keycloakUser.Username,
            Email = keycloakUser.Email,
            FirstName = keycloakUser.FirstName,
            LastName = keycloakUser.LastName,
            Enabled = keycloakUser.Enabled,
            EmailVerified = keycloakUser.EmailVerified,
            CreatedTimestamp = keycloakUser.CreatedTimestamp ?? 0,
            Roles = roles,
            Attributes = keycloakUser.Attributes ?? new Dictionary<string, List<string>>()
        };
    }
}