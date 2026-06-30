namespace Directo.Wari.TarifaEngine.Domain.Common
{
    /// <summary>
    /// Interfaz marcadora para eventos de dominio.
    /// Se mantiene libre de dependencias externas (principio DDD).
    /// La integración con MediatR se realiza en la capa de Application.
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }

    }
}
