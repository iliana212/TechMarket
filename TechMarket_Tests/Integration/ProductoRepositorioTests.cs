using Microsoft.EntityFrameworkCore;
using TechMarket_Productos.Data;
using TechMarket_Productos.Models;
using TechMarket_Tests.Soporte;

namespace TechMarket_Tests.Integration;

[Collection("Postgres")]
public class ProductoRepositorioTests(PostgresFixture postgres)
{
    // Fixture: una categoría y un producto reales en la base de datos, con nombres únicos
    // (todas las pruebas comparten la misma base, así que ninguna depende de los datos de otra).
    private async Task<int> CrearProductoAsync(int stock)
    {
        await using var contexto = postgres.CrearContextoProductos();
        var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}" };
        contexto.Categorias.Add(categoria);
        await contexto.SaveChangesAsync();

        var producto = await new ProductoRepositorio(contexto).Crear(new Producto
        {
			Nombre = $"Prod-{Guid.NewGuid():N}",
			CategoriaId = categoria.Id,
			Precio = 100m,
			Stock = stock
        });

        return producto.Id;
    }

    private async Task<int> LeerStockAsync(int productoId)
    {
		await using var contexto = postgres.CrearContextoProductos();
			return await contexto.Productos.AsNoTracking()
	        .Where(p => p.Id == productoId)
	        .Select(p => p.Stock)
	        .SingleAsync();
	}

    private async Task<bool> DescontarAsync(int productoId, int cantidad)
    {
		await using var contexto = postgres.CrearContextoProductos();
	        return await new ProductoRepositorio(contexto).DescontarStock(productoId, cantidad);
	}

    // ---------- Crear / consultar ----------
	[Fact]
    public async Task Crear_DevuelveElProductoConSuCategoriaCargada()
    {
		await using var contexto = postgres.CrearContextoProductos();
		var categoria = new Categoria { Nombre = $"Cat-{Guid.NewGuid():N}" };
		contexto.Categorias.Add(categoria);
		await contexto.SaveChangesAsync();
	
		var creado = await new ProductoRepositorio(contexto).Crear(
		new Producto { Nombre = $"Prod-{Guid.NewGuid():N}", CategoriaId = categoria.Id, Precio = 10m, Stock = 1 });
	
		Assert.True(creado.Id > 0);
		Assert.NotNull(creado.Categoria);
		Assert.Equal(categoria.Nombre, creado.Categoria!.Nombre);
	}

	[Fact]
    public async Task ObtenerPorId_IncluyeLaCategoria()
    {
		var id = await CrearProductoAsync(stock: 5);
	
		await using var contexto = postgres.CrearContextoProductos();
		var producto = await new ProductoRepositorio(contexto).ObtenerPorId(id);
	
		Assert.NotNull(producto);
		Assert.NotNull(producto!.Categoria);
	}

	[Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNull()
    {
		await using var contexto = postgres.CrearContextoProductos();	
		var producto = await new ProductoRepositorio(contexto).ObtenerPorId(999_999_999);	
		Assert.Null(producto);
	}

    // ---------- Descuento de stock ----------
	[Fact]
    public async Task DescontarStock_ConStockSuficiente_RestaYDevuelveTrue()
    {
		var id = await CrearProductoAsync(stock: 10);	
		var resultado = await DescontarAsync(id, 3);

		Assert.True(resultado);
		Assert.Equal(7, await LeerStockAsync(id));
	}

	[Fact]
    public async Task DescontarStock_ExactamenteElStockDisponible_LlegaACero()
    {
		var id = await CrearProductoAsync(stock: 4);	
		var resultado = await DescontarAsync(id, 4);	

		Assert.True(resultado);
		Assert.Equal(0, await LeerStockAsync(id));
	}

	[Fact]
    public async Task DescontarStock_ConStockInsuficiente_NoCambiaNadaYDevuelveFalse()
    {
		var id = await CrearProductoAsync(stock: 2);	
		var resultado = await DescontarAsync(id, 3);

		Assert.False(resultado);
		Assert.Equal(2, await LeerStockAsync(id));
	}

	[Fact]
    public async Task DescontarStock_ProductoInexistente_DevuelveFalse()
    {
		var resultado = await DescontarAsync(999_999_999, 1);	
		Assert.False(resultado);
	}

    // ---------- Concurrencia ----------
    // Con el patrón anterior (leer el stock, restar en C# y guardar) dos descuentos simultáneos podían leer el
    // mismo valor de partida y pisarse. Con un UPDATE ... WHERE Stock >= @cantidad atómico, PostgreSQL los serializa.
	[Fact]
    public async Task DescontarStock_VeinteDescuentosSimultaneosSobreDiezUnidades_ExactamenteDiezTienenExito()
    {
		var id = await CrearProductoAsync(stock: 10);
	
		var intentos = Enumerable.Range(0, 20).Select(_ => DescontarAsync(id, 1));
		var resultados = await Task.WhenAll(intentos);
	
		Assert.Equal(10, resultados.Count(exito => exito));
		Assert.Equal(10, resultados.Count(exito => !exito));
		Assert.Equal(0, await LeerStockAsync(id));
	}

	[Fact]
    public async Task DescontarStock_ConCantidadesDistintasEnParalelo_ElStockNuncaQuedaNegativoNiInconsistente()
    {
		var id = await CrearProductoAsync(stock: 10);
		var cantidades = new[] { 3, 4, 5, 6, 2, 7 };
	
		var intentos = cantidades.Select(async cantidad => (Cantidad: cantidad, Exito: await DescontarAsync(id, cantidad)));
		var resultados = await Task.WhenAll(intentos);
	
		var descontado = resultados.Where(r => r.Exito).Sum(r => r.Cantidad);
		var stockFinal = await LeerStockAsync(id);
	
		Assert.True(stockFinal >= 0, $"El stock quedó negativo: {stockFinal}");
		Assert.Equal(10 - descontado, stockFinal);   // lo que bajó es exactamente lo que se confirmó
	}
}