using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using TechMarket_Gateway.Observability;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Health check del propio Gateway (solo liveness: no tiene base de datos propia)
builder.Services.AddHealthChecks();

// Rate limiting: limita cuántas peticiones puede hacer cada cliente (por IP) en una ventana de tiempo.
// Las políticas se asignan a cada ruta de YARP en appsettings.json ("RateLimiterPolicy").
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	
	// Política general: 100 peticiones por minuto por IP
	options.AddPolicy("general", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
			factory: _ => new FixedWindowRateLimiterOptions
	        {
				PermitLimit = 100,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
            }
		)
	);
	
	// Política estricta para login: 10 intentos por minuto por IP (frena ataques de fuerza bruta)
	options.AddPolicy("auth", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
			factory: _ => new FixedWindowRateLimiterOptions
	        {
				PermitLimit = 10,
				Window = TimeSpan.FromMinutes(1),
				QueueLimit = 0
			}
		)
	);
	
	// Respuesta cuando se excede el límite: 429 + header Retry-After + ProblemDetails (mismo formato que el resto de la API)
	options.OnRejected = async (context, cancellationToken) =>
	{
		if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
		{
			context.HttpContext.Response.Headers["Retry-After"] = ((int)retryAfter.TotalSeconds).ToString();
		}
		
		context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Status = StatusCodes.Status429TooManyRequests,
			Title = "Demasiadas peticiones",
			Detail = "Superaste el límite de peticiones permitido. Intenta de nuevo en unos segundos.",
			Instance = context.HttpContext.Request.Path
        }, cancellationToken);
	};
});

builder.AddObservability();

var app = builder.Build();

app.UseRateLimiter();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.MapGet("/", () => Results.Ok(new
{
	servicio = "ms-gateway",
	estado = "OK",
	mensaje = "TechMarket - API Gateway - puerta de entrada única a productos y pedidos"
}));

app.MapReverseProxy();

app.Run();
