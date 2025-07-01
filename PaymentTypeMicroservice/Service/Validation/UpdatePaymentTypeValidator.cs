using FluentValidation;
using PaymentTypeMicroservice.Data.Dtos;

namespace PaymentTypeMicroservice.Validators
{
    public class UpdatePaymentTypeValidator : AbstractValidator<UpdatePaymentTypeDto>
    {
        public UpdatePaymentTypeValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçerli bir ID giriniz.");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Kargo tipi boş olamaz.");
        }
    }
}
