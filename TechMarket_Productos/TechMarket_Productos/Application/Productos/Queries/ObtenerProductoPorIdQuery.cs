using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Application.Productos.Queries
{
	public record ObtenerProductoPorIdQuery(int Id);

	public static class ObtenerProductoPorIdHandler
	{
		public static async Task<ProductoDTO> Handler(ObtenerProductoPorIdQuery consulta, IProductoRepositorio repositorio)
		{
			var producto = await repositorio.ObtenerPorId(consulta.Id);
			if (producto is null)
				throw new RecursoNoEncontradoException("No existe el producto");

			return new ProductoDTO(producto.Id, producto.Nombre, producto.Precio, producto.Stock, producto.Activo, producto.Categoria?.Nombre ?? "(sin categoria)");
		}
	}
}
