using OpenTelemetry.Trace;

namespace TechMarket_Gateway.Observability
{
	public static class ObservabilityExtensions
	{
		public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
		{
			builder.Logging.AddSimpleConsole(options => // Los logs muestran TraceId y SpanId
			{
				options.IncludeScopes = true;
				options.SingleLine = true;
			});

			var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

			builder.Services.AddOpenTelemetry().WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation(options => // Peticiones HTTP entrantes
						options.Filter = context => !context.Request.Path.StartsWithSegments("/health")) 
					.AddSource("System.Net.Http"); // Llamadas HTTP salientes

				if (!string.IsNullOrWhiteSpace(otlpEndpoint))
		        {
					tracing.AddOtlpExporter();
		        }
	        });

           return builder;
        }
    }
}