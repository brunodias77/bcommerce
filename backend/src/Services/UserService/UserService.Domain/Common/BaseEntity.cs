using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Common;

public abstract class BaseEntity
{
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("version")]
    public int Version { get; set; } = 1;
}