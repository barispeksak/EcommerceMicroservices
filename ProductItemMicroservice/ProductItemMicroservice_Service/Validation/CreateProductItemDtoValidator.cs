using FluentValidation;
using ProductItemMicroservice_Service.DTOs;
using System.Text.RegularExpressions;

namespace ProductItemMicroservice_Service.Validation
{
    public class CreateProductItemDtoValidator : AbstractValidator<CreateProductItemDto>
    {
        public CreateProductItemDtoValidator()
        {
            // SKU: Zorunlu, max 30 karakter, sadece büyük harf, rakam ve tire
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU alanı zorunludur.")
                .MaximumLength(30).WithMessage("SKU en fazla 30 karakter olabilir.")
                .Matches("^[A-Z0-9\\-]+$").WithMessage("SKU sadece büyük harf, rakam ve tire içerebilir. (Ör: 12345-KIRMIZI-M)");

            // Quantity: 0 veya üstü
            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0).WithMessage("Stok adedi negatif olamaz.");

            // Price: 0'dan büyük
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalı.");

            // Currency: TRY, USD, EUR (case-sensitive)
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Para birimi zorunludur.")
                .Must(IsValidCurrency).WithMessage("Para birimi sadece 'TRY', 'USD' veya 'EUR' olabilir.");

            // ProductId: pozitif
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId pozitif olmalı.");
        }

        // Para birimi kontrolü (büyük harf)
        private bool IsValidCurrency(string currency)
        {
            return currency == "TRY" || currency == "USD" || currency == "EUR";
        }
    }
}
