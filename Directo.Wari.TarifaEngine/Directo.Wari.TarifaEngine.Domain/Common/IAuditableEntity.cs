namespace Directo.Wari.TarifaEngine.Domain.Common
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; }
        DateTime? UpdatedAt { get; }
    }
}
