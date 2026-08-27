namespace TechMarket_Pedidos.Models
{
	public class ItemPedido
	{
		public int Id { get; set; }
		public int PedidoId { get; set; }
		public int ProductoId { get; set; }
		public string ProductoNombre { get; set; } = string.Empty;
		public int Cantidad { get; set; }
		public decimal PrecioUnitario { get; set; }
		public decimal Subtotal => Cantidad * PrecioUnitario;
	}

	public record ItemPedidoDTO(int ProductoId, int Cantidad);

}
