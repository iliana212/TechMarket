using Microsoft.EntityFrameworkCore;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Data
{
	public class ProductoRepositorio(AppDbContext context) : IProductoRepositorio
	{
		public async Task<bool> Actualizar(int id, Producto producto)
		{
			var existe = await context.Productos.FirstOrDefaultAsync(x => x.Id == id);
			if (existe is null) return false;

			existe.Nombre = producto.Nombre;
			existe.CategoriaId = producto.CategoriaId;
			existe.Precio = producto.Precio;
			existe.Stock = producto.Stock;
			existe.Activo = producto.Activo;

			await context.SaveChangesAsync();
			return true;
		}

		public async Task<Producto> Crear(Producto producto)
		{
			context.Productos.Add(producto);
			await context.SaveChangesAsync();
			await context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
			return producto;
		}

		public async Task<bool> Eliminar(int id)
		{
			var existe = await context.Productos.FirstOrDefaultAsync(x => x.Id == id);
			if (existe is null) return false;

			context.Remove(existe);
			await context.SaveChangesAsync();
			return true;
		}

		public async Task<Producto?> ObtenerPorId(int id)
		{
			return await context.Productos
				.Include(x => x.Categoria)
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<IEnumerable<Producto>> ObtenerTodos()
		{
			return await context.Productos.AsNoTracking().Include(p => p.Categoria).ToListAsync();
		}
	}
}
