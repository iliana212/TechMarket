using FluentValidation;
using TechMarket_Productos.Models;

namespace TechMarket_Productos.Validators
{
	public class CategoriaValidator : AbstractValidator<CrearCategoriaDTO>
	{
		public CategoriaValidator() 
		{
			RuleFor(c => c.Nombre)
				.NotEmpty().WithMessage("El nombre de la categoría es obligatorio")
				.MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres");

			RuleFor(c => c.Descripcion)
				.MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres");
		}
	}
}
