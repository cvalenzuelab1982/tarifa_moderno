using Microsoft.EntityFrameworkCore;

namespace Directo.Wari.TarifaEngine.Infrastructure.Legacy.Persistence
{
    public class LegacyDbContext : DbContext
    {
        public LegacyDbContext(DbContextOptions<LegacyDbContext> options) : base(options) { }

        //public DbSet<Servicio> Servicios => Set<Servicio>();
        //public DbSet<Destino> Destinos => Set<Destino>();
        //public DbSet<Cliente> Clientes => Set<Cliente>();
        //public DbSet<Conductor> Conductores => Set<Conductor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica configuraciones específicas para el schema legacy de SQL Server
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LegacyDbContext).Assembly);
        }

    }
}
