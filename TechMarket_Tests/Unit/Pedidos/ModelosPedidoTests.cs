using TechMarket_Pedidos.Models;

namespace TechMarket_Tests.Unit.Pedidos;

public class ModelosPedidoTests
{
    [Fact]
    public void Subtotal_EsCantidadPorPrecioUnitario()
    {
        var item = new ItemPedido { Cantidad = 3, PrecioUnitario = 19.99m };
        Assert.Equal(59.97m, item.Subtotal);
    }

    [Fact]
    public void PedidoNuevo_NaceConfirmadoConFechaUtcYSinItems()
    {
	    var pedido = new Pedido();
	
	    Assert.Equal("Confirmado", pedido.Estado);
	    Assert.Equal(DateTimeKind.Utc, pedido.Fecha.Kind);
	    Assert.Empty(pedido.Items);
	}
}