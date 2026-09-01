var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
	servicio = "ms-gateway",
	estado = "OK",
	mensaje = "TechMarket - API Gateway - puerta de entrada única a productos y pedidos"
}));

app.MapReverseProxy();

app.Run();
