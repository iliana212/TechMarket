namespace TechMarket_Pedidos.Models
{
	public record ProductoRemoto(
		int Id, string Nombre, decimal Precio, int Stock, bool Activo, string Categoria
	);
}
