using BuildingBlocks.Abstractions;
using BuildingBlocks.Mediator;
using CatalogService.Infrastructure.Abstractions;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.Commands.Categories.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryCommandResponse>>
{
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICatalogUnitOfWork unitOfWork,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CreateCategoryCommandResponse>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Criando categoria com Nome: {Name} e Slug: {Slug}", request.Name, request.Slug);

            // Iniciar transação
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Verificar se já existe uma categoria com o mesmo Slug
            var existingCategory = await _unitOfWork.Context.Categories
                .FirstOrDefaultAsync(c => c.Slug == request.Slug && c.DeletedAt == null, cancellationToken);

            if (existingCategory != null)
            {
                _logger.LogWarning("Categoria com slug já existe. Slug: {Slug}", request.Slug);
                return Result<CreateCategoryCommandResponse>.Failure("Já existe uma categoria com este slug.");
            }

            // Verificar se a categoria pai existe (se fornecida)
            if (request.ParentCategoryId.HasValue)
            {
                var parentCategoryExists = await _unitOfWork.Context.Categories
                    .AnyAsync(c => c.CategoryId == request.ParentCategoryId.Value && c.DeletedAt == null, cancellationToken);
                if (!parentCategoryExists)
                {
                    return Result<CreateCategoryCommandResponse>.Failure("Categoria pai não encontrada.");
                }
            }

            try
            {
                // Criar a entidade Category
                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = request.Name,
                    Slug = request.Slug,
                    Description = request.Description,
                    ParentCategoryId = request.ParentCategoryId,
                    IsActive = request.IsActive,
                    SortOrder = request.SortOrder,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Version = 1
                };

                // Adicionar a categoria ao contexto
                await _unitOfWork.Context.Categories.AddAsync(category, cancellationToken);

                // Salvar as mudanças através do UnitOfWork
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Confirmar transação
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Categoria criada com sucesso. CategoryId: {CategoryId}, Nome: {Name}", 
                    category.CategoryId, category.Name);

                var response = new CreateCategoryCommandResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Slug = category.Slug,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder,
                    CreatedAt = category.CreatedAt,
                    Version = category.Version
                };

                return Result<CreateCategoryCommandResponse>.Success(response);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Erro de banco de dados ao criar categoria. Nome: {Name}, Slug: {Slug}", 
                    request.Name, request.Slug);
                
                // Rollback da transação
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                
                return Result<CreateCategoryCommandResponse>.Failure("Erro ao salvar categoria no banco de dados");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar categoria. Nome: {Name}, Slug: {Slug}", 
                request.Name, request.Slug);
            
            // Rollback da transação em caso de erro
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Erro ao fazer rollback da transação. Nome: {Name}, Slug: {Slug}", 
                    request.Name, request.Slug);
            }
            
            return Result<CreateCategoryCommandResponse>.Failure("Erro inesperado ao criar categoria");
        }
    }
}