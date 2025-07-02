using FluentValidation;
using ShoppingCartMicroservice_Service.DTOs;

namespace ShoppingCartMicroservice_Service.Validation
{
    public class CreateShoppingCartDtoValidator : AbstractValidator<CreateShoppingCartDto>
    {
        public CreateShoppingCartDtoValidator()
        {
            RuleFor(x => x.ProductItemId).GreaterThan(0).WithMessage("Ürün ID sıfırdan büyük olmalıdır.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Adet en az 1 olmalı.");        }
    }
}
