using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechMarket_Productos.Data;
using TechMarket_Productos.Endpoints;
using TechMarket_Productos.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var conexion = builder.Configuration.GetConnectionString("TechMarketDb")
	?? throw new InvalidOperationException("Falta cadena de conexion");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conexion));

builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
	{
		Title = "TechMarket - Microservicios de Productos",
		Version = "v1",
		Description = "Curso de Microservicios con .Net y Azure"
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "Productos API");
	});
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
	servicio = "ms-productos",
	estado = "Ok",
	mensaje = "TechMarket - Microservicios funcionando"
}));

app.MapProductosEndpoints();
app.MapCategoriasEndpoints();

app.Run();
