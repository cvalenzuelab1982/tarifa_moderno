namespace Directo.Wari.TarifaEngine.Domain.Exceptions
{
    /// <summary>
    /// Excepción base del dominio.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Se lanza cuando una entidad no es encontrada.
    /// </summary>
    public class EntityNotFoundException : DomainException
    {
        public EntityNotFoundException(string entityName, object key)
            : base($"La entidad '{entityName}' con clave ({key}) no fue encontrada.") { }
    }

    /// <summary>
    /// Se lanza cuando una regla de negocio es violada.
    /// </summary>
    public class BusinessRuleException : DomainException
    {
        public BusinessRuleException(string message) : base(message) { }
    }

    /// <summary>
    /// Se lanza cuando un servicio no puede cambiar a un estado determinado.
    /// </summary>
    public class InvalidStateTransitionException : DomainException
    {
        public InvalidStateTransitionException(string currentState, string targetState)
            : base($"No se puede cambiar del estado '{currentState}' al estado '{targetState}'.") { }
    }
}
