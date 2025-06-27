using FluentValidation;
using Variation_OptionMicroservice.Service.DTOs;

namespace Variation_OptionMicroservice.Service.Validation
{
    public class UpdateVariationOptionDtoValidator : AbstractValidator<UpdateVariationOptionDto>
    {
        public UpdateVariationOptionDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID boş olamaz")
                .GreaterThan(0).WithMessage("ID 0'dan büyük olmalıdır");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Değer boş olamaz")
                .MaximumLength(100).WithMessage("Değer en fazla 100 karakter olabilir");

            RuleFor(x => x.VariationId)
                .NotEmpty().WithMessage("Varyasyon ID boş olamaz")
                .GreaterThan(0).WithMessage("Varyasyon ID 0'dan büyük olmalıdır");

            RuleFor(x => x.AdditionalInfo)
                .MaximumLength(200).WithMessage("Ek bilgi en fazla 200 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.AdditionalInfo));
        }
    }
}