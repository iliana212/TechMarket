using Microsoft.EntityFrameworkCore;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Producto> Productos { get; set; }
		public DbSet<Categoria> Categorias { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Categoria>(entidad =>
			{
				entidad.ToTable("Categorias");
				entidad.HasKey(c => c.Id);
				entidad.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
				entidad.Property(c => c.Descripcion).HasMaxLength(300);
				entidad.HasIndex(c => c.Nombre).IsUnique();
			});

			modelBuilder.Entity<Producto>(entidad =>
			{
				entidad.ToTable("Productos");
				entidad.HasKey(c => c.Id);
				entidad.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
				entidad.Property(c => c.Precio).HasColumnType("numeric(10,2)");

				entidad.HasOne(p => p.Categoria)
					.WithMany(c => c.Productos)
					.HasForeignKey(p => p.CategoriaId)
					.OnDelete(DeleteBehavior.Restrict);
			});
		}		
	}
}
