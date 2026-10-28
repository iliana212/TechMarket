using TechMarket_Tests.Soporte;
using TechMarket_Productos.Exceptions;
using TechMarket_Productos.Middleware;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;

namespace TechMarket_Tests.Unit.Productos;

public class GlobalExceptionHandlerProductosTests
{
	private static Task<RespuestaDeError> EjecutarAsync(Exception excepcion, string entorno = "Production") =>
        ManejadorDeExcepciones.EjecutarAsync(
            new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, Entornos.Crear(entorno)), excepcion);

	[Fact]
    public async Task RecursoNoEncontrado_Responde404()
    {
        var respuesta = await EjecutarAsync(new RecursoNoEncontradoException("No existe el producto"));

        Assert.True(respuesta.Manejada);
        Assert.Equal(404, respuesta.Status);
        Assert.Equal("Recurso no encontrado", respuesta.Cuerpo.GetProperty("title").GetString());
        Assert.Equal("No existe el producto", respuesta.Cuerpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ErrorDeValidacion_Responde400ConLaListaDeErrores()
    {
	    var excepcion = new ValidationException("fallo",
	    [
		    new ValidationFailure("Nombre", "El nombre del producto es obligatorio"),
            new ValidationFailure("Precio", "El precio debe ser mayor a cero")
        ]);
	
	    var respuesta = await EjecutarAsync(excepcion);
	
	    Assert.Equal(400, respuesta.Status);
	    var errores = respuesta.Cuerpo.GetProperty("errores");
	    Assert.Equal(2, errores.GetArrayLength());
	    Assert.Equal("Nombre", errores[0].GetProperty("campo").GetString());
	    Assert.Equal("El precio debe ser mayor a cero", errores[1].GetProperty("mensaje").GetString());
	}

    [Fact]
    public async Task ErrorGenericoDeBaseDeDatos_Responde400SinExponerElDetalleInterno()
    {
	    var respuesta = await EjecutarAsync(new DbUpdateException("fallo", new Exception("detalle-interno-secreto")));
	
	    Assert.Equal(400, respuesta.Status);
	    Assert.Equal("Error al guardar los datos", respuesta.Cuerpo.GetProperty("title").GetString());
	    Assert.DoesNotContain("secreto", respuesta.Cuerpo.GetRawText());
	}

	// El mensaje real de una excepción inesperada puede revelar detalles internos (tablas, consultas, rutas).
    [Fact]
    public async Task ErrorInesperado_EnProduccion_NoExponeElMensajeDeLaExcepcion()
    {
        var respuesta = await EjecutarAsync(new InvalidOperationException("relation \"Productos\" no existe: detalle-secreto"), "Production");

        Assert.Equal(500, respuesta.Status);
        Assert.Equal("Ocurrió un error inesperado.", respuesta.Cuerpo.GetProperty("detail").GetString());
        Assert.DoesNotContain("secreto", respuesta.Cuerpo.GetRawText());
    }

    [Fact]
    public async Task ErrorInesperado_EnDesarrollo_IncluyeElMensajeReal()
    {
        var respuesta = await EjecutarAsync(new InvalidOperationException("detalle visible"), "Development");

        Assert.Equal(500, respuesta.Status);
        Assert.Equal("detalle visible", respuesta.Cuerpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Respuesta_IncluyeUnTraceIdParaRastrearElError()
    {
	    var respuesta = await EjecutarAsync(new InvalidOperationException("x"));	
	    Assert.False(string.IsNullOrWhiteSpace(respuesta.Cuerpo.GetProperty("traceId").GetString()));
	}

    [Fact]
    public async Task ErrorInesperado_Responde500()
    {
	    var respuesta = await EjecutarAsync(new InvalidOperationException("boom"));
	
	    Assert.Equal(500, respuesta.Status);
	    Assert.Equal("Error interno del servidor", respuesta.Cuerpo.GetProperty("title").GetString());
	}

    [Fact]
    public async Task Respuesta_IncluyeInstanceYStatusCoherente()
    {
	    var respuesta = await EjecutarAsync(new RecursoNoEncontradoException("x"));
	
	    Assert.Equal("/api/prueba", respuesta.Cuerpo.GetProperty("instance").GetString());
	    Assert.Equal(respuesta.Status, respuesta.Cuerpo.GetProperty("status").GetInt32());
	}
}