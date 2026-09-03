using TechMarket_Productos.Data;
using Wolverine.Attributes;

namespace TechMarket_Productos.Application.Productos.Events
{
	[WolverineHandler]
	public class PedidoConfirmadoEventHandler
	{
		public static async Task Handle(PedidoConfirmadoEvent evento, IProductoRepositorio repo, ILogger<PedidoConfirmadoEvent> logger)
		{
			logger.LogInformation("Procesando pedido confirmado: PedidoId={PedidoId}, Cliente={Cliente}, {CantidadItems} items",
				evento.PedidoId, evento.ClienteNombre, evento.Items.Count);
			
			foreach (var item in evento.Items) {
				var actualizado = await repo.DescontarStock(item.ProductoId, item.Cantidad);
			
				if (!actualizado)
				{
					logger.LogWarning("No se pudo descontar stock del producto {ProductoId} (pedido {PedidoId}): no existe o el stock ya no alcanza",
 						item.ProductoId, evento.PedidoId);
				}			
			}	
		}
	}
}
