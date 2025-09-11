using Microsoft.EntityFrameworkCore;
using IdentityService.Models;
using IdentityService.Data.Configurations;
using BuildingBlocks.Abstractions;
using IdentityService.Events;

namespace IdentityService.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<IdentityService.Events.DomainEvent> DomainEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações específicas
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
        modelBuilder.ApplyConfiguration(new DomainEventConfiguration());
        
        // Ignorar a propriedade Events do AggregateRoot
        modelBuilder.Entity<UserProfile>().Ignore(x => x.Events);

        // Configurações globais
        ConfigureGlobalSettings(modelBuilder);
    }

    private static void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Configurar convenções para todas as entidades
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configurar nomes de tabelas em snake_case
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entityType.SetTableName(ToSnakeCase(tableName));
            }

            // Configurar nomes de colunas em snake_case
            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToLower(input[i]));
        }
        return result.ToString();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Atualizar timestamps antes de salvar
        UpdateTimestamps();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Atualizar timestamps antes de salvar
        UpdateTimestamps();
        
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.MarkAsUpdated();
        }
    }
}