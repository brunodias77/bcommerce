namespace UserService.Application.Dtos.Keycloak;

public class CreateUserRequestKeycloak
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public List<string> Roles { get; set; } = new();
}