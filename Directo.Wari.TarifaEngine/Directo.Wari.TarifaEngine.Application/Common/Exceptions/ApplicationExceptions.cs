namespace Directo.Wari.TarifaEngine.Application.Common.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se produce un error de validación en la capa de aplicación.
    /// </summary>
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException() : base("Se han producido uno o más errores de validación.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors) : this()
        {
            Errors = errors;
        }

        public ValidationException(string propertyName, string errorMessage) : this()
        {
            Errors = new Dictionary<string, string[]>
        {
            { propertyName, [errorMessage] }
        };
        }
    }

    /// <summary>
    /// Excepción lanzada cuando no se tiene permiso para realizar una operación.
    /// </summary>
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException() : base("No tiene permisos para realizar esta acción.") { }
        public ForbiddenAccessException(string message) : base(message) { }
    }
}
