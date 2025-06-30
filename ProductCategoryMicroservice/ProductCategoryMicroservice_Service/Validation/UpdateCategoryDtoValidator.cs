using FluentValidation;
using ProductCategoryMicroservice_Service.DTOs;

namespace ProductCategoryMicroservice_Service.Validation;

/// <summary>
/// Güncelleme sırasında boş bırakılan alanlar değiştirilmez.
/// Gönderilen (null olmayan) alanlar aynı Create kuralı ile doğrulanır.
/// </summary>
public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        When(c => c.CategoryName is not null, () =>
        {
            RuleFor(c => c.CategoryName!)
                .NotEmpty().WithMessage("Kategori adı boş olamaz.")
                .MaximumLength(100).WithMessage("Kategori adı 100 karakterden uzun olamaz.");
        });

        When(c => c.ParentCategoryId is not null, () =>
        {
            RuleFor(c => c.ParentCategoryId!.Value)
                .GreaterThan(0).WithMessage("ParentCategoryId pozitif bir sayı olmalıdır.");
        });
    }
}
