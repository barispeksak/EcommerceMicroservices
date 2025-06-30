using FluentValidation;
using ShippingTypeMicroservice.Data.Dtos;

namespace ShippingTypeMicroservice.Validators
{
    public class UpdateShippingTypeValidator : AbstractValidator<UpdateShippingTypeDto>
    {
        public UpdateShippingTypeValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçerli bir ID giriniz.");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Kargo tipi boş olamaz.");
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }
}
