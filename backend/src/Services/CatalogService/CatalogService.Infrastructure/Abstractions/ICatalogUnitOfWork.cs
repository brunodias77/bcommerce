using BuildingBlocks.Abstractions;
using CatalogService.Infrastructure.Data;

namespace CatalogService.Infrastructure.Abstractions;

public interface ICatalogUnitOfWork : IUnitOfWork
{
    CatalogDbContext Context { get; }
}