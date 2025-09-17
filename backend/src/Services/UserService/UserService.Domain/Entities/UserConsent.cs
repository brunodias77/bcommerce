using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("user_consents")]
public class UserConsent : BaseEntity
{
    [Key]
    [Column("consent_id")]
    public Guid ConsentId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("type")]
    public ConsentType Type { get; set; }

    [MaxLength(30)]
    [Column("terms_version")]
    public string TermsVersion { get; set; }

    [Required]
    [Column("is_granted")]
    public bool IsGranted { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}