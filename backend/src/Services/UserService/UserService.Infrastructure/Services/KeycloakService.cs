using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure.Services.Interfaces;

namespace UserService.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de integração com Keycloak
    /// </summary>
    public class KeycloakService : IKeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KeycloakService> _logger;
        private readonly KeycloakConfiguration _config;
        private readonly JsonSerializerOptions _jsonOptions;

        public KeycloakService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<KeycloakService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = new KeycloakConfiguration();
            configuration.GetSection("Keycloak").Bind(_config);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<string> CreateUserAsync(string email, string password, string firstName, string lastName, bool enabled = true)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Não foi possível obter token de acesso do Keycloak");
                }

                var userPayload = new
                {
                    username = email,
                    email = email,
                    firstName = firstName,
                    lastName = lastName,
                    enabled = enabled,
                    credentials = new[]
                    {
                        new
                        {
                            type = "password",
                            value = password,
                            temporary = false
                        }
                    }
                };

                var json = JsonSerializer.Serialize(userPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PostAsync($"{_config.AdminUrl}/users", content);

                if (response.IsSuccessStatusCode)
                {
                    var locationHeader = response.Headers.Location?.ToString();
                    if (!string.IsNullOrEmpty(locationHeader))
                    {
                        var userId = locationHeader.Split('/').Last();
                        _logger.LogInformation("Usuário criado no Keycloak com ID: {UserId}", userId);
                        return userId;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao criar usuário no Keycloak: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new InvalidOperationException($"Erro ao criar usuário no Keycloak: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário no Keycloak para email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(string keycloakUserId, string email, string firstName, string lastName, bool? enabled = null)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                var updatePayload = new Dictionary<string, object>
                {
                    ["email"] = email,
                    ["firstName"] = firstName,
                    ["lastName"] = lastName
                };

                if (enabled.HasValue)
                {
                    updatePayload["enabled"] = enabled.Value;
                }

                var json = JsonSerializer.Serialize(updatePayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PutAsync($"{_config.AdminUrl}/users/{keycloakUserId}", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Usuário atualizado no Keycloak: {UserId}", keycloakUserId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao atualizar usuário no Keycloak: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário no Keycloak: {UserId}", keycloakUserId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string keycloakUserId)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.DeleteAsync($"{_config.AdminUrl}/users/{keycloakUserId}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Usuário deletado do Keycloak: {UserId}", keycloakUserId);
                    return true;
                }

                _logger.LogError("Erro ao deletar usuário do Keycloak: {StatusCode}", response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar usuário do Keycloak: {UserId}", keycloakUserId);
                return false;
            }
        }

        public async Task<KeycloakUserDto?> GetUserByEmailAsync(string email)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_config.AdminUrl}/users?email={Uri.EscapeDataString(email)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<KeycloakUserDto[]>(content, _jsonOptions);
                    return users?.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email no Keycloak: {Email}", email);
                return null;
            }
        }

        public async Task<KeycloakUserDto?> GetUserByIdAsync(string keycloakUserId)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_config.AdminUrl}/users/{keycloakUserId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<KeycloakUserDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por ID no Keycloak: {UserId}", keycloakUserId);
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string keycloakUserId, string newPassword, bool temporary = false)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                var passwordPayload = new
                {
                    type = "password",
                    value = newPassword,
                    temporary = temporary
                };

                var json = JsonSerializer.Serialize(passwordPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PutAsync($"{_config.AdminUrl}/users/{keycloakUserId}/reset-password", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Senha alterada no Keycloak para usuário: {UserId}", keycloakUserId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha no Keycloak: {UserId}", keycloakUserId);
                return false;
            }
        }

        public async Task<bool> SetUserEnabledAsync(string keycloakUserId, bool enabled)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                var payload = new { enabled = enabled };
                var json = JsonSerializer.Serialize(payload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PutAsync($"{_config.AdminUrl}/users/{keycloakUserId}", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status do usuário no Keycloak: {UserId}", keycloakUserId);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string keycloakUserId)
        {
            try
            {
                var accessToken = await GetAdminAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Enumerable.Empty<string>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.GetAsync($"{_config.AdminUrl}/users/{keycloakUserId}/role-mappings/realm");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var roles = JsonSerializer.Deserialize<KeycloakRole[]>(content, _jsonOptions);
                    return roles?.Select(r => r.Name) ?? Enumerable.Empty<string>();
                }

                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter roles do usuário no Keycloak: {UserId}", keycloakUserId);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> AssignRolesToUserAsync(string keycloakUserId, IEnumerable<string> roles)
        {
            // Implementação simplificada - seria necessário primeiro obter os IDs dos roles
            _logger.LogWarning("AssignRolesToUserAsync não implementado completamente");
            return false;
        }

        public async Task<bool> RemoveRolesFromUserAsync(string keycloakUserId, IEnumerable<string> roles)
        {
            // Implementação simplificada - seria necessário primeiro obter os IDs dos roles
            _logger.LogWarning("RemoveRolesFromUserAsync não implementado completamente");
            return false;
        }

        public async Task<KeycloakAuthResult?> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = _config.ClientId,
                    ["client_secret"] = _config.ClientSecret,
                    ["username"] = email,
                    ["password"] = password
                };

                var content = new FormUrlEncodedContent(payload);
                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/protocol/openid-connect/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, _jsonOptions);
                    
                    if (tokenResponse != null)
                    {
                        return new KeycloakAuthResult
                        {
                            AccessToken = tokenResponse.GetValueOrDefault("access_token")?.ToString() ?? string.Empty,
                            RefreshToken = tokenResponse.GetValueOrDefault("refresh_token")?.ToString() ?? string.Empty,
                            ExpiresIn = int.TryParse(tokenResponse.GetValueOrDefault("expires_in")?.ToString(), out var expiresIn) ? expiresIn : 0,
                            TokenType = tokenResponse.GetValueOrDefault("token_type")?.ToString() ?? "Bearer",
                            Scope = tokenResponse.GetValueOrDefault("scope")?.ToString() ?? string.Empty
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Falha na autenticação do usuário {Email}: {StatusCode} - {Error}", email, response.StatusCode, errorContent);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao autenticar usuário no Keycloak: {Email}", email);
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["token"] = token
                };

                var content = new FormUrlEncodedContent(payload);
                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/protocol/openid-connect/token/introspect", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, _jsonOptions);
                    return result?.ContainsKey("active") == true && result["active"].ToString() == "True";
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token no Keycloak");
                return false;
            }
        }

        public async Task<KeycloakUserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                // Implementação simplificada - seria necessário decodificar o JWT
                _logger.LogWarning("GetUserFromTokenAsync não implementado completamente");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário do token");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["token"] = token,
                    ["client_id"] = _config.ClientId,
                    ["client_secret"] = _config.ClientSecret
                };

                var content = new FormUrlEncodedContent(payload);
                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/protocol/openid-connect/revoke", content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao revogar token no Keycloak");
                return false;
            }
        }

        private async Task<string?> GetAdminAccessTokenAsync()
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _config.AdminClientId,
                    ["client_secret"] = _config.AdminClientSecret
                };

                var content = new FormUrlEncodedContent(payload);
                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/protocol/openid-connect/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent, _jsonOptions);
                    return tokenResponse?["access_token"]?.ToString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter token de acesso admin do Keycloak");
                return null;
            }
        }
    }

    /// <summary>
    /// Configuração do Keycloak
    /// </summary>
    public class KeycloakConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string AdminUrl { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string AdminClientId { get; set; } = string.Empty;
        public string AdminClientSecret { get; set; } = string.Empty;
    }

    /// <summary>
    /// Representação de um role do Keycloak
    /// </summary>
    public class KeycloakRole
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}