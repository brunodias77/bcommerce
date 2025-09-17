using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Domain.Entities;

[Table("user_tokens")]
public class UserToken
{
    [Key]
    [Column("token_id")]
    public Guid TokenId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("token_type")]
    public string TokenType { get; set; }

    [Required]
    [MaxLength(256)]
    [Column("token_value")]
    public string TokenValue { get; set; }

    [Required]
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("revoked_at")]
    public DateTime? RevokedAt { get; set; }

    [Column("version")]
    public int Version { get; set; } = 1;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}