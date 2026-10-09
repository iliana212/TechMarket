using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechMarket_Pedidos.Data;

namespace TechMarket_Pedidos.Health
{	
	public class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
	{
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
		{
			try
			{
				return await db.Database.CanConnectAsync(cancellationToken)
					? HealthCheckResult.Healthy("PostgreSQL responde")
					: HealthCheckResult.Unhealthy("PostgreSQL no responde");
			}
			catch (Exception ex)
			{
				return HealthCheckResult.Unhealthy("Error al conectar con PostgreSQL", ex);
			}
       }
   }
}