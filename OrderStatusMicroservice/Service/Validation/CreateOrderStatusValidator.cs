using FluentValidation;
using OrderStatusMicroservice.Data.Dtos;

namespace OrderStatusMicroservice.Validators
{
    public class CreateOrderStatusValidator : AbstractValidator<CreateOrderStatusDto>
    {
        public CreateOrderStatusValidator()
        {
            RuleFor(x => x.ShopOrderId).NotEmpty().WithMessage("Sipariş ID'si boş olamaz.");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Sipariş durumu boş olamaz.");
            RuleFor(x => x.City).NotEmpty().WithMessage("Siparişin bulunduğu boş olamaz.");
        }
    }
}
