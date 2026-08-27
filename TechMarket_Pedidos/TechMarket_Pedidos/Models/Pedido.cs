namespace TechMarket_Pedidos.Models
{
	public class Pedido
	{
		public int Id { get; set; }
		public string ClienteNombre { get; set; } = string.Empty;
		public DateTime Fecha { get; set; } = DateTime.UtcNow;
		public string Estado { get; set; } = "Confirmado";
		public decimal Total {  get; set; }

		public List<ItemPedido> Items { get; set; } = new();
	}

	public record CrearPedidoDTO(string ClienteNombre, List<ItemPedidoDTO> Items);
	public record PedidoDTO(int Id, string ClienteNombre, DateTime Fecha, string Estado, decimal Total, List<ItemPedido> Items);
		
}
