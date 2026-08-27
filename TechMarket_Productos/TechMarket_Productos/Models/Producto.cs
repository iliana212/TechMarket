namespace TechMarket_Productos.Models
{	
	public class Producto
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public decimal Precio { get; set; }
		public int Stock { get; set; }
		public bool Activo { get; set; } = true;
		public int CategoriaId { get; set; }

		public Categoria? Categoria { get; set; }
	}

	public record CrearProductoDTO(string Nombre, int CategoriaId, decimal Precio, int Stock);

	public record ActualizarProductoDTO(string Nombre, int CategoriaId, decimal Precio, int Stock, bool Activo);

	public record ProductoDTO(int Id, string Nombre, decimal Precio, int Stock, bool Activo, string Categoria);

	public record ProductoResumenDTO(int Id, string Nombre, decimal Precio, int Stock, bool Activo);
}

