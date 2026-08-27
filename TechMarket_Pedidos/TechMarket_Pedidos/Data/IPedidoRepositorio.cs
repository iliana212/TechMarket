using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Data
{
	public interface IPedidoRepositorio
	{
		IEnumerable<Pedido> ObtenerTodos();
		Pedido? ObtenerPorId(int id);
		Pedido Crear(Pedido pedido);
	}
}
