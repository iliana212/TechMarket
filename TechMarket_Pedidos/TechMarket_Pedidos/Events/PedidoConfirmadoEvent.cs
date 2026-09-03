using Wolverine.Attributes;

namespace TechMarket_Pedidos.Events
{
	[MessageIdentity("pedido.confirmado")]
	public record PedidoConfirmadoEvent(int PedidoId, string ClienteNombre, List<ItemConfirmadoEvent> Items);
	public record ItemConfirmadoEvent(int ProductoId, int Cantidad);
	
	
}
