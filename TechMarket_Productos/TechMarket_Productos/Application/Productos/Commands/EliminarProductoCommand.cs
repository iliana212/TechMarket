using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;

namespace TechMarket_Productos.Application.Productos.Commands
{
	public record EliminarProductoCommand(int Id);

	public static class EliminarProductoHandler
	{
		public static async Task Handle(EliminarProductoCommand command, IProductoRepositorio repositorio)
		{
			var eliminado = await repositorio.Eliminar(command.Id);
			if (!eliminado)
				throw new RecursoNoEncontradoException("No eliminado");
		}
	}
}
