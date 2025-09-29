using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using CatalogService.Infrastructure.Abstractions;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Commands.Products.UpdateProductCommand;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<UpdateProductCommandResponse>>
{
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(
        ICatalogUnitOfWork unitOfWork,
        ILogger<UpdateProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UpdateProductCommandResponse>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Atualizando produto com ProductId: {ProductId}", request.ProductId);

            // Iniciar transação
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Buscar o produto existente
            var existingProduct = await _unitOfWork.Context.Products
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && p.DeletedAt == null, cancellationToken);

            if (existingProduct == null)
            {
                _logger.LogWarning("Produto não encontrado. ProductId: {ProductId}", request.ProductId);
                return Result<UpdateProductCommandResponse>.Failure("Produto não encontrado.");
            }

            // Verificar se a categoria existe
            var categoryExists = await _unitOfWork.Context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.DeletedAt == null, cancellationToken);
            if (!categoryExists)
            {
                return Result<UpdateProductCommandResponse>.Failure("Categoria não encontrada.");
            }

            // Verificar se a marca existe (se fornecida)
            if (request.BrandId.HasValue)
            {
                var brandExists = await _unitOfWork.Context.Brands
                    .AnyAsync(b => b.BrandId == request.BrandId.Value && b.DeletedAt == null, cancellationToken);
                if (!brandExists)
                {
                    return Result<UpdateProductCommandResponse>.Failure("Marca não encontrada.");
                }
            }

            try
            {
                // Atualizar as propriedades do produto
                existingProduct.BaseSku = request.BaseSku;
                existingProduct.Name = request.Name;
                existingProduct.Slug = request.Slug;
                existingProduct.Description = request.Description;
                existingProduct.CategoryId = request.CategoryId;
                existingProduct.BrandId = request.BrandId;
                existingProduct.BasePrice = request.BasePrice;
                existingProduct.SalePrice = request.SalePrice;
                existingProduct.SalePriceStartDate = request.SalePriceStartDate;
                existingProduct.SalePriceEndDate = request.SalePriceEndDate;
                existingProduct.StockQuantity = request.StockQuantity;
                existingProduct.IsActive = request.IsActive;
                existingProduct.WeightKg = request.WeightKg;
                existingProduct.HeightCm = request.HeightCm;
                existingProduct.WidthCm = request.WidthCm;
                existingProduct.DepthCm = request.DepthCm;
                
                // Atualizar campos de controle
                existingProduct.UpdatedAt = DateTimeOffset.UtcNow;
                existingProduct.Version += 1;

                // Salvar as mudanças através do UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Confirmar transação
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Produto atualizado com sucesso. ProductId: {ProductId}, Version: {Version}", 
                    existingProduct.ProductId, existingProduct.Version);

                var response = new UpdateProductCommandResponse
                {
                    ProductId = existingProduct.ProductId,
                    BaseSku = existingProduct.BaseSku,
                    Name = existingProduct.Name,
                    Slug = existingProduct.Slug,
                    IsActive = existingProduct.IsActive,
                    UpdatedAt = existingProduct.UpdatedAt,
                    Version = existingProduct.Version
                };

                return Result<UpdateProductCommandResponse>.Success(response);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Erro de banco de dados ao atualizar produto. ProductId: {ProductId}", request.ProductId);
                
                // Rollback da transação
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                
                return Result<UpdateProductCommandResponse>.Failure("Erro ao atualizar produto no banco de dados");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao atualizar produto. ProductId: {ProductId}", request.ProductId);
            
            // Rollback da transação em caso de erro
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação. ProductId: {ProductId}", request.ProductId);
            }
            
            return Result<UpdateProductCommandResponse>.Failure("Erro inesperado ao atualizar produto");
        }
    }
}