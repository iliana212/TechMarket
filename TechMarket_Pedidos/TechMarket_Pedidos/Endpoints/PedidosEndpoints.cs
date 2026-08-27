using TechMarket_Pedidos.Clients;
using TechMarket_Pedidos.Data;
using TechMarket_Pedidos.Exceptions;
using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Endpoints
{
	public static class PedidosEndpoints
	{
		public static void MapPedidosEndpoints(this WebApplication app)
		{
			var grupo = app.MapGroup("/api/pedidos")
				.WithTags("Pedidos");

			//GET
			grupo.MapGet("/", async (IPedidoRepositorio repo) =>
			{
				var pedidos = (await repo.ObtenerTodos()).Select(APedidoDTO);
				return Results.Ok(pedidos);
			})
			.WithName("ObtenerPedidos").WithSummary("Lista todos los pedidos");

			//GET
			grupo.MapGet("/{id:int}", async (int id, IPedidoRepositorio repo) =>
			{
				var pedido = await repo.ObtenerPorId(id);
				if (pedido is null)
					throw new RecursoNoEncontradoException($"No existe un pedido con Id {id}");

				return Results.Ok(APedidoDTO(pedido));
			})
			.WithName("ObtenerPedidoPorId").WithSummary("Obtiene un pedido por su Id");

			//POST
			grupo.MapPost("/", async (CrearPedidoDTO dto, IProductosClient client, IPedidoRepositorio repo, CancellationToken cancellation) =>
			{
				if (string.IsNullOrEmpty(dto.ClienteNombre))
					return Results.BadRequest(new { mensaje = "Nombre de cliente obligatorio" });

				if (dto.Items is null || dto.Items.Count == 0)
					return Results.BadRequest(new { mensaje = "El pedido debe tener al menos un producto" });

				var pedido = new Pedido
				{
					ClienteNombre = dto.ClienteNombre
				};
				
				foreach (var item in dto.Items)
				{
					var producto = await client.ObtenerProductoAsync(item.ProductoId, cancellation);

					if (producto is null)
						throw new RecursoNoEncontradoException($"El producto {item.ProductoId} no existe en el catálogo");			
				
					if (!producto.Activo)			
						throw new RecursoNoEncontradoException($"El producto {producto.Nombre} ya no está disponible");

					if (producto.Stock < item.Cantidad)					
						throw new StockInsuficienteException($"Stock insuficiente para {producto.Nombre}: disponible {producto.Stock}");
					

					pedido.Items.Add(new ItemPedido
					{
						ProductoId = producto.Id,
						ProductoNombre = producto.Nombre,
						Cantidad = item.Cantidad,
						PrecioUnitario = producto.Precio

					});
				}

				pedido.Total = pedido.Items.Sum(i => i.Subtotal);

				var creado = await repo.Crear(pedido);
				return Results.Created($"/api/pedidos/{creado.Id}", APedidoDTO(creado));
			})
			.WithName("CrearPedido").WithSummary("Crea un nuevo pedido");
		}


		private static PedidoDTO APedidoDTO(Pedido p) =>
			new PedidoDTO(p.Id, p.ClienteNombre, p.Fecha, p.Estado, p.Total, p.Items);
	}
}
