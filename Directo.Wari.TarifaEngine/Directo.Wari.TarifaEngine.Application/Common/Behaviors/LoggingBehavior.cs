using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace Directo.Wari.TarifaEngine.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior que registra información de cada request/response.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation(
                "WariDirecto Request: {Name} {@Request}",
                requestName, request);

            var response = await next();

            //En caso de traer una JSON lista, evita la demora en la escritura de los logs y solo indica la cantidad de registros
            if (response is ICollection collection)
            {
                _logger.LogInformation(
                    "WariDirecto Response: {Name} Count: {Count}",
                    requestName,
                    collection.Count);
            }
            else
            {
                _logger.LogInformation(
                    "WariDirecto Response: {Name} {@Response}",
                    requestName, response);
            }


            return response;
        }
    }
}
