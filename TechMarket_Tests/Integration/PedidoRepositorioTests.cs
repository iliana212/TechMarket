using TechMarket_Pedidos.Data;
using TechMarket_Pedidos.Models;
using TechMarket_Tests.Soporte;

namespace TechMarket_Tests.Integration;

[Collection("Postgres")]
public class PedidoRepositorioTests(PostgresFixture postgres)
{
    private static Pedido NuevoPedido(string cliente)
    {
        var pedido = new Pedido { ClienteNombre = cliente, Total = 500m };
        pedido.Items.Add(new ItemPedido { ProductoId = 1, ProductoNombre = "Mouse", Cantidad = 2, PrecioUnitario = 250m });
        return pedido;
    }

	[Fact]
    public async Task Crear_PersisteElPedidoConSusItemsYGeneraLosIds()
    {
		await using var escritura = postgres.CrearContextoPedidos();	
		var creado = await new PedidoRepositorio(escritura).Crear(NuevoPedido("Ana"));	
		Assert.True(creado.Id > 0);
	
	    // Se lee con un contexto distinto para comprobar lo que realmente quedó en la base de datos.
		await using var lectura = postgres.CrearContextoPedidos();
		var leido = await new PedidoRepositorio(lectura).ObtenerPorId(creado.Id);
	
		Assert.NotNull(leido);
		Assert.Equal("Ana", leido!.ClienteNombre);
		Assert.Equal("Confirmado", leido.Estado);
		Assert.Equal(500m, leido.Total);
	
		var item = Assert.Single(leido.Items);
		Assert.Equal(creado.Id, item.PedidoId);
		Assert.Equal("Mouse", item.ProductoNombre);
		Assert.Equal(500m, item.Subtotal);   // Subtotal se calcula en memoria, no es una columna
	}

	[Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNull()
    {
		await using var contexto = postgres.CrearContextoPedidos();	
		var pedido = await new PedidoRepositorio(contexto).ObtenerPorId(999_999_999);	
		Assert.Null(pedido);
	}

	[Fact]
    public async Task ObtenerTodos_IncluyeLosPedidosCreadosConSusItems()
    {
		await using var escritura = postgres.CrearContextoPedidos();
		var creado = await new PedidoRepositorio(escritura).Crear(NuevoPedido($"Cliente-{Guid.NewGuid():N}"));
	
		await using var lectura = postgres.CrearContextoPedidos();
		var todos = await new PedidoRepositorio(lectura).ObtenerTodos();
	
		var encontrado = Assert.Single(todos, p => p.Id == creado.Id);
		Assert.Single(encontrado.Items);
	}
}