# FHIRAI - Code Patterns Guide

## 🏷️ Naming Conventions

### C# Naming Standards
- **PascalCase**: Classes, methods, properties, namespaces, constants
- **camelCase**: Variables, parameters, local variables, private fields
- **UPPER_CASE**: Constants, static readonly fields, enum values
- **snake_case**: PostgreSQL database objects, configuration keys
- **Descriptive Names**: Self-documenting code, avoid abbreviations

### File Naming Patterns
```
src/
├── Application/
│   └── [FeatureName]/
│       ├── Commands/
│       │   └── [CommandName]/
│       │       ├── [CommandName]Command.cs
│       │       ├── [CommandName]CommandHandler.cs
│       │       └── [CommandName]CommandValidator.cs
│       ├── Queries/
│       │   └── [QueryName]/
│       │       ├── [QueryName]Query.cs
│       │       ├── [QueryName]QueryHandler.cs
│       │       └── [QueryName]QueryValidator.cs
│       └── EventHandlers/
│           └── [EventName]EventHandler.cs
├── Domain/
│   ├── Entities/
│   │   └── [EntityName].cs
│   ├── ValueObjects/
│   │   └── [ValueObjectName].cs
│   └── Events/
│       └── [EventName]Event.cs
├── Infrastructure/
│   ├── Data/
│   │   └── Configurations/
│   │       └── [EntityName]Configuration.cs
│   └── Identity/
│       └── [ServiceName].cs
└── Web/
    └── Endpoints/
        └── [EntityName].cs
```

### Database Naming Conventions
```sql
-- Tables: PascalCase
CREATE TABLE "TodoItems" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Created" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100)
);

-- Columns: PascalCase
-- Foreign Keys: [ReferencedTable]Id
-- Indexes: IX_[TableName]_[ColumnName]
-- Constraints: [ConstraintType]_[TableName]_[ColumnName]
```

## 🔧 Implementation Patterns

### Command Pattern (Write Operations)
```csharp
// Command
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.Medium;
    public string? Note { get; init; }
}

// Handler
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateTodoItemCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Priority = request.Priority,
            Note = request.Note,
            Done = false
        };

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));
        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}

// Validator
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.ListId)
            .GreaterThan(0)
            .WithMessage("ListId must be greater than 0.");

        RuleFor(v => v.Note)
            .MaximumLength(1000)
            .When(v => !string.IsNullOrEmpty(v.Note))
            .WithMessage("Note must not exceed 1000 characters.");
    }
}
```

### Query Pattern (Read Operations)
```csharp
// Query
public record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchString { get; init; }
    public PriorityLevel? Priority { get; init; }
    public bool? Done { get; init; }
}

// Handler
public class GetTodoItemsWithPaginationQueryHandler 
    : IRequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodoItemsWithPaginationQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(
        GetTodoItemsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems
            .Where(x => x.ListId == request.ListId);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchString))
        {
            query = query.Where(x => x.Title.Contains(request.SearchString));
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == request.Priority.Value);
        }

        if (request.Done.HasValue)
        {
            query = query.Where(x => x.Done == request.Done.Value);
        }

        return await query
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
```

### Endpoint Pattern (Minimal APIs)
```csharp
public class TodoItems : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTodoItemsWithPagination).RequireAuthorization();
        groupBuilder.MapPost(CreateTodoItem).RequireAuthorization();
        groupBuilder.MapPut(UpdateTodoItem, "{id}").RequireAuthorization();
        groupBuilder.MapPut(UpdateTodoItemDetail, "UpdateDetail/{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteTodoItem, "{id}").RequireAuthorization();
    }

    public async Task<Ok<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination(
        ISender sender, [AsParameters] GetTodoItemsWithPaginationQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateTodoItem(ISender sender, CreateTodoItemCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(TodoItems)}/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest>> UpdateTodoItem(
        ISender sender, int id, UpdateTodoItemCommand command)
    {
        if (id != command.Id) return TypedResults.BadRequest();

        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteTodoItem(ISender sender, int id)
    {
        await sender.Send(new DeleteTodoItemCommand(id));
        return TypedResults.NoContent();
    }
}
```

### Entity Pattern (Domain Layer)
```csharp
public class TodoItem : BaseAuditableEntity
{
    public int ListId { get; set; }
    public string? Title { get; set; }
    public bool Done { get; set; }
    public PriorityLevel Priority { get; set; }
    public string? Note { get; set; }
    
    public TodoList List { get; set; } = null!;

    // Business logic methods
    public void MarkComplete()
    {
        if (Done) return;
        
        Done = true;
        AddDomainEvent(new TodoItemCompletedEvent(this));
    }

    public void MarkIncomplete()
    {
        if (!Done) return;
        
        Done = false;
        AddDomainEvent(new TodoItemIncompleteEvent(this));
    }

    public void UpdatePriority(PriorityLevel newPriority)
    {
        if (Priority == newPriority) return;
        
        var oldPriority = Priority;
        Priority = newPriority;
        AddDomainEvent(new TodoItemPriorityChangedEvent(this, oldPriority, newPriority));
    }
}
```

### Value Object Pattern
```csharp
public class Colour : ValueObject
{
    public string Code { get; }

    public Colour(string code)
    {
        Code = code;
    }

    public static Colour From(string code)
    {
        var colour = new Colour(code);
        
        if (!SupportedColours.Contains(colour))
        {
            throw new UnsupportedColourException(code);
        }

        return colour;
    }

    public static Colour White => new("#FFFFFF");
    public static Colour Red => new("#FF0000");
    public static Colour Orange => new("#FFA500");
    public static Colour Yellow => new("#FFFF00");
    public static Colour Green => new("#008000");
    public static Colour Blue => new("#0000FF");
    public static Colour Purple => new("#800080");
    public static Colour Grey => new("#808080");

    private static IEnumerable<Colour> SupportedColours
    {
        get
        {
            yield return White;
            yield return Red;
            yield return Orange;
            yield return Yellow;
            yield return Green;
            yield return Blue;
            yield return Purple;
            yield return Grey;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
```

### Domain Event Pattern
```csharp
public class TodoItemCompletedEvent : BaseEvent
{
    public TodoItemCompletedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}

public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;

    public TodoItemCompletedEventHandler(ILogger<TodoItemCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FHIRAI Domain: TodoItem {Id} was completed", notification.Item.Id);
        
        // Handle side effects here
        // e.g., send notifications, update statistics, etc.
        
        await Task.CompletedTask;
    }
}
```

### Repository Pattern (Infrastructure)
```csharp
public interface ITodoItemRepository
{
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItem>> GetByListIdAsync(int listId, CancellationToken cancellationToken = default);
    Task<TodoItem> AddAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
}

public class TodoItemRepository : ITodoItemRepository
{
    private readonly IApplicationDbContext _context;

    public TodoItemRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TodoItems
            .Include(x => x.List)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TodoItem>> GetByListIdAsync(int listId, CancellationToken cancellationToken = default)
    {
        return await _context.TodoItems
            .Where(x => x.ListId == listId)
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoItem> AddAsync(TodoItem todoItem, CancellationToken cancellationToken = default)
    {
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync(cancellationToken);
        return todoItem;
    }

    public async Task UpdateAsync(TodoItem todoItem, CancellationToken cancellationToken = default)
    {
        _context.TodoItems.Update(todoItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoItem todoItem, CancellationToken cancellationToken = default)
    {
        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### DTO Pattern (Data Transfer Objects)
```csharp
// Request DTOs
public record CreateTodoItemRequest
{
    public int ListId { get; init; }
    public string? Title { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.Medium;
    public string? Note { get; init; }
}

public record UpdateTodoItemRequest
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public PriorityLevel Priority { get; init; }
    public string? Note { get; init; }
}

// Response DTOs
public record TodoItemBriefDto
{
    public int Id { get; init; }
    public int ListId { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public PriorityLevel Priority { get; init; }
}

public record TodoItemDetailDto
{
    public int Id { get; init; }
    public int ListId { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public PriorityLevel Priority { get; init; }
    public string? Note { get; init; }
    public DateTime Created { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
}
```

### AutoMapper Profile Pattern
```csharp
public class TodoItemProfile : Profile
{
    public TodoItemProfile()
    {
        CreateMap<TodoItem, TodoItemBriefDto>();
        CreateMap<TodoItem, TodoItemDetailDto>();
        CreateMap<CreateTodoItemCommand, TodoItem>();
        CreateMap<UpdateTodoItemCommand, TodoItem>();
    }
}
```

## 🏗️ Code Organization

### Feature-based Structure
```
src/Application/
├── Common/                    # Shared components
│   ├── Behaviours/           # Cross-cutting concerns
│   ├── Exceptions/           # Application exceptions
│   ├── Interfaces/           # Application contracts
│   ├── Models/               # Shared DTOs
│   └── Security/             # Authorization attributes
├── TodoItems/                # TodoItems feature
│   ├── Commands/             # Write operations
│   ├── Queries/              # Read operations
│   └── EventHandlers/        # Domain event handlers
├── TodoLists/                # TodoLists feature
│   ├── Commands/
│   ├── Queries/
│   └── EventHandlers/
└── WeatherForecasts/         # WeatherForecasts feature
    └── Queries/
```

### Layer Separation
- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Orchestrates business operations, depends only on Domain
- **Infrastructure Layer**: Implements external concerns, depends on Application
- **Web Layer**: Handles HTTP concerns, depends on Application and Infrastructure

### Dependency Direction
```
Domain Layer (Independent)
    ↑
Application Layer (Depends on Domain)
    ↑
Infrastructure Layer (Depends on Application)
    ↑
Web Layer (Depends on Application & Infrastructure)
```

## 🧪 Testing Patterns

### Unit Test Pattern
```csharp
[Test]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new CreateTodoItemCommand 
    { 
        ListId = 1, 
        Title = "Test Todo Item" 
    };
    
    var mockContext = new Mock<IApplicationDbContext>();
    var mockMapper = new Mock<IMapper>();
    var handler = new CreateTodoItemCommandHandler(mockContext.Object, mockMapper.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeGreaterThan(0);
    mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}

[Test]
public void MarkComplete_WhenNotDone_SetsDoneToTrue()
{
    // Arrange
    var todoItem = new TodoItem { Title = "Test", Done = false };

    // Act
    todoItem.MarkComplete();

    // Assert
    todoItem.Done.Should().BeTrue();
    todoItem.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<TodoItemCompletedEvent>();
}
```

### Integration Test Pattern
```csharp
public class TodoItemsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TodoItemsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Test]
    public async Task GetTodoItems_ReturnsSuccessResult()
    {
        // Arrange
        var query = new GetTodoItemsWithPaginationQuery { ListId = 1 };

        // Act
        var response = await _client.GetAsync($"/api/todoitems?listId={query.ListId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}
```

## 📊 Code Quality Standards

### Code Metrics
- **Cyclomatic Complexity**: ≤ 10 per method
- **Lines of Code**: ≤ 50 per method
- **Class Coupling**: Minimize dependencies
- **Depth of Inheritance**: ≤ 4 levels
- **Maintainability Index**: ≥ 65

### Code Review Checklist
- [ ] Follows naming conventions
- [ ] Implements proper error handling
- [ ] Includes appropriate logging
- [ ] Has unit tests with good coverage
- [ ] Follows SOLID principles
- [ ] Uses async/await properly
- [ ] Implements proper validation
- [ ] Follows security best practices

### Performance Guidelines
- [ ] Use async/await for I/O operations
- [ ] Implement proper caching strategies
- [ ] Optimize database queries
- [ ] Use pagination for large datasets
- [ ] Implement proper disposal of resources
- [ ] Monitor memory usage

---

**🎯 Remember**: Always follow established patterns, maintain consistency across the codebase, and prioritize readability and maintainability. Use patterns to solve specific problems, not just for the sake of using patterns. Keep code simple, testable, and well-documented.
