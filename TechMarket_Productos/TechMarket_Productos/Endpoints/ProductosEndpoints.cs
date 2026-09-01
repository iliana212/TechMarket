using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using TechMarket_Productos.Application.Productos.Commands;
using TechMarket_Productos.Application.Productos.Queries;
using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Models;
using Wolverine;

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
			grupo.MapGet("/", async (IMessageBus bus) =>
			{
				var productos = await bus.InvokeAsync<IEnumerable<ProductoDTO>>(new ObtenerProductosQuery());
				return Results.Ok(productos);
			})
			.WithName("ObtenerProductos")
			.WithSummary("Lista todos los productos del catálogo");

			//GET
			grupo.MapGet("/{id:int}", async (int id, IMessageBus bus) =>
			{
				var producto = await bus.InvokeAsync<ProductoDTO>(new ObtenerProductoPorIdQuery(id));
				
				return Results.Ok(producto);
			})
			.WithName("ObtenerProductoPorId")
			.WithSummary("Obtiene un producto por su Id");

			//POST
			grupo.MapPost("/", async (CrearProductoDTO dto, IMessageBus bus) =>
			{
				var comando = new CrearProductoCommand(dto.Nombre, dto.CategoriaId, dto.Precio, dto.Stock);

				var creado = await bus.InvokeAsync<Producto>(comando);

				return Results.Created($"/api/productos/{creado.Id}", creado);
			})
			.WithName("CrearProducto")
			.WithSummary("Registra un nuevo producto en el catálogo");

			//PUT
			grupo.MapPut("/{id:int}", async (int id, ActualizarProductoDTO dto, IMessageBus bus) =>
			{
				var comando = new ActualizarProductoCommand(id, dto.Nombre, dto.CategoriaId, dto.Precio, dto.Stock, dto.Activo);
				await bus.InvokeAsync(comando);

				return Results.NoContent();
			})
			.WithName("ActualizarProducto")
			.WithSummary("Actualiza los datos de un producto existente");

			//DELETE
			grupo.MapDelete("/{id:int}", async (int id, IMessageBus bus) =>
			{
				await bus.InvokeAsync(new EliminarProductoCommand(id));
				return Results.NoContent();
			})
			.WithName("EliminarProducto")
			.WithSummary("Elimina un producto del catálogo");

		}

		private static ProductoDTO AProductoDTO(Producto p) =>
			new ProductoDTO(p.Id, p.Nombre, p.Precio, p.Stock, p.Activo, p.Categoria?.Nombre ?? "(sin Categoría)");
	}
}
