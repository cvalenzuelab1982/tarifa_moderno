using Directo.Wari.TarifaEngine.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Directo.Wari.TarifaEngine.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior que ejecuta las validaciones de FluentValidation
    /// antes de que el handler procese el request.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessages = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var error = Error.Validation(errorMessages);

                var genericType = typeof(TResponse).GenericTypeArguments[0];

                var method = typeof(Result)
                  .GetMethods()
                  .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                  .MakeGenericMethod(genericType);

                return (TResponse)method.Invoke(null, new object[] { error })!;
            }

            return await next();
        }
    }
}
