using FluentValidation;
using TechMarket_Productos.Data;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Application.Productos.Commands
{
	public record CrearProductoCommand(string Nombre, int CategoriaId, decimal Precio, int Stock);
	
	public class CrearProductoHandler()
	{
		public static async Task<Producto> Handler(CrearProductoCommand comando, IValidator<CrearProductoDTO> validator, IProductoRepositorio repositorio)
		{
			var dto = new CrearProductoDTO(comando.Nombre, comando.CategoriaId, comando.Precio, comando.Stock);
			await validator.ValidateAndThrowAsync(dto);
			var producto = new Producto { Nombre = comando.Nombre, CategoriaId = comando.CategoriaId, Precio = comando.Precio, Stock = comando.Stock };

			var creado = await repositorio.Crear(producto);
			return creado;
		}
	}
}
