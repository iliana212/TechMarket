namespace TechMarket_Productos.Models
{
	public class Categoria
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string? Descripcion { get; set; }

		public ICollection<Producto> Productos { get; set; } = new List<Producto>();
	}
	
	public record CrearCategoriaDTO(string Nombre, string? Descripcion);
	public record CategoriaDTO(int Id, string Nombre, string? Descripcion, List<ProductoResumenDTO> Productos);
	public record CategoriaResumenDTO(int Id, string Nombre, string? Descripcion);
}
