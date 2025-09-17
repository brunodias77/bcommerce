using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Domain.Common;

public static class UserExtensions
{
    public static bool IsActive(this User user)
    {
        return user.Status == UserStatus.Ativo && user.DeletedAt == null;
    }

    public static bool IsAccountLocked(this User user)
    {
        return user.AccountLockedUntil.HasValue && user.AccountLockedUntil > DateTime.UtcNow;
    }

    public static string GetFullName(this User user)
    {
        return $"{user.FirstName} {user.LastName}".Trim();
    }
}