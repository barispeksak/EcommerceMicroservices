using FluentValidation;
using ShopOrderMicroservice.Data.Dtos;

namespace ShopOrderMicroservice.Data.Validators
{
    public class UpdateShopOrderValidator : AbstractValidator<UpdateShopOrderDto>
    {
        public UpdateShopOrderValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Güncelleme için geçerli bir Id belirtilmelidir.");
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçerli bir kullanıcı ID'si girilmelidir.");
            RuleFor(x => x.PaymentTypeId).GreaterThan(0).WithMessage("Geçerli bir ödeme tipi seçilmelidir.");
            RuleFor(x => x.ShippingAddressId).GreaterThan(0).WithMessage("Geçerli bir adres ID girilmelidir.");
            RuleFor(x => x.ShippingTypeId).GreaterThan(0).WithMessage("Geçerli bir kargo tipi ID girilmelidir.");
            RuleFor(x => x.OrderTotal).GreaterThan(0).WithMessage("Sipariş tutarı sıfırdan büyük olmalıdır.");
            RuleFor(x => x.OrderDate)
                .NotEmpty().WithMessage("Sipariş tarihi boş olamaz.");
        }
    }
}
