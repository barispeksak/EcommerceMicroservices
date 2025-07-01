using FluentValidation;
using OrderStatusMicroservice.Data.Dtos;

namespace OrderStatusMicroservice.Validators
{
    public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçerli bir ID giriniz.");
            RuleFor(x => x.ShopOrderId).GreaterThan(0).WithMessage("Geçerli bir Sipariş ID'si giriniz.");
            RuleFor(x => x.Status).NotEmpty().WithMessage("Sipariş durumu boş olamaz.");
            RuleFor(x => x.City).NotEmpty().WithMessage("Siparişin bulunduğu boş olamaz.");
        }
    }
}

