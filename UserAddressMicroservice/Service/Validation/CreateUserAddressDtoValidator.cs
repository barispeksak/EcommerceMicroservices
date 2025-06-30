using FluentValidation;
using UserAddressMicroservice.Data.Dtos;

namespace UserAddressMicroservice.Service.Validation
{
    public class CreateUserAddressDtoValidator : AbstractValidator<CreateUserAddressDto>
    {
        public CreateUserAddressDtoValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.AddressId).GreaterThan(0);
        }
    } 
}

