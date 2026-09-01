using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using TechMarket_Pedidos.Clients;
using TechMarket_Pedidos.Data;
using TechMarket_Pedidos.Endpoints;
using TechMarket_Pedidos.Middleware;

var builder = WebApplication.CreateBuilder(args);

var urlProductos = builder.Configuration["Servicios:Productos:BaseUrl"] ?? throw new InvalidOperationException("Falta de Url para conectar ");

var conexion = builder.Configuration.GetConnectionString("TechMarketPedidos")
   ?? throw new InvalidOperationException("Falta cadena de conexion");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conexion));

builder.Services.AddHttpClient<IProductosClient, ProductosClient>(cliente =>
{
	cliente.BaseAddress = new Uri(urlProductos);
}).AddResilienceHandler("Producto-pipeline", pipeline =>
{
	pipeline.AddRetry(new HttpRetryStrategyOptions
	{
		MaxRetryAttempts = 3,
		BackoffType = DelayBackoffType.Exponential,
		Delay = TimeSpan.FromMilliseconds(300),
		UseJitter = true
	});

	pipeline.AddTimeout(TimeSpan.FromSeconds(2));

	pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
	{
		FailureRatio = 0.5,
		MinimumThroughput = 4,
		SamplingDuration = TimeSpan.FromSeconds(10),
		BreakDuration = TimeSpan.FromSeconds(15)
	});
});

// Add services to the container.

builder.Services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
	{
		Title = "TechMarket - Microservicios de Pedidos",
		Version = "v1",
		Description = "Curso de Microservicios con .Net y Azure"
	});
});


var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pedidos API");
	});
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
	servicio = "ms-pedidos",
	estado = "OK",
	mensaje = "TechMarket - Microservicio de pedidos",
	dependeDe = urlProductos
}));

app.MapPedidosEndpoints();

app.Run();
