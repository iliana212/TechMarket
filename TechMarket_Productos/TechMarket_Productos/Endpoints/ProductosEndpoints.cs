using FluentValidation;
using System.ComponentModel.DataAnnotations;
using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Endpoints
{
	public static class ProductosEndpoints
	{
		public static void MapProductosEndpoints(this WebApplication app)
		{
			//Ruta base
			var grupo = app.MapGroup("/api/productos")
				.WithTags("Productos");

			//GET
			grupo.MapGet("/", async (IProductoRepositorio repo) =>
			{
				var productos = await repo.ObtenerTodos();
				return Results.Ok(productos.Select(AProductoDTO));
			})
			.WithName("ObtenerProductos")
			.WithSummary("Lista todos los productos del catálogo");

			//GET
			grupo.MapGet("/{id:int}", async (int id, IProductoRepositorio repo) =>
			{
				var producto = await repo.ObtenerPorId(id);
				if (producto is null)
					throw new RecursoNoEncontradoException($"No existe el producto con Id {id}");

				return Results.Ok(AProductoDTO(producto));
			})
			.WithName("ObtenerProductoPorId")
			.WithSummary("Obtiene un producto por su Id");

			//POST
			grupo.MapPost("/", async (CrearProductoDTO dto, IValidator<CrearProductoDTO> validador, IProductoRepositorio repo) =>
			{
				await validador.ValidateAndThrowAsync(dto);

				var nuevo = new Producto
				{
					Nombre = dto.Nombre,
					CategoriaId = dto.CategoriaId,
					Precio = dto.Precio,
					Stock = dto.Stock
				};

				var creado = await repo.Crear(nuevo);

				return Results.Created($"/api/productos/{creado.Id}", AProductoDTO(creado)); ///++++ solo creado
			})
			.WithName("CrearProducto")
			.WithSummary("Registra un nuevo producto en el catálogo");

			//PUT
			grupo.MapPut("/{id:int}", async (int id, ActualizarProductoDTO dto, IValidator<ActualizarProductoDTO> validador, IProductoRepositorio repo) =>
			{
				await validador.ValidateAndThrowAsync(dto);

				var producto = new Producto
				{
					Nombre = dto.Nombre,
					CategoriaId = dto.CategoriaId,
					Precio = dto.Precio,
					Stock = dto.Stock,
					Activo = dto.Activo
				};

				return await repo.Actualizar(id, producto) ? Results.NoContent() : Results.NotFound(new { mensaje = $"Producto {id} no existe" });
			})
			.WithName("ActualizarProducto")
			.WithSummary("Actualiza los datos de un producto existente");

			//DELETE
			grupo.MapDelete("/{id:int}", async (int id, IProductoRepositorio repo) =>
			{
				return await repo.Eliminar(id) ? Results.NoContent() : Results.NotFound(new { mensaje = $"Producto {id} no existe" });
			})
			.WithName("EliminarProducto")
			.WithSummary("Elimina un producto del catálogo");

		}

		private static ProductoDTO AProductoDTO(Producto p) =>
			new ProductoDTO(p.Id, p.Nombre, p.Precio, p.Stock, p.Activo, p.Categoria?.Nombre ?? "(sin Categoría)");
	}
}
