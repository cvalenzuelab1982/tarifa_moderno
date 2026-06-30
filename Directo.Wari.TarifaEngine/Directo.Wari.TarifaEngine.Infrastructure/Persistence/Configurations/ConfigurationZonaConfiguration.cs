using Directo.Wari.TarifaEngine.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Directo.Wari.TarifaEngine.Infrastructure.Persistence.Configurations
{
    public class ConfigurationZonaConfiguration : IEntityTypeConfiguration<ConfiguracionZona>
    {
        public void Configure(EntityTypeBuilder<ConfiguracionZona> builder)
        {
            builder.ToTable("configuracion_zonas");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("idzona_config")
                .UseIdentityColumn();

            builder.Property(e => e.IdZona)
                .HasColumnName("idzona");

            builder.Property(e => e.Propiedad)
                .HasColumnName("propiedad")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Valor)
                .HasColumnName("valor")
                .HasDefaultValue(true);

            // Auditoría moderna
            builder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp");
            builder.Property(x => x.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp");
            builder.Property(x => x.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(100);
            builder.Property(x => x.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("timestamp");
            builder.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            //TODO: solo tráeme los registros que NO estén eliminados
            // Soft delete filter
            builder.HasQueryFilter(x => !x.IsDeleted);

            // Ignorar domain events
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
