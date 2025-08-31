using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Application.Common.Models;
using FHIRAI.Domain.Entities;

namespace FHIRAI.Application.TodoLists.Commands.UpdateTodoList;

public record UpdateTodoListCommand : IRequest
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    public ColourDto Colour { get; init; } = new();
}

public class UpdateTodoListCommandHandler : IRequestHandler<UpdateTodoListCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTodoListCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoLists
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Title = request.Title;
        entity.Colour = request.Colour;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
