using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechMarket_Pedidos.Exceptions;

namespace TechMarket_Pedidos.Middleware
{
	public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			var (statusCode, titulo, detalle) = exception switch
			{
				RecursoNoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
				StockInsuficienteException => (StatusCodes.Status409Conflict, "Stock insuficiente", exception.Message),
				ServiceProductosNoDisponibleException => (StatusCodes.Status503ServiceUnavailable, "Servicio no disponible", exception.Message),
				_ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", exception.Message)
			};

			if (statusCode == StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(exception, "Error no controlado procesando {Metodo} {Ruta}",
					httpContext.Request.Method, httpContext.Request.Path);
			}
			
			var problemDetails = new ProblemDetails
			{
				Status = statusCode,
				Title = titulo,
				Detail = detalle,
				Instance = httpContext.Request.Path
			};

			httpContext.Response.StatusCode = statusCode;
			await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

			return true;
		}

	}
}
