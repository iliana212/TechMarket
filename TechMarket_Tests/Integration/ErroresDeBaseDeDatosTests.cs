using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using TechMarket_Productos.Data;
using TechMarket_Productos.Middleware;
using TechMarket_Productos.Models;
using TechMarket_Tests.Soporte;

namespace TechMarket_Tests.Integration;

[Collection("Postgres")]
public class ErroresDeBaseDeDatosTests(PostgresFixture postgres)
{
    private static Task<RespuestaDeError> TraducirAsync(Exception excepcion) =>
		ManejadorDeExcepciones.EjecutarAsync(
			new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, Entornos.Crear("Production")), excepcion);

	[Fact]
    public async Task CategoriaConNombreDuplicado_ViolaElIndiceUnico_YSeTraduceA409()
    {
        var nombre = $"Cat-{Guid.NewGuid():N}";
        await using (var primero = postgres.CrearContextoProductos())
        {
            primero.Categorias.Add(new Categoria { Nombre = nombre });
            await primero.SaveChangesAsync();
        }

		await using var segundo = postgres.CrearContextoProductos();
		segundo.Categorias.Add(new Categoria { Nombre = nombre });
		var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() => segundo.SaveChangesAsync());

		var postgresError = Assert.IsType<PostgresException>(excepcion.InnerException);
		Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresError.SqlState);

		var respuesta = await TraducirAsync(excepcion);
		Assert.Equal(409, respuesta.Status);
		Assert.Equal("Conflicto de datos", respuesta.Cuerpo.GetProperty("title").GetString());
    }

	[Fact]
    public async Task ProductoConNombreDeMasDe100Caracteres_LoRechazaLaBaseDeDatos_YSeTraduceA400()
    {
		await using var contexto = postgres.CrearContextoProductos();
		var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}" };
		contexto.Categorias.Add(categoria);
		await contexto.SaveChangesAsync();
	
		var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() =>
		new ProductoRepositorio(contexto).Crear(new Producto
		{
			Nombre = new string('A', 101),   // la columna es varchar(100)
			CategoriaId = categoria.Id,
			Precio = 10m,
			Stock = 1
		}));
	
		var postgresError = Assert.IsType<PostgresException>(excepcion.InnerException);
		Assert.Equal("22001", postgresError.SqlState);   // string_data_right_truncation
	
		var respuesta = await TraducirAsync(excepcion);
		Assert.Equal(400, respuesta.Status);
		Assert.Equal("Error al guardar los datos", respuesta.Cuerpo.GetProperty("title").GetString());
	}

	[Fact]
    public async Task ProductoConCategoriaInexistente_ViolaLaLlaveForanea_YSeTraduceA400()
    {
		await using var contexto = postgres.CrearContextoProductos();
	
		var excepcion = await Assert.ThrowsAsync<DbUpdateException>(() =>
		new ProductoRepositorio(contexto).Crear(new Producto
		{
			Nombre = $"Prod-{Guid.NewGuid():N}",
			CategoriaId = 999_999_999,
			Precio = 10m,
			Stock = 1
		}));
	
		var postgresError = Assert.IsType<PostgresException>(excepcion.InnerException);
		Assert.Equal("23503", postgresError.SqlState);   // foreign_key_violation
	
		var respuesta = await TraducirAsync(excepcion);
		Assert.Equal(400, respuesta.Status);
	}
}