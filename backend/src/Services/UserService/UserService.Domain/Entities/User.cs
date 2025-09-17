using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

 [Table("users")]
    public class User : BaseEntity
    {
        [Key]
        [Column("user_id")]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Column("keycloak_id")]
        public Guid? KeycloakId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(155)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; }

        [Column("email_verified_at")]
        public DateTime? EmailVerifiedAt { get; set; }

        [MaxLength(20)]
        [Column("phone")]
        public string Phone { get; set; }

        [MaxLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [StringLength(11)]
        [Column("cpf")]
        public string Cpf { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("newsletter_opt_in")]
        public bool NewsletterOptIn { get; set; } = false;

        [Required]
        [Column("status")]
        public UserStatus Status { get; set; } = UserStatus.Ativo;

        [Required]
        [Column("role")]
        public UserRole Role { get; set; } = UserRole.Customer;

        [Column("failed_login_attempts")]
        public short FailedLoginAttempts { get; set; } = 0;

        [Column("account_locked_until")]
        public DateTime? AccountLockedUntil { get; set; }

        // Navigation properties
        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<UserSavedCard> SavedCards { get; set; } = new List<UserSavedCard>();
        public virtual ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();
        public virtual ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
        public virtual ICollection<RevokedJwtToken> RevokedTokens { get; set; } = new List<RevokedJwtToken>();
    }