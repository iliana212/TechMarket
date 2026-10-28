using NSubstitute;
using TechMarket_Productos.Data;
using TechMarket_Productos.Models;
using TechMarket_Productos.Validators;

namespace TechMarket_Tests.Unit.Productos;

public class ValidadoresProductoTests
{
    private static ICategoriaRepositorio Categorias(bool existe = true)
    {
        var categorias = Substitute.For<ICategoriaRepositorio>();
        categorias.Existe(Arg.Any<int>()).Returns(existe);
        return categorias;
    }

    // ---------- Crear ----------
	[Fact]
    public async Task Crear_ConDatosValidos_PasaLaValidacion()
    {
		var validador = new ProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new CrearProductoDTO("Mouse", 1, 250m, 10));
	
		Assert.True(resultado.IsValid);
	}

	[Theory]
	[InlineData("", 10.0, 1, "Nombre")]       // nombre vacío
	[InlineData("   ", 10.0, 1, "Nombre")]    // solo espacios
	[InlineData("Mouse", 0.0, 1, "Precio")]   // precio en cero
	[InlineData("Mouse", -5.0, 1, "Precio")]  // precio negativo
	[InlineData("Mouse", 10.0, -1, "Stock")]  // stock negativo
    public async Task Crear_ConDatosInvalidos_FallaEnElCampoCorrecto(string nombre, double precio, int stock, string campo)
    {
		var validador = new ProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new CrearProductoDTO(nombre, 1, (decimal)precio, stock));
	
		Assert.False(resultado.IsValid);
		Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
	}

	[Fact]
    public async Task Crear_ConStockEnCero_EsValido()
    {
		var validador = new ProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new CrearProductoDTO("Mouse", 1, 10m, 0));
	
		Assert.True(resultado.IsValid);
	}

	[Fact]
    public async Task Crear_ConCategoriaInexistente_FallaIndicandoElId()
    {
		var validador = new ProductoValidator(Categorias(existe: false));	
		var resultado = await validador.ValidateAsync(new CrearProductoDTO("Mouse", 999, 10m, 1));
	
		var error = Assert.Single(resultado.Errors);
		Assert.Equal("CategoriaId", error.PropertyName);
		Assert.Contains("999", error.ErrorMessage);
	}

    // ---------- Longitud del nombre ----------
	[Theory]
	[InlineData(100, true)]    // límite exacto
	[InlineData(101, false)]   // un carácter de más
    public async Task Crear_ConLongitudDeNombreEnElLimite(int longitud, bool esValido)
    {
		var validador = new ProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new CrearProductoDTO(new string('A', longitud), 1, 10m, 1));
	
		Assert.Equal(esValido, resultado.IsValid);
	}

	[Theory]
	[InlineData(100, true)]
	[InlineData(101, false)]
    public async Task Actualizar_ConLongitudDeNombreEnElLimite(int longitud, bool esValido)
    {
		var validador = new ActualizaProductoValidator(Categorias());
	
		var resultado = await validador.ValidateAsync(
		new ActualizarProductoDTO(new string('A', longitud), 1, 10m, 1, true));
	
		Assert.Equal(esValido, resultado.IsValid);
	}

    // ---------- Actualizar ----------
	[Fact]
    public async Task Actualizar_ConDatosValidos_PasaLaValidacion()
    {
		var validador = new ActualizaProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new ActualizarProductoDTO("Mouse", 1, 250m, 10, false));
	
		Assert.True(resultado.IsValid);
	}

	[Fact]
    public async Task Actualizar_ConPrecioYStockInvalidos_ReportaAmbosErrores()
    {
		var validador = new ActualizaProductoValidator(Categorias());	
		var resultado = await validador.ValidateAsync(new ActualizarProductoDTO("Mouse", 1, 0m, -1, true));
	
		Assert.Contains(resultado.Errors, e => e.PropertyName == "Precio");
		Assert.Contains(resultado.Errors, e => e.PropertyName == "Stock");
	}
}