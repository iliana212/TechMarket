using FluentValidation;
using ImTools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TechMarket_Productos.Auth;
using TechMarket_Productos.Data;
using TechMarket_Productos.Endpoints;
using TechMarket_Productos.Middleware;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseWolverine(opt =>
{
	opt.Discovery.IncludeAssembly(typeof(Program).Assembly);
	opt.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();
});

var conexion = builder.Configuration.GetConnectionString("TechMarketDb")
	?? throw new InvalidOperationException("Falta cadena de conexion");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conexion));

builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddSingleton<IEmisorTokenJWT, EmisorTokenJWT>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opts =>
{
	opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwt["Issuer"],
		ValidAudience = jwt["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
	};
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
	{
		Title = "TechMarket - Microservicios de Productos",
		Version = "v1",
		Description = "Curso de Microservicios con .Net y Azure"
	});

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Pega el token devuelto por /api/auth/login (sin palabra Bearer)"
	});

	options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
	{
		[new OpenApiSecuritySchemeReference("Bearer", document)] = []
	});

});

builder.Services.AddAuthorization(option =>
{
	option.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "Productos API");
	});
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
	servicio = "ms-productos",
	estado = "Ok",
	mensaje = "TechMarket - Microservicios funcionando"
}));

app.MapProductosEndpoints();
app.MapCategoriasEndpoints();
app.MapAuthEndpoints();

app.Run();
