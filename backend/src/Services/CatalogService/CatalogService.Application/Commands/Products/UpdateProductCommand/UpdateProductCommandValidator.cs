using FluentValidation;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Commands.Products.UpdateProductCommand;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private readonly CatalogDbContext _context;

    public UpdateProductCommandValidator(CatalogDbContext context)
    {
        _context = context;

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId é obrigatório");

        RuleFor(x => x.BaseSku)
            .NotEmpty().WithMessage("BaseSku é obrigatório")
            .MaximumLength(50).WithMessage("BaseSku deve ter no máximo 50 caracteres")
            .MustAsync(BeUniqueBaseSku).WithMessage("BaseSku já existe");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug é obrigatório")
            .MaximumLength(200).WithMessage("Slug deve ter no máximo 200 caracteres")
            .MustAsync(BeUniqueSlug).WithMessage("Slug já existe");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId é obrigatório")
            .MustAsync(CategoryExists).WithMessage("Categoria não encontrada");

        When(x => x.BrandId.HasValue, () => {
            RuleFor(x => x.BrandId!.Value)
                .MustAsync(BrandExists).WithMessage("Marca não encontrada");
        });

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Preço base deve ser maior ou igual a 0");

        When(x => x.SalePrice.HasValue, () => {
            RuleFor(x => x.SalePrice!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Preço promocional deve ser maior ou igual a 0")
                .LessThan(x => x.BasePrice).WithMessage("Preço promocional deve ser menor que o preço base");
        });

        When(x => x.SalePrice.HasValue, () => {
            RuleFor(x => x.SalePriceStartDate)
                .NotNull().WithMessage("Data de início da promoção é obrigatória quando há preço promocional");

            RuleFor(x => x.SalePriceEndDate)
                .NotNull().WithMessage("Data de fim da promoção é obrigatória quando há preço promocional")
                .GreaterThan(x => x.SalePriceStartDate).WithMessage("Data de fim deve ser posterior à data de início");
        });

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque deve ser maior ou igual a 0");

        When(x => x.WeightKg.HasValue, () => {
            RuleFor(x => x.WeightKg!.Value)
                .GreaterThan(0).WithMessage("Peso deve ser maior que 0");
        });

        When(x => x.HeightCm.HasValue, () => {
            RuleFor(x => x.HeightCm!.Value)
                .GreaterThan(0).WithMessage("Altura deve ser maior que 0");
        });

        When(x => x.WidthCm.HasValue, () => {
            RuleFor(x => x.WidthCm!.Value)
                .GreaterThan(0).WithMessage("Largura deve ser maior que 0");
        });

        When(x => x.DepthCm.HasValue, () => {
            RuleFor(x => x.DepthCm!.Value)
                .GreaterThan(0).WithMessage("Profundidade deve ser maior que 0");
        });
    }

    private async Task<bool> BeUniqueBaseSku(UpdateProductCommand command, string baseSku, CancellationToken cancellationToken)
    {
        return !await _context.Products
            .AnyAsync(p => p.BaseSku == baseSku && p.ProductId != command.ProductId && p.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> BeUniqueSlug(UpdateProductCommand command, string slug, CancellationToken cancellationToken)
    {
        return !await _context.Products
            .AnyAsync(p => p.Slug == slug && p.ProductId != command.ProductId && p.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> CategoryExists(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AnyAsync(c => c.CategoryId == categoryId && c.DeletedAt == null, cancellationToken);
    }

    private async Task<bool> BrandExists(Guid brandId, CancellationToken cancellationToken)
    {
        return await _context.Brands
            .AnyAsync(b => b.BrandId == brandId && b.DeletedAt == null, cancellationToken);
    }
}