using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("user_saved_cards")]
public class UserSavedCard : BaseEntity
{
    [Key]
    [Column("saved_card_id")]
    public Guid SavedCardId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [MaxLength(50)]
    [Column("nickname")]
    public string Nickname { get; set; }

    [Required]
    [StringLength(4)]
    [Column("last_four_digits")]
    public string LastFourDigits { get; set; }

    [Required]
    [Column("brand")]
    public CardBrand Brand { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("gateway_token")]
    public string GatewayToken { get; set; }

    [Required]
    [Column("expiry_date")]
    public DateTime ExpiryDate { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
