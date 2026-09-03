using Wolverine.Attributes;

namespace TechMarket_Productos.Application.Productos.Events
{
	[MessageIdentity("pedido.confirmado")]
	public record PedidoConfirmadoEvent(int PedidoId, string ClienteNombre, List<ItemConfirmadoEvent> Items);
	public record ItemConfirmadoEvent(int ProductoId, int Cantidad);


}
