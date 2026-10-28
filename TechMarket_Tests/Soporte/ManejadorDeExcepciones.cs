using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace TechMarket_Tests.Soporte;

public sealed record RespuestaDeError(bool Manejada, int Status, JsonElement Cuerpo);

public static class ManejadorDeExcepciones
{
    public static async Task<RespuestaDeError> EjecutarAsync(IExceptionHandler manejador, Exception excepcion, string ruta = "/api/prueba")
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Method = HttpMethods.Get;
        contexto.Request.Path = ruta;
		contexto.TraceIdentifier = "traza-de-prueba";
		contexto.Response.Body = new MemoryStream();

        var manejada = await manejador.TryHandleAsync(contexto, excepcion, CancellationToken.None);

        contexto.Response.Body.Position = 0;
        using var documento = await JsonDocument.ParseAsync(contexto.Response.Body);
        return new RespuestaDeError(manejada, contexto.Response.StatusCode, documento.RootElement.Clone());
    }
}