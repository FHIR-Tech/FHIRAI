using FHIRAI.Application.Common.Models;
using FHIRAI.Domain.Entities;
using FHIRAI.Domain.ValueObjects;

namespace FHIRAI.Application.TodoLists.Queries.GetTodos;

public class TodoListDto
{
    public Guid Id { get; init; }

    public string? Title { get; init; }

    public ColourDto Colour { get; init; } = new();

    public IList<TodoItemDto> Items { get; init; } = new List<TodoItemDto>();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<TodoList, TodoListDto>();
        }
    }
}
