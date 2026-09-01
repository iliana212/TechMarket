using FluentValidation;
using System.ComponentModel.DataAnnotations;
using TechMarket_Productos.Data;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Endpoints
{
	public static class CategoriasEndpoints
	{
		public static void MapCategoriasEndpoints(this WebApplication app)
		{
			//Ruta base
			var grupo = app.MapGroup("/api/categorias")
				.WithTags("Categorias");

			//GET
			grupo.MapGet("/", async (ICategoriaRepositorio repo) =>
			{
				var categorias = await repo.ObtenerTodos();
				var resultado = categorias.Select(c => new CategoriaResumenDTO(c.Id,c.Nombre,c.Descripcion));
				return Results.Ok(resultado);
			})
			.WithName("ObtenerCategorias")
			.WithSummary("Lista todas las categorías");

			//GET
			grupo.MapGet("/{id:int}", async (int id, ICategoriaRepositorio repo) =>
			{
				var categoria = await repo.ObtenerPorId(id);
				if (categoria is null)
					throw new RecursoNoEncontradoException($"No existe una categoría con Id {id}");

				return Results.Ok(ACategoriaDTO(categoria));
			})
			.WithName("ObtenerCategoriaPorId")
			.WithSummary("Obtiene una categoría con sus productos asociados");

			//POST
			grupo.MapPost("/", async (CrearCategoriaDTO dto, IValidator<CrearCategoriaDTO> validador, ICategoriaRepositorio repo) =>
			{
				await validador.ValidateAndThrowAsync(dto);

				var nueva = new Categoria { Nombre = dto.Nombre, Descripcion = dto.Descripcion };
				var creada = await repo.Crear(nueva);

				return Results.Created($"/api/categorias/{creada.Id}", ACategoriaDTO(creada));
			})
			.WithName("CrearCategoria")
			.WithSummary("Registra una nueva categoría")
			.RequireAuthorization("AdminOnly");

			//PUT
			grupo.MapPut("/{id:int}", async (int id, CrearCategoriaDTO dto, IValidator<CrearCategoriaDTO> validador, ICategoriaRepositorio repo) =>
			{
				await validador.ValidateAndThrowAsync(dto);

				var categoria = new Categoria
				{
					Nombre = dto.Nombre,
					Descripcion = dto.Descripcion					
				};

				return await repo.Actualizar(id, categoria) ? Results.NoContent() : Results.NotFound(new { mensaje = $"Categoria {id} no existe" });
			})
			.WithName("ActualizarCategoria")
			.WithSummary("Actualiza los datos de una categoria existente")
			.RequireAuthorization("AdminOnly");
		}


		private static CategoriaDTO ACategoriaDTO(Categoria c) =>
			new CategoriaDTO(c.Id, c.Nombre, c.Descripcion, c.Productos.Select(p => new ProductoResumenDTO(p.Id, p.Nombre,p.Precio, p.Stock, p.Activo)).ToList());
	}
}
