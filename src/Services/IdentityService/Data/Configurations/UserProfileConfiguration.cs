using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IdentityService.Models;

namespace IdentityService.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        // Configuração da tabela
        builder.ToTable("user_profiles");
        
        // Chave primária
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // KeycloakUserId
        builder.Property(x => x.KeycloakUserId)
            .HasColumnName("keycloak_user_id")
            .IsRequired();

        // Timestamps herdados da Entity
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);
            
        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        // BirthDate
        builder.Property(x => x.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired(false);

        // Índices
        builder.HasIndex(x => x.KeycloakUserId)
            .IsUnique()
            .HasDatabaseName("idx_user_profiles_keycloak_user_id");

        // Configuração do PersonName como value object
        builder.OwnsOne(x => x.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();
                
            nameBuilder.Property(n => n.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();
                
            nameBuilder.WithOwner().HasForeignKey("Id");
        });

        // Configuração do PhoneNumber como value object
        builder.OwnsOne(x => x.Phone, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.Number)
                .HasColumnName("phone_number")
                .HasMaxLength(15)
                .IsRequired(false);
                
            phoneBuilder.Property(p => p.CountryCode)
                .HasColumnName("phone_country_code")
                .HasMaxLength(5)
                .IsRequired(false);
                
            phoneBuilder.WithOwner().HasForeignKey("Id");
        });

        // Query filter para soft delete
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}