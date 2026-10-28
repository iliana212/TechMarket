using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TechMarket_Pedidos.Clients;
using TechMarket_Pedidos.Data;
using TechMarket_Pedidos.Endpoints;
using TechMarket_Pedidos.Events;
using TechMarket_Pedidos.Exceptions;
using TechMarket_Pedidos.Middleware;
using TechMarket_Pedidos.Models;
using Wolverine;

namespace TechMarket_Tests.Unit.Pedidos;

public class PedidosEndpointsTests
{
    private static readonly ProductoRemoto Mouse = new(1, "Mouse", 250m, 10, true, "Perifericos");
    private static readonly ProductoRemoto Teclado = new(2, "Teclado", 400m, 5, true, "Perifericos");

    private sealed class Escenario : IAsyncDisposable
    {
        public IProductosClient Productos { get; } = Substitute.For<IProductosClient>();
        public IPedidoRepositorio Repositorio { get; } = Substitute.For<IPedidoRepositorio>();
		public Wolverine.IMessageBus Bus { get; } = Substitute.For<Wolverine.IMessageBus>();
		public HttpClient Cliente { get; private set; } = null!;
        private WebApplication? _app;
		private readonly CapturaDeErrores _errores = new();

		public Escenario()
        {
            // Por defecto, "guardar" un pedido le asigna el Id 42 y lo devuelve.
            Repositorio.Crear(Arg.Any<Pedido>()).Returns(llamada =>
            {
				var pedido = llamada.Arg<Pedido>();
				pedido.Id = 42;
		        return Task.FromResult(pedido);
		    });
        }

        public Escenario ConProducto(ProductoRemoto producto)
        {
            Productos.ObtenerProductoAsync(producto.Id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductoRemoto?>(producto));
            return this;
        }

        public Escenario SinProducto(int id)
        {
            Productos.ObtenerProductoAsync(id, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<ProductoRemoto?>(null));
            return this;
        }

        public async Task<Escenario> IniciarAsync()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Logging.ClearProviders();
			builder.Logging.AddProvider(_errores);

			builder.Services.AddSingleton<IProductosClient>(Productos);
			builder.Services.AddSingleton<IPedidoRepositorio>(Repositorio);
			builder.Services.AddSingleton<IMessageBus>(Bus);
			builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            _app = builder.Build();

			_app.UseExceptionHandler();
            _app.MapPedidosEndpoints();

            await _app.StartAsync();
			Cliente = new HttpClient(new FallaSiHay500(_app.GetTestServer().CreateHandler(), _errores))
            {
				BaseAddress = new Uri("http://localhost/")
			};
			return this;
		}

		public IEnumerable<PedidoConfirmadoEvent> EventosPublicados() =>
			Bus.ReceivedCalls()
                .Where(llamada => llamada.GetMethodInfo().Name == "PublishAsync")
                .Select(llamada => llamada.GetArguments()[0])
                .OfType<PedidoConfirmadoEvent>();

        public async ValueTask DisposeAsync()
        {
            Cliente?.Dispose();
            if (_app is not null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
            }
        }
	}

	// Ninguna de estas pruebas espera un 500. Si el servidor responde 500, la prueba falla de inmediato con la
    // excepción real que se registró en el log (en producción el cuerpo de la respuesta oculta el mensaje).
    private sealed class FallaSiHay500(HttpMessageHandler interno, CapturaDeErrores errores) : DelegatingHandler(interno)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage solicitud, CancellationToken cancelacion)
        {
            var respuesta = await base.SendAsync(solicitud, cancelacion);
            if (respuesta.StatusCode == HttpStatusCode.InternalServerError)
            {
                var detalle = errores.Errores.Count > 0 ? string.Join("\n---\n", errores.Errores) : "(sin errores registrados)";
                throw new InvalidOperationException(
                    $"{solicitud.Method} {solicitud.RequestUri?.PathAndQuery} respondió 500. Excepción registrada:\n{detalle}");
            }
            return respuesta;
        }
    }

    private sealed class CapturaDeErrores : ILoggerProvider, ILogger
    {
        public List<string> Errores { get; } = [];

        public ILogger CreateLogger(string categoria) => this;
        public IDisposable? BeginScope<TState>(TState estado) where TState : notnull => null;
        public bool IsEnabled(LogLevel nivel) => nivel >= LogLevel.Error;

        public void Log<TState>(LogLevel nivel, EventId id, TState estado, Exception? excepcion, Func<TState, Exception?, string> formateador)
        {
            if (nivel >= LogLevel.Error)
                Errores.Add(excepcion?.ToString() ?? formateador(estado, excepcion));
        }

        public void Dispose() { }
    }

	private static object CuerpoPedido(string cliente, params (int ProductoId, int Cantidad)[] items) => new
	{
		clienteNombre = cliente,
		items = items.Select(i => new { productoId = i.ProductoId, cantidad = i.Cantidad }).ToArray()
	};

    // ---------- Pedido válido ----------
	[Fact]
    public async Task Crear_ConDatosValidos_Responde201CalculaElTotalYPublicaElEvento()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).IniciarAsync();	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 2)));
	
		Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
		Assert.Equal("/api/pedidos/42", respuesta.Headers.Location!.ToString());
	
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Equal(42, cuerpo.GetProperty("id").GetInt32());
		Assert.Equal("Ana", cuerpo.GetProperty("clienteNombre").GetString());
		Assert.Equal("Confirmado", cuerpo.GetProperty("estado").GetString());
		Assert.Equal(500m, cuerpo.GetProperty("total").GetDecimal());   // 2 x 250
	
		var item = cuerpo.GetProperty("items")[0];
		Assert.Equal("Mouse", item.GetProperty("productoNombre").GetString());
		Assert.Equal(250m, item.GetProperty("precioUnitario").GetDecimal());
	
		var evento = Assert.Single(escenario.EventosPublicados());
		Assert.Equal(42, evento.PedidoId);
		Assert.Equal("Ana", evento.ClienteNombre);
		var itemEvento = Assert.Single(evento.Items);
		Assert.Equal(1, itemEvento.ProductoId);
		Assert.Equal(2, itemEvento.Cantidad);
	}

	[Fact]
    public async Task Crear_ConVariosProductos_SumaElTotalDeTodasLasLineas()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).ConProducto(Teclado).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 2), (2, 1)));
	
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Equal(900m, cuerpo.GetProperty("total").GetDecimal());   // 2 x 250  1 x 400
		Assert.Equal(2, cuerpo.GetProperty("items").GetArrayLength());
	}

    // El precio siempre sale del microservicio de Productos, nunca de lo que mande el cliente.
	[Fact]
    public async Task Crear_IgnoraElPrecioQueMandaElCliente()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).IniciarAsync();
		var cuerpoManipulado = new
		{
			clienteNombre = "Ana",
			items = new[] { new { productoId = 1, cantidad = 2, precioUnitario = 1 } }
		};
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", cuerpoManipulado);
	
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Equal(500m, cuerpo.GetProperty("total").GetDecimal());
	}

    // ---------- Validaciones de la petición ---------
	[Fact]
    public async Task Crear_SinNombreDeCliente_Responde400YNoGuardaNada()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("", (1, 1)));
	
		Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
		_ = escenario.Repositorio.DidNotReceive().Crear(Arg.Any<Pedido>());
		Assert.Empty(escenario.EventosPublicados());
	}

	[Fact]
    public async Task Crear_SinProductos_Responde400()
    {
		await using var escenario = await new Escenario().IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana"));
	
		Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
		_ = escenario.Repositorio.DidNotReceive().Crear(Arg.Any<Pedido>());
	}

    // ---------- Reglas de negocio (errores de negocio, no de infraestructura) ----------
	[Fact]
    public async Task Crear_ConProductoInexistente_Responde404()
    {
		await using var escenario = await new Escenario().SinProducto(999).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (999, 1)));
	
		Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Contains("999", cuerpo.GetProperty("detail").GetString());
		Assert.Empty(escenario.EventosPublicados());
	}

	[Fact]
    public async Task Crear_ConProductoInactivo_Responde404()
    {
		var descontinuado = Mouse with { Activo = false };
		await using var escenario = await new Escenario().ConProducto(descontinuado).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 1)));
	
		Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
		_ = escenario.Repositorio.DidNotReceive().Crear(Arg.Any<Pedido>());
	}

	[Fact]
    public async Task Crear_ConStockInsuficiente_Responde409ConElStockDisponible()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 11)));   // hay 10
	
		Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Contains("disponible 10", cuerpo.GetProperty("detail").GetString());
	}

	[Fact]
    public async Task Crear_ConStockExactamenteIgualAlDisponible_EsValido()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).IniciarAsync();	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 10)));
	
		Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
	}

    // Si UN producto falla, el pedido completo se rechaza: no se guarda nada ni se publica ningún evento.
	[Fact]
    public async Task Crear_SiUnProductoNoTieneStock_RechazaElPedidoCompleto()
    {
		await using var escenario = await new Escenario().ConProducto(Mouse).ConProducto(Teclado).IniciarAsync();
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 1), (2, 6)));   // teclado: hay 5
	
		Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
		_ = escenario.Repositorio.DidNotReceive().Crear(Arg.Any<Pedido>());
		Assert.Empty(escenario.EventosPublicados());
	}

    // ---------- Falla de infraestructura ----------
	[Fact]
    public async Task Crear_SiProductosNoEstaDisponible_Responde503YNoGuardaNada()
    {
		await using var escenario = await new Escenario().IniciarAsync();
		escenario.Productos.ObtenerProductoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
					.ThrowsAsync(new ServiceProductosNoDisponibleException("No disponible"));
	
		var respuesta = await escenario.Cliente.PostAsJsonAsync("/api/pedidos", CuerpoPedido("Ana", (1, 1)));
	
		Assert.Equal(HttpStatusCode.ServiceUnavailable, respuesta.StatusCode);
		_ = escenario.Repositorio.DidNotReceive().Crear(Arg.Any<Pedido>());
		Assert.Empty(escenario.EventosPublicados());
	}

    // ---------- Consultas ----------
	[Fact]
    public async Task ObtenerPorId_Existente_Responde200()
    {
		await using var escenario = await new Escenario().IniciarAsync();
		escenario.Repositorio.ObtenerPorId(7).Returns(Task.FromResult<Pedido?>(
		new Pedido { Id = 7, ClienteNombre = "Luis", Total = 100m }));
	
		var respuesta = await escenario.Cliente.GetAsync("/api/pedidos/7");
	
		Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
		var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
		Assert.Equal("Luis", cuerpo.GetProperty("clienteNombre").GetString());
	}

	[Fact]
    public async Task ObtenerPorId_Inexistente_Responde404()
    {
		await using var escenario = await new Escenario().IniciarAsync();
		escenario.Repositorio.ObtenerPorId(99).Returns(Task.FromResult<Pedido?>(null));
	
		var respuesta = await escenario.Cliente.GetAsync("/api/pedidos/99");
	
		Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
	}
}