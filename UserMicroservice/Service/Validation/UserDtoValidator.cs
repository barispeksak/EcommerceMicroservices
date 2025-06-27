using FluentValidation;
using UserMicroservice.Dtos;

namespace UserMicroservice.Service.Validation
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Fname)
                .NotEmpty().WithMessage("Ad boş olamaz.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

            RuleFor(x => x.Lname)
                .NotEmpty().WithMessage("Soyad boş olamaz.")
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş olamaz.")
                .EmailAddress().WithMessage("Geçersiz email adresi.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Telefon boş olamaz.")
                .Matches(@"^\d{9,15}$").WithMessage("Telefon 9-15 haneli rakamlardan oluşmalıdır.");

            RuleFor(x => x.Dob)
                .LessThan(DateTime.Today).WithMessage("Doğum tarihi bugünden küçük olmalı.");
        }
    }
}
