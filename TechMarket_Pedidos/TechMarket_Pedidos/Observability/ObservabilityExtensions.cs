using OpenTelemetry.Trace;

namespace TechMarket_Pedidos.Observability
{
	public static class ObservabilityExtensions
	{
		public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
		{
			// Los logs muestran TraceId y SpanId
			builder.Logging.AddSimpleConsole(options =>
			{
				options.IncludeScopes = true;
				options.SingleLine = true;
			});

			var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

			builder.Services.AddOpenTelemetry()
               .WithTracing(tracing =>
               {
					tracing	                       
	                    .AddAspNetCoreInstrumentation(options => // Peticiones HTTP entrantes
							options.Filter = context => !context.Request.Path.StartsWithSegments("/health"))
	                    .AddSource("System.Net.Http") // Llamadas HTTP salientes														 
						.AddSource("Npgsql") // Consultas SQL de Npgsql
						.AddSource("Wolverine"); // Mensajería de Wolverine: publicar y consumir eventos de RabbitMQ

				   if (!string.IsNullOrWhiteSpace(otlpEndpoint))
		           {
						tracing.AddOtlpExporter();
		           }
	           });

           return builder;
        }
    }
}