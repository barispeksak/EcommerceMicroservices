using AuthMicroservice.Service.DTOs;
using FluentValidation;

namespace AuthMicroservice.Service.Validation
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir email giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");

            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.")
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));
        }
    }
}
