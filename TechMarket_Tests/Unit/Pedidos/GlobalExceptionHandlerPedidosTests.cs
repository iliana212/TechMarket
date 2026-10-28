using Microsoft.Extensions.Logging.Abstractions;
using TechMarket_Pedidos.Exceptions;
using TechMarket_Tests.Soporte;
using TechMarket_Pedidos.Middleware;

namespace TechMarket_Tests.Unit.Pedidos;

public class GlobalExceptionHandlerPedidosTests
{
	private static Task<RespuestaDeError> EjecutarAsync(Exception excepcion, string entorno = "Production") =>
		ManejadorDeExcepciones.EjecutarAsync(
		    new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, Entornos.Crear(entorno)), excepcion);

	[Fact]
    public async Task RecursoNoEncontrado_Responde404()
    {
        var respuesta = await EjecutarAsync(new RecursoNoEncontradoException("El producto 9 no existe"));

        Assert.Equal(404, respuesta.Status);
        Assert.Equal("Recurso no encontrado", respuesta.Cuerpo.GetProperty("title").GetString());
    }

    [Fact]
    public async Task StockInsuficiente_Responde409()
    {
        var respuesta = await EjecutarAsync(new StockInsuficienteException("Stock insuficiente para Mouse"));
	
        Assert.Equal(409, respuesta.Status);
        Assert.Equal("Stock insuficiente", respuesta.Cuerpo.GetProperty("title").GetString());
	}

    // Falla de infraestructura (Productos caído, timeout, circuito abierto): 503, no 500 ni 404.
    [Fact]
    public async Task ServicioDeProductosNoDisponible_Responde503()
    {
	    var respuesta = await EjecutarAsync(new ServiceProductosNoDisponibleException("No disponible"));
	
	    Assert.Equal(503, respuesta.Status);
	    Assert.Equal("Servicio no disponible", respuesta.Cuerpo.GetProperty("title").GetString());
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
}