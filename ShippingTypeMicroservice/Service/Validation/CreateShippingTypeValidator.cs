using FluentValidation;
using ShippingTypeMicroservice.Data.Dtos;

namespace ShippingTypeMicroservice.Validators
{
    public class CreateShippingTypeValidator : AbstractValidator<CreateShippingTypeDto>
    {
        public CreateShippingTypeValidator()
        {
            RuleFor(x => x.Type).NotEmpty().WithMessage("Kargo tipi boş olamaz.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Fiyat pozitif olmalıdır.");  // ✅
        }
    }
}
