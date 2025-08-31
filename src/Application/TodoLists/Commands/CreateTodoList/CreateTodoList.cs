using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Application.Common.Models;
using FHIRAI.Domain.Entities;

namespace FHIRAI.Application.TodoLists.Commands.CreateTodoList;

public record CreateTodoListCommand : IRequest<Guid>
{
    public string? Title { get; init; }

    public ColourDto Colour { get; init; } = new();
}

public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoList();

        entity.Title = request.Title;
        entity.Colour = request.Colour;

        _context.TodoLists.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
