using FluentValidation;
using PaymentTypeMicroservice.Data.Dtos;

namespace PaymentTypeMicroservice.Validators
{
    public class CreatePaymentTypeValidator : AbstractValidator<CreatePaymentTypeDto>
    {
        public CreatePaymentTypeValidator()
        {
            RuleFor(x => x.Type).NotEmpty().WithMessage("Kargo tipi boş olamaz.");
        }
    }
}
