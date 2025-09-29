using FluentValidation;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Commands.Categories.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly CatalogDbContext _context;

    public CreateCategoryCommandValidator(CatalogDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da categoria é obrigatório")
            .MaximumLength(100).WithMessage("Nome da categoria deve ter no máximo 100 caracteres");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug da categoria é obrigatório")
            .MaximumLength(150).WithMessage("Slug da categoria deve ter no máximo 150 caracteres")
            .MustAsync(BeUniqueSlug).WithMessage("Já existe uma categoria com este slug");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ParentCategoryId)
            .MustAsync(ParentCategoryExists).WithMessage("Categoria pai não encontrada")
            .When(x => x.ParentCategoryId.HasValue);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Ordem de classificação deve ser maior ou igual a 0");
    }

    private async Task<bool> BeUniqueSlug(string slug, CancellationToken cancellationToken)
    {
        return !await _context.Categories
            .AnyAsync(c => c.Slug == slug && c.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> ParentCategoryExists(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        if (!parentCategoryId.HasValue)
            return true;

        return await _context.Categories
            .AnyAsync(c => c.CategoryId == parentCategoryId.Value && c.DeletedAt == null, cancellationToken);
    }
}