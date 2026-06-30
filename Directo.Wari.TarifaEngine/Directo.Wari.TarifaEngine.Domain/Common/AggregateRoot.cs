namespace Directo.Wari.TarifaEngine.Domain.Common
{
    /// <summary>
    /// Clase base para Aggregate Roots.
    /// Los Aggregate Roots son las únicas entidades accesibles desde fuera del aggregate.
    /// </summary>
    public abstract class AggregateRoot<IId> : BaseEntity, IAuditableEntity
    {
        // Propiedades de auditoría
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public string? CreatedBy { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public string? UpdatedBy { get; protected set; }
        public DateTime? DeletedAt { get; protected set; }
        public string? DeletedBy { get; protected set; }
        public bool IsDeleted { get; protected set; }

        /// <summary>
        /// Marca la entidad como eliminada (soft delete).
        /// </summary>
        public virtual void SoftDelete(string? deletedBy = null)
        {
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }
    }
}
