using BuildingBlocks.Abstractions;
using CatalogService.Infrastructure.Abstractions;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Commands.Products.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductCommandResponse>
{
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        ICatalogUnitOfWork unitOfWork,
        ILogger<CreateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateProductCommandResponse> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Criando produto com BaseSku: {BaseSku}", request.BaseSku);

            // Iniciar transação
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Verificar se já existe um produto com o mesmo BaseSku
            var existingProduct = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.BaseSku == request.BaseSku, cancellationToken);

            if (existingProduct != null)
            {
                return new CreateProductCommandResponse
                {
                    Success = false,
                    Message = $"Produto com BaseSku '{request.BaseSku}' já existe."
                };
            }

            // Verificar se a categoria existe
            var categoryExists = await _unitOfWork.Context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

            if (!categoryExists)
            {
                return new CreateProductCommandResponse
                {
                    Success = false,
                    Message = $"Categoria com ID '{request.CategoryId}' não encontrada."
                };
            }

            // Verificar se a marca existe
            if (request.BrandId.HasValue)
            {
                var brandExists = await _unitOfWork.Context.Brands
                    .AnyAsync(b => b.BrandId == request.BrandId.Value, cancellationToken);
                if (!brandExists)
                {
                    return new CreateProductCommandResponse
                    {
                        Success = false,
                        Message = "Marca não encontrada."
                    };
                }
            }

            try
            {
                // Criar a entidade Product
                var product = new Product
                {
                    ProductId = Guid.NewGuid(),
                    BaseSku = request.BaseSku,
                    Name = request.Name,
                    Slug = request.Slug,
                    Description = request.Description,
                    CategoryId = request.CategoryId,
                    BrandId = request.BrandId,
                    BasePrice = request.BasePrice,
                    SalePrice = request.SalePrice,
                    SalePriceStartDate = request.SalePriceStartDate,
                    SalePriceEndDate = request.SalePriceEndDate,
                    StockQuantity = request.StockQuantity,
                    IsActive = request.IsActive,
                    WeightKg = request.WeightKg,
                    HeightCm = request.HeightCm,
                    WidthCm = request.WidthCm,
                    DepthCm = request.DepthCm,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Version = 1
                };

                // Adicionar o produto ao contexto
                await _unitOfWork.Context.Products.AddAsync(product, cancellationToken);

                // Salvar as mudanças através do UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Confirmar transação
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Produto criado com sucesso. ProductId: {ProductId}", product.ProductId);

                return new CreateProductCommandResponse
                {
                    ProductId = product.ProductId,
                    Success = true,
                    Message = "Produto criado com sucesso",
                    BaseSku = product.BaseSku,
                    Name = product.Name,
                    Slug = product.Slug,
                    IsActive = product.IsActive,
                    CreatedAt = product.CreatedAt
                };
            }
            catch
            {
                // Rollback em caso de erro
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar produto no banco de dados. BaseSku: {BaseSku}", request.BaseSku);
            
            return new CreateProductCommandResponse
            {
                ProductId = Guid.Empty,
                Success = false,
                Message = "Erro ao salvar produto no banco de dados",
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar produto. BaseSku: {BaseSku}", request.BaseSku);
            
            return new CreateProductCommandResponse
            {
                ProductId = Guid.Empty,
                Success = false,
                Message = "Erro inesperado ao criar produto",
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}