using BuildingBlocks.Abstractions;
using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Mediator;

namespace CatalogService.Application.Commands.Products.UpdateProductCommand;

public class UpdateProductCommand : IRequest<Result<UpdateProductCommandResponse>>
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string BaseSku { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public Guid? BrandId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "O preço base deve ser maior ou igual a 0")]
    public decimal BasePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço promocional deve ser maior ou igual a 0")]
    public decimal? SalePrice { get; set; }

    public DateTimeOffset? SalePriceStartDate { get; set; }

    public DateTimeOffset? SalePriceEndDate { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade em estoque deve ser maior ou igual a 0")]
    public int StockQuantity { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [Range(0.001, double.MaxValue, ErrorMessage = "O peso deve ser maior que 0")]
    public decimal? WeightKg { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A altura deve ser maior que 0")]
    public int? HeightCm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A largura deve ser maior que 0")]
    public int? WidthCm { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A profundidade deve ser maior que 0")]
    public int? DepthCm { get; set; }
}