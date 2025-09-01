using FHIRAI.Domain.Entities;

namespace FHIRAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<FhirResource> FhirResources { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
