using FluentValidation;
using VariationMicroservice.Service.DTOs;

namespace VariationMicroservice.Service.Validation
{
    public class CreateVariationDtoValidator : AbstractValidator<CreateVariationDto>
    {
        public CreateVariationDtoValidator()
        {
            RuleFor(x => x.VarTypeName)
                .NotEmpty().WithMessage("Varyasyon tipi adı boş olamaz")
                .MaximumLength(50).WithMessage("Varyasyon tipi adı en fazla 50 karakter olabilir");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Kategori ID boş olamaz")
                .GreaterThan(0).WithMessage("Kategori ID 0'dan büyük olmalıdır");
        }
    }
}