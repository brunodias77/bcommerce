using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserService.Domain.Common;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data;


    public class UserManagementDbContext : DbContext
    {
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserSavedCard> UserSavedCards { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserConsent> UserConsents { get; set; }
        public DbSet<RevokedJwtToken> RevokedJwtTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração para User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("\"deleted_at\" IS NULL");
                entity.HasIndex(e => e.Status).HasFilter("\"deleted_at\" IS NULL");
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.KeycloakId).IsUnique();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                
                entity.Property(e => e.Role)
                    .HasConversion<string>();

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configuração para UserAddress
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasKey(e => e.AddressId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Type })
                    .IsUnique()
                    .HasFilter("\"is_default\" = true AND \"deleted_at\" IS NULL");
                
                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Addresses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configuração para UserSavedCard
            modelBuilder.Entity<UserSavedCard>(entity =>
            {
                entity.HasKey(e => e.SavedCardId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.GatewayToken).IsUnique();
                entity.HasIndex(e => e.UserId)
                    .IsUnique()
                    .HasFilter("\"is_default\" = true AND \"deleted_at\" IS NULL");
                
                entity.Property(e => e.Brand)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.SavedCards)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configuração para UserToken
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TokenType);
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.TokenValue).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Tokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração para UserConsent
            modelBuilder.Entity<UserConsent>(entity =>
            {
                entity.HasKey(e => e.ConsentId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Type }).IsUnique();
                
                entity.Property(e => e.Type)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Consents)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Soft delete filter
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });

            // Configuração para RevokedJwtToken
            modelBuilder.Entity<RevokedJwtToken>(entity =>
            {
                entity.HasKey(e => e.Jti);
                entity.HasIndex(e => e.ExpiresAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RevokedTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Timestamps são gerenciados manualmente pelo método UpdateTimestamps()
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        entry.Entity.Version++;
                        break;
                }
            }
        }
    }