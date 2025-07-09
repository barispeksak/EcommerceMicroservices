using FluentValidation;
using ProductMicroservice_Service.DTOs;

namespace ProductMicroservice_Service.Validation;

/// <summary>
/// PUT isteklerinde gelen UpdateProductDto’nun opsiyonel alanlarını kontrol eder.
/// Null gelen alanlar değiştirilmeyeceği için kurallar sadece dolu alanlarda çalışır.
/// </summary>
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        When(x => x.CategoryId.HasValue, () =>
        {
            RuleFor(x => x.CategoryId!.Value)
                .GreaterThan(0).WithMessage("CategoryId pozitif olmalı");
        });

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!)
                .Length(3, 80).WithMessage("Ürün adı 3-80 karakter olmalı");
        });

        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description!)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter");
        });

        When(x => x.Image is not null, () =>
        {
            RuleFor(x => x.Image!)
                .Must(i => Uri.TryCreate(i, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL gir");
        });

        When(x => x.Brand is not null, () =>
        {
            RuleFor(x => x.Brand!)
                .Length(2, 40).WithMessage("Marka 2-40 karakter olmalı");
        });
    }
}
