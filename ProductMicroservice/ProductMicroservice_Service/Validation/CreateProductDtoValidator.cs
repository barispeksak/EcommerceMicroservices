using FluentValidation;
using ProductMicroservice.Service.DTOs;

namespace ProductMicroservice.Service.Validation;

/// <summary>
/// POST isteklerinde gelen CreateProductDto’nun kurallarını kontrol eder.
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId pozitif olmalı");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunlu")
            .Length(3, 80).WithMessage("Ürün adı 3-80 karakter olmalı");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama zorunlu")
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Görsel URL zorunlu")
            .Must(i => Uri.TryCreate(i, UriKind.Absolute, out _))
            .WithMessage("Geçerli bir URL gir");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Marka zorunlu")
            .Length(2, 40).WithMessage("Marka 2-40 karakter olmalı");
    }
}
