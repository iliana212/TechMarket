using FluentValidation;
using TechMarket_Productos.Data;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Validators
{
	public class ActualizaProductoValidator : AbstractValidator<ActualizarProductoDTO>
	{
		public ActualizaProductoValidator(ICategoriaRepositorio categorias)
		{
			RuleFor(c => c.Nombre)
				.NotEmpty().WithMessage("El nombre del producto es obligatorio")
				.MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres");

			RuleFor(c => c.Precio)
				.GreaterThan(0).WithMessage("El precio debe ser mayor a cero");

			RuleFor(c => c.Stock)
				.GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

			RuleFor(c => c.CategoriaId)
				.MustAsync(async (categoriaId, cancelacion) => await categorias.Existe(categoriaId))
				.WithMessage(p => $"No existe una categoria con Id {p.CategoriaId}");
		}
	}
}
