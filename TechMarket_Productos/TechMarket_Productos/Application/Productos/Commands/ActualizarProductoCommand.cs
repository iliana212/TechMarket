using FluentValidation;
using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Application.Productos.Commands
{
	public record ActualizarProductoCommand(int Id, string Nombre, int CategoriaId, decimal Precio, int Stock, bool Activo);

	public class ActualizarProductoHandler()
	{
		public static async Task Handler(ActualizarProductoCommand comando, IValidator<ActualizarProductoDTO> validator, IProductoRepositorio repositorio)
		{
			var dto = new ActualizarProductoDTO(comando.Nombre, comando.CategoriaId, comando.Precio, comando.Stock, comando.Activo);
			await validator.ValidateAndThrowAsync(dto);
			var producto = new Producto { Nombre = comando.Nombre, CategoriaId = comando.CategoriaId, Precio = comando.Precio, Stock = comando.Stock, Activo = comando.Activo };

			var actualizado = await repositorio.Actualizar(comando.Id, producto);
			if (!actualizado)
				throw new RecursoNoEncontradoException("No actualiza");
		}
	}	
}
