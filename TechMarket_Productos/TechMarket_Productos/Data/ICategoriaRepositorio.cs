using TechMarket_Productos.Models;

namespace TechMarket_Productos.Data
{
	public interface ICategoriaRepositorio
	{
		Task<bool> Actualizar(int id, Categoria categoria);
		Task<Categoria> Crear(Categoria categoria);
		Task<Categoria?> ObtenerPorId(int id);
		Task<bool> Existe(int id);
		Task<IEnumerable<Categoria>> ObtenerTodos();
	}
}