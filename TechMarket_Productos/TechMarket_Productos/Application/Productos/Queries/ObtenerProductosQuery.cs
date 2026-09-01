using TechMarket_Productos.Data;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Application.Productos.Queries
{
	public record ObtenerProductosQuery();

	public static class ObtenerProductoQueryHandler
	{
		public static async Task<IEnumerable<ProductoDTO>> Handler(ObtenerProductosQuery consulta, IProductoRepositorio repositorio)
		{
			var productos = await repositorio.ObtenerTodos();
			return productos.Select(p => new ProductoDTO(p.Id, p.Nombre, p.Precio, p.Stock, p.Activo, p.Categoria?.Nombre ?? "(sin categoria)"));
		}
	}
}
