using Directo.Wari.TarifaEngine.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configuración EF Core para OutboxMessage en PostgreSQL.
    /// Tabla utilizada por el Outbox Pattern para sincronización dual-database.
    /// </summary>
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(m => m.EntityType)
                .HasColumnName("entity_type")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(m => m.EntityId)
                .HasColumnName("entity_id")
                .IsRequired();

            builder.Property(m => m.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(m => m.Payload)
                .HasColumnName("payload")
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(m => m.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(m => m.ProcessedAt)
                .HasColumnName("processed_at");

            builder.Property(m => m.Error)
                .HasColumnName("error")
                .HasMaxLength(2000);

            builder.Property(m => m.RetryCount)
                .HasColumnName("retry_count")
                .HasDefaultValue(0);

            // Índice para consultar mensajes pendientes eficientemente
            builder.HasIndex(m => new { m.ProcessedAt, m.RetryCount })
                .HasDatabaseName("ix_outbox_messages_pending");

            // Índice para consultar por tipo de entidad
            builder.HasIndex(m => m.EntityType)
                .HasDatabaseName("ix_outbox_messages_entity_type");
        }
    }
}
