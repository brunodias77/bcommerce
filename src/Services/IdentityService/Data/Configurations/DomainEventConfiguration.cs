using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BuildingBlocks.Abstractions;
using System.Text.Json;

namespace IdentityService.Data.Configurations;

public class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        // Configuração da tabela
        builder.ToTable("domain_events");
        
        // Chave primária usando EventId
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .ValueGeneratedNever(); // O EventId é gerado pela classe DomainEvent

        // OccurredAt - timestamp do evento
        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Discriminator para diferentes tipos de eventos
        builder.HasDiscriminator<string>("event_type");

        // Configuração adicional para armazenar dados do evento como JSON
        builder.Property<string>("EventData")
            .HasColumnName("event_data")
            .HasColumnType("jsonb")
            .IsRequired();

        // Campos adicionais para controle
        builder.Property<Guid?>("AggregateId")
            .HasColumnName("aggregate_id")
            .IsRequired(false);
            
        builder.Property<DateTime?>("ProcessedAt")
            .HasColumnName("processed_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        // Índices para performance
        builder.HasIndex("AggregateId")
            .HasDatabaseName("idx_domain_events_aggregate_id");
            
        builder.HasIndex("event_type")
            .HasDatabaseName("idx_domain_events_event_type");
            
        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("idx_domain_events_occurred_at");
            
        builder.HasIndex("ProcessedAt")
            .HasDatabaseName("idx_domain_events_processed_at")
            .HasFilter("processed_at IS NULL");
    }
}