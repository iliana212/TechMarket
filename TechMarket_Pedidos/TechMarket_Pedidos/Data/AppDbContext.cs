using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechMarket_Pedidos.Models;

namespace TechMarket_Pedidos.Data
{
	public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
	{
		public DbSet<Pedido> Pedidos { get; set; }
		public DbSet<ItemPedido> ItemPedidos { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Pedido>(entidad =>
            {
				entidad.ToTable("Pedidos");
				entidad.HasKey(p => p.Id);
				entidad.Property(p => p.ClienteNombre).IsRequired().HasMaxLength(150);
				entidad.Property(p => p.Estado).IsRequired().HasMaxLength(50);
				entidad.Property(p => p.Total).HasColumnType("numeric(10,2)");
				
				entidad.HasMany(p => p.Items)
				                   .WithOne()
				                   .HasForeignKey(i => i.PedidoId)
				                   .OnDelete(DeleteBehavior.Cascade);
			});
			
			modelBuilder.Entity<ItemPedido>(entidad =>
			{
				entidad.ToTable("ItemsPedido");
				entidad.HasKey(i => i.Id);
				entidad.Property(i => i.ProductoNombre).IsRequired().HasMaxLength(150);
				entidad.Property(i => i.PrecioUnitario).HasColumnType("numeric(10,2)");				         
				            
				entidad.Ignore(i => i.Subtotal); // Subtotal es una propiedad calculada en memoria
			});
		}
	}
}
