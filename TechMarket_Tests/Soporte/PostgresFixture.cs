using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using PedidosDb = TechMarket_Pedidos.Data.AppDbContext;
using ProductosDb = TechMarket_Productos.Data.AppDbContext;

namespace TechMarket_Tests.Soporte;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _contenedor = new PostgreSqlBuilder("postgres:16-alpine").Build();

    public async Task InitializeAsync()
    {
        await _contenedor.StartAsync();

        // Aplica las migraciones reales: así también se comprueba que el esquema se crea bien.
        await using (var productos = CrearContextoProductos())
            await productos.Database.MigrateAsync();

		await using var pedidos = CrearContextoPedidos();
		    await pedidos.Database.MigrateAsync();
	}

    public Task DisposeAsync() => _contenedor.DisposeAsync().AsTask();

    public ProductosDb CrearContextoProductos() =>
        new(new DbContextOptionsBuilder<ProductosDb>().UseNpgsql(CadenaConexion("productos_test")).Options);

    public PedidosDb CrearContextoPedidos() =>
        new(new DbContextOptionsBuilder<PedidosDb>().UseNpgsql(CadenaConexion("pedidos_test")).Options);

    private string CadenaConexion(string baseDeDatos) =>
        new NpgsqlConnectionStringBuilder(_contenedor.GetConnectionString()) { Database = baseDeDatos }.ConnectionString;
}


[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
}