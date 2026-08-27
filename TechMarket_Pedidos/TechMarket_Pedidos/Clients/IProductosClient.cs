using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Clients
{
	public interface IProductosClient
	{
		Task<ProductoRemoto?> ObtenerProductoAsync(int id, CancellationToken cancellationToken);

	}
}
