using TechMarket_Productos.Models;

namespace TechMarket_Productos.Data
{
	public interface IProductoRepositorio
	{
		Task<bool> Actualizar(int id, Producto producto);
		Task<Producto> Crear(Producto producto);
		Task<bool> Eliminar(int id);
		Task<Producto?> ObtenerPorId(int id);
		Task<IEnumerable<Producto>> ObtenerTodos();
		Task<bool> DescontarStock(int productoId, int cantidad);
	}
}