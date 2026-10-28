using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace TechMarket_Tests.Soporte;

public static class Entornos
{
    /// <summary>Entorno de ejecución simulado ("Development", "Production", ...).</summary>
    public static IHostEnvironment Crear(string nombre)
    {
        var entorno = Substitute.For<IHostEnvironment>();
        entorno.EnvironmentName.Returns(nombre);
        return entorno;
    }
}