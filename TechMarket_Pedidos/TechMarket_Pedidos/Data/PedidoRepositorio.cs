using Microsoft.EntityFrameworkCore;
using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Data
{
	public class PedidoRepositorio(AppDbContext context) : IPedidoRepositorio
	{
		public async Task<Pedido> Crear(Pedido pedido)
		{
			context.Pedidos.Add(pedido);
			await context.SaveChangesAsync();
			return pedido;
		}

		public async Task<Pedido?> ObtenerPorId(int id)
		{
			return await context.Pedidos
				.Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
		}

		public async Task<IEnumerable<Pedido>> ObtenerTodos()
		{
			return await context.Pedidos.Include(p => p.Items).AsNoTracking().ToListAsync();
		}
	}
}
