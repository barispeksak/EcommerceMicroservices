using FluentValidation;
using ProductCategoryMicroservice_Service.DTOs;

namespace ProductCategoryMicroservice_Service.Validation;

/// <summary>
/// Yeni kategori eklerken gelen alanların kuralları:
/// • CategoryName zorunlu, 100 karakteri geçemez
/// • ParentCategoryId pozitif olmalı (null ise kök kategoridir)
/// </summary>
public sealed class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(c => c.CategoryName)
            .NotEmpty().WithMessage("Kategori adı gereklidir.")
            .MaximumLength(100).WithMessage("Kategori adı 100 karakterden uzun olamaz.");

        RuleFor(c => c.ParentCategoryId)
            .Must(id => id == null || id > 0)
            .WithMessage("ParentCategoryId pozitif bir sayı olmalıdır.");
    }
}
