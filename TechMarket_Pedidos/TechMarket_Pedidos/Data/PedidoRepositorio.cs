using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Data
{
	public class PedidoRepositorio : IPedidoRepositorio
	{
		private readonly List<Pedido> _pedidos = new List<Pedido>();
		private int _siguienteId = 1;

		public Pedido Crear(Pedido pedido)
		{
			pedido.Id = _siguienteId++;
			_pedidos.Add(pedido);
			return pedido;
		}

		public Pedido? ObtenerPorId(int id)
		{
			return _pedidos.FirstOrDefault(o => o.Id == id);
		}

		public IEnumerable<Pedido> ObtenerTodos()
		{
			return _pedidos.ToList();
		}
	}
}
