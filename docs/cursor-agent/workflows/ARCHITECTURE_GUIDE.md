# FHIRAI - Architecture Guide

## 🎯 Core Architecture Principles

### Clean Architecture 4-Layer Pattern
FHIRAI tuân thủ Clean Architecture với 4 layers rõ ràng:

```
┌─────────────────────────────────────┐
│           Web Layer                 │ ← Presentation (API Endpoints)
├─────────────────────────────────────┤
│        Application Layer            │ ← Business Logic (Commands/Queries)
├─────────────────────────────────────┤
│       Infrastructure Layer          │ ← External Concerns (Database, APIs)
├─────────────────────────────────────┤
│          Domain Layer               │ ← Business Entities & Rules
└─────────────────────────────────────┘
```

### Dependency Inversion Principle
- **Domain Layer**: Không phụ thuộc vào bất kỳ layer nào khác
- **Application Layer**: Chỉ phụ thuộc vào Domain
- **Infrastructure Layer**: Implement interfaces từ Application
- **Web Layer**: Phụ thuộc vào Application và Infrastructure

### Domain-Driven Design (DDD)
- **Entities**: Business objects với identity và lifecycle
- **Value Objects**: Immutable objects không có identity
- **Domain Events**: Events phát sinh từ business logic
- **Aggregates**: Clusters of related entities

### SOLID Principles
- **Single Responsibility**: Mỗi class có một trách nhiệm duy nhất
- **Open/Closed**: Mở để mở rộng, đóng để sửa đổi
- **Liskov Substitution**: Subtypes có thể thay thế base types
- **Interface Segregation**: Small, focused interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

## 📁 Layer Responsibilities

### Domain Layer (`src/Domain/`)
**Mục đích**: Chứa business logic và rules

#### Entities
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
        Done = true;
        AddDomainEvent(new TodoItemCompletedEvent(this));
    }
}
```

#### Value Objects
```csharp
public class Colour : ValueObject
{
    public string Code { get; }

    public Colour(string code)
    {
        Code = code;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
```

#### Domain Events
```csharp
public class TodoItemCreatedEvent : BaseEvent
{
    public TodoItemCreatedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}
```

### Application Layer (`src/Application/`)
**Mục đích**: Orchestrate business logic và coordinate between layers

#### Commands (Write Operations)
```csharp
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
}

public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoItem
        {
            ListId = request.ListId,
            Title = request.Title,
            Done = false
        };

        entity.AddDomainEvent(new TodoItemCreatedEvent(entity));
        _context.TodoItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
```

#### Queries (Read Operations)
```csharp
public record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetTodoItemsWithPaginationQueryHandler 
    : IRequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(
        GetTodoItemsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        return await _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
```

#### Behaviours (Cross-cutting Concerns)
```csharp
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        return await next();
    }
}
```

### Infrastructure Layer (`src/Infrastructure/`)
**Mục đích**: Implement external concerns và technical details

#### Data Access (Entity Framework Core)
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

#### Entity Configuration
```csharp
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Note)
            .HasMaxLength(1000);
    }
}
```

#### Identity Implementation
```csharp
public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
        return user.UserName;
    }
}
```

### Presentation Layer (`src/Web/`)
**Mục đích**: Handle HTTP requests và responses

#### Minimal API Endpoints
```csharp
public class TodoItems : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTodoItemsWithPagination).RequireAuthorization();
        groupBuilder.MapPost(CreateTodoItem).RequireAuthorization();
        groupBuilder.MapPut(UpdateTodoItem, "{id}").RequireAuthorization();
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
}
```

## 🔄 Design Patterns

### CQRS Pattern (Command Query Responsibility Segregation)
- **Commands**: Write operations, modify state
- **Queries**: Read operations, return data
- **Benefits**: Separation of concerns, optimization for read/write

### Repository Pattern
```csharp
public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }
    DbSet<TodoItem> TodoItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

### Factory Pattern
```csharp
public class TodoItemFactory
{
    public static TodoItem Create(string title, int listId, PriorityLevel priority = PriorityLevel.Medium)
    {
        return new TodoItem
        {
            Title = title,
            ListId = listId,
            Priority = priority,
            Done = false
        };
    }
}
```

### Observer Pattern (Domain Events)
```csharp
public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;

    public async Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FHIRAI Domain: TodoItem {Id} was completed", notification.Item.Id);
        await Task.CompletedTask;
    }
}
```

### Strategy Pattern
```csharp
public interface IPriorityStrategy
{
    int CalculatePriority(TodoItem item);
}

public class HighPriorityStrategy : IPriorityStrategy
{
    public int CalculatePriority(TodoItem item) => item.Priority == PriorityLevel.High ? 100 : 0;
}
```

## 📊 Architecture Diagrams

### System Overview
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web Layer     │    │  Application    │    │ Infrastructure  │
│   (Minimal API) │◄──►│   Layer         │◄──►│   Layer         │
│                 │    │  (MediatR)      │    │  (EF Core)      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   Domain Layer  │
                       │  (Entities)     │
                       └─────────────────┘
```

### Data Flow
```
HTTP Request → Web Layer → Application Layer → Domain Layer
                ↓              ↓                ↓
HTTP Response ← Web Layer ← Application Layer ← Infrastructure Layer
```

### Security Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   JWT Token     │    │  Authorization  │    │   Azure Key     │
│   Validation    │◄──►│   Policies      │◄──►│   Vault         │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🎯 Architecture Checklist

### ✅ Core Principles
- [ ] Clean Architecture 4-layer pattern implemented
- [ ] Dependency Inversion Principle followed
- [ ] Domain layer has no external dependencies
- [ ] CQRS pattern with MediatR implemented
- [ ] SOLID principles applied consistently

### ✅ Layer Responsibilities
- [ ] Domain layer contains business entities and logic
- [ ] Application layer orchestrates business operations
- [ ] Infrastructure layer handles external concerns
- [ ] Web layer handles HTTP requests/responses

### ✅ Design Patterns
- [ ] Repository pattern for data access
- [ ] Factory pattern for object creation
- [ ] Observer pattern for domain events
- [ ] Strategy pattern for business rules
- [ ] CQRS pattern for command/query separation

### ✅ Code Organization
- [ ] Feature-based folder structure
- [ ] Clear separation between layers
- [ ] Consistent naming conventions
- [ ] Proper dependency injection setup

---

**🎯 Remember**: Always maintain layer boundaries, follow dependency direction (Domain → Application → Infrastructure → Web), and keep business logic in the Domain layer. Use design patterns appropriately to solve specific problems, not just for the sake of using patterns.
