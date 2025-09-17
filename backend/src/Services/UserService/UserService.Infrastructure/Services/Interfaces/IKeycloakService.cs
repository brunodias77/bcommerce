using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Interface para integração com o Keycloak
    /// Responsável por operações de autenticação, autorização e gerenciamento de usuários
    /// </summary>
    public interface IKeycloakService
    {
        /// <summary>
        /// Cria um novo usuário no Keycloak
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <param name="password">Senha do usuário</param>
        /// <param name="firstName">Primeiro nome</param>
        /// <param name="lastName">Sobrenome</param>
        /// <param name="enabled">Se o usuário está habilitado</param>
        /// <returns>ID do usuário criado no Keycloak</returns>
        Task<string> CreateUserAsync(string email, string password, string firstName, string lastName, bool enabled = true);

        /// <summary>
        /// Atualiza os dados de um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <param name="email">Novo email</param>
        /// <param name="firstName">Novo primeiro nome</param>
        /// <param name="lastName">Novo sobrenome</param>
        /// <param name="enabled">Status de habilitação</param>
        /// <returns>True se atualizado com sucesso</returns>
        Task<bool> UpdateUserAsync(string keycloakUserId, string email, string firstName, string lastName, bool? enabled = null);

        /// <summary>
        /// Deleta um usuário do Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <returns>True se deletado com sucesso</returns>
        Task<bool> DeleteUserAsync(string keycloakUserId);

        /// <summary>
        /// Busca um usuário no Keycloak por email
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <returns>Dados do usuário ou null se não encontrado</returns>
        Task<KeycloakUserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Busca um usuário no Keycloak por ID
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <returns>Dados do usuário ou null se não encontrado</returns>
        Task<KeycloakUserDto?> GetUserByIdAsync(string keycloakUserId);

        /// <summary>
        /// Altera a senha de um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <param name="newPassword">Nova senha</param>
        /// <param name="temporary">Se a senha é temporária</param>
        /// <returns>True se alterada com sucesso</returns>
        Task<bool> ChangePasswordAsync(string keycloakUserId, string newPassword, bool temporary = false);

        /// <summary>
        /// Habilita ou desabilita um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <param name="enabled">Status de habilitação</param>
        /// <returns>True se alterado com sucesso</returns>
        Task<bool> SetUserEnabledAsync(string keycloakUserId, bool enabled);

        /// <summary>
        /// Obtém os roles/papéis de um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <returns>Lista de roles do usuário</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(string keycloakUserId);

        /// <summary>
        /// Atribui roles/papéis a um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <param name="roles">Lista de roles para atribuir</param>
        /// <returns>True se atribuídos com sucesso</returns>
        Task<bool> AssignRolesToUserAsync(string keycloakUserId, IEnumerable<string> roles);

        /// <summary>
        /// Remove roles/papéis de um usuário no Keycloak
        /// </summary>
        /// <param name="keycloakUserId">ID do usuário no Keycloak</param>
        /// <param name="roles">Lista de roles para remover</param>
        /// <returns>True se removidos com sucesso</returns>
        Task<bool> RemoveRolesFromUserAsync(string keycloakUserId, IEnumerable<string> roles);

        /// <summary>
        /// Valida se um token JWT é válido no Keycloak
        /// </summary>
        /// <param name="token">Token JWT para validar</param>
        /// <returns>True se o token é válido</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Obtém informações do usuário a partir de um token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Informações do usuário ou null se token inválido</returns>
        Task<KeycloakUserDto?> GetUserFromTokenAsync(string token);

        /// <summary>
        /// Revoga/invalida um token JWT no Keycloak
        /// </summary>
        /// <param name="token">Token JWT para revogar</param>
        /// <returns>True se revogado com sucesso</returns>
        Task<bool> RevokeTokenAsync(string token);
    }

    /// <summary>
    /// DTO para representar dados do usuário do Keycloak
    /// </summary>
    public class KeycloakUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }
}