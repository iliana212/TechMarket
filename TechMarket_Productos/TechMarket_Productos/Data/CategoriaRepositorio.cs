using Microsoft.EntityFrameworkCore;
using TechMarket_Productos.Data;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Data
{
	public class CategoriaRepositorio(AppDbContext context) : ICategoriaRepositorio
	{
		public async Task<Categoria> Crear(Categoria categoria)
		{
			context.Categorias.Add(categoria);
			await context.SaveChangesAsync();
			return categoria;
		}

		public async Task<bool> Existe(int id)
		{
			var existe = await context.Categorias.AnyAsync(x => x.Id == id);
			return existe;
		}

		public async Task<IEnumerable<Categoria>> ObtenerTodos()
		{
			return await context.Categorias.AsNoTracking().ToListAsync();
		}

		public async Task<bool> Actualizar(int id, Categoria categoria)
		{
			var existe = await context.Categorias.FirstOrDefaultAsync(x => x.Id == id);
			if (existe is null) return false;

			existe.Nombre = categoria.Nombre;
			existe.Descripcion = categoria.Descripcion;

			await context.SaveChangesAsync();
			return true;
		}

		public async Task<Categoria?> ObtenerPorId(int id)
		{
			return await context.Categorias.Include(p => p.Productos).FirstOrDefaultAsync(x => x.Id == id);
		}
	}
}
