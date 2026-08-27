using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TechMarket_Productos.Exceptions;

namespace TechMarket_Productos.Middleware
{
	public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			var (statusCode, titulo, detalle) = exception switch
			{
				RecursoNoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
				
				ValidationException => (StatusCodes.Status400BadRequest, "Error de Validación", exception.Message),
				
				// Violación de restricción única en PostgreSQL (ej. nombre de categoría duplicado)
				DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } }
				                   => (StatusCodes.Status409Conflict, "Conflicto de datos", "Ya existe un registro con esos datos (por ejemplo, un nombre duplicado)."),
				
				// Cualquier otro error al guardar en la base de datos (truncado, FK inválida, etc.)
				DbUpdateException => (StatusCodes.Status400BadRequest, "Error al guardar los datos", "No se pudo guardar la información. Verifica los datos enviados."),
				
				_ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", exception.Message)
			};

			if (statusCode == StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(exception, "Error no controlado procesando {Metodo} {Ruta}",
					httpContext.Request.Method, httpContext.Request.Path);
			}
			else if (exception is DbUpdateException)
			{
				_logger.LogWarning(exception, "Error de base de datos procesando {Metodo} {Ruta}",
					httpContext.Request.Method, httpContext.Request.Path);
			}

			var problemDetails = new ProblemDetails
			{
				Status = statusCode,
				Title = titulo,
				Detail = detalle,
				Instance = httpContext.Request.Path
			};

			if (exception is ValidationException validationException)
			{
				problemDetails.Extensions["errores"] = validationException.Errors
					.Select(e => new { campo = e.PropertyName, mensaje = e.ErrorMessage });
			}

			httpContext.Response.StatusCode = statusCode;
			await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

			return true;
		}
	}
}
