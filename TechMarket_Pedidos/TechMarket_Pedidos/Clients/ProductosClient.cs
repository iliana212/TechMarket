using TechMarket_Pedidos.Exceptions;
using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Clients
{
	public class ProductosClient(HttpClient http, ILogger<ProductosClient> logger) : IProductosClient
	{
		public async Task<ProductoRemoto?> ObtenerProductoAsync(int id, CancellationToken cancellationToken)
		{
			try
			{
				var respuesta = await http.GetAsync($"/api/productos/{id}", cancellationToken);
				if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
				respuesta.EnsureSuccessStatusCode();
				return await respuesta.Content.ReadFromJsonAsync<ProductoRemoto>(cancellationToken);
			}
			catch (Exception ex) {
				logger.LogWarning(ex, "Error al consultar el producto {ProductoId} en el servicio de Productos", id);
				throw new ServiceProductosNoDisponibleException("Servicio No disponible", ex);
			}
		}
	}
}
