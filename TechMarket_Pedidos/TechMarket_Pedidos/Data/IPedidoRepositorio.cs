using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Data
{
	public interface IPedidoRepositorio
	{
		Task<IEnumerable<Pedido>> ObtenerTodos();
		Task<Pedido?> ObtenerPorId(int id);
		Task<Pedido> Crear(Pedido pedido);
	}
}
