using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

[Table("user_addresses")]
public class UserAddress : BaseEntity
{
    [Key]
    [Column("address_id")]
    public Guid AddressId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("type")]
    public AddressType Type { get; set; }

    [Required]
    [StringLength(8)]
    [Column("postal_code")]
    public string PostalCode { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("street")]
    public string Street { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("street_number")]
    public string StreetNumber { get; set; }

    [MaxLength(100)]
    [Column("complement")]
    public string Complement { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("neighborhood")]
    public string Neighborhood { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("city")]
    public string City { get; set; }

    [Required]
    [StringLength(2)]
    [Column("state_code")]
    public string StateCode { get; set; }

    [Required]
    [StringLength(2)]
    [Column("country_code")]
    public string CountryCode { get; set; } = "BR";

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}