# FHIRAI - Code Patterns Guide

## 📋 Overview

FHIRAI sử dụng **Clean Architecture** với **MediatR**, **AutoMapper**, và **Entity Framework Core**. Template này cung cấp patterns chuẩn cho việc triển khai features một cách nhất quán và hiệu quả.

### 🏗️ Architecture Layers
```
Domain Layer (Entities, Value Objects, Domain Events)
    ↑
Application Layer (Commands, Queries, Handlers, DTOs)
    ↑
Infrastructure Layer (DbContext, Repositories, External Services)
    ↑
Web Layer (Endpoints, Controllers, Middleware)
```

## 🏷️ Naming Conventions

### C# Naming Standards
- **PascalCase**: Classes, methods, properties, namespaces, constants
- **camelCase**: Variables, parameters, local variables, private fields
- **UPPER_CASE**: Constants, static readonly fields, enum values
- **snake_case**: PostgreSQL database objects, configuration keys

### File Structure Patterns
```
src/
├── Application/
│   ├── Common/                    # Shared components
│   │   ├── Behaviours/           # Pipeline behaviors
│   │   ├── Exceptions/           # Application exceptions
│   │   ├── Interfaces/           # Application contracts
│   │   ├── Models/               # Shared models (PaginatedList, Result)
│   │   └── Mappings/             # AutoMapper extensions
│   └── [FeatureName]/            # Feature-based organization
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
│   ├── Common/                   # Base entities, value objects
│   ├── Entities/
│   ├── ValueObjects/
│   └── Events/
├── Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Interceptors/
│   └── Identity/
└── Web/
    ├── Endpoints/
    └── Services/
```

## 🔧 Core Implementation Patterns

### 1. Command Pattern (Write Operations)

#### Basic Command (Current FHIRAI Template)
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

    public CreateTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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
    }
}
```

#### Enhanced Command with Base Classes (Enterprise)
```csharp
// Base Request for enterprise features
public abstract record BaseRequest<TResponse> : IRequest<TResponse>
{
    public Guid RequestId { get; init; } = Guid.NewGuid();
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public string? UserId { get; init; }
    public string? TenantId { get; init; }
}

// Enhanced Command
public record CreateTodoItemCommand : BaseRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.Medium;
    public string? Note { get; init; }
}

// Enhanced Handler with error handling
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;

    public CreateTodoItemCommandHandler(IApplicationDbContext context, ILogger<CreateTodoItemCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        try
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

            _logger.LogInformation("Created TodoItem {Id} for List {ListId}", entity.Id, request.ListId);
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TodoItem for List {ListId}", request.ListId);
            throw;
        }
    }
}
```

### 2. Query Pattern (Read Operations)

#### Basic Query (Current FHIRAI Template)
```csharp
// Query
public record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
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
        return await _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
```

#### Enhanced Query with Advanced Filtering
```csharp
// Enhanced Query with filters
public record GetTodoItemsQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchString { get; init; }
    public PriorityLevel? Priority { get; init; }
    public bool? Done { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

// Enhanced Handler with filtering
public class GetTodoItemsQueryHandler 
    : IRequestHandler<GetTodoItemsQuery, PaginatedList<TodoItemBriefDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTodoItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(
        GetTodoItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TodoItems
            .Where(x => x.ListId == request.ListId);

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchString))
        {
            query = query.Where(x => x.Title.Contains(request.SearchString));
        }

        // Apply priority filter
        if (request.Priority.HasValue)
        {
            query = query.Where(x => x.Priority == request.Priority.Value);
        }

        // Apply done filter
        if (request.Done.HasValue)
        {
            query = query.Where(x => x.Done == request.Done.Value);
        }

        // Apply date range filter
        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(x => x.Created >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(x => x.Created <= request.CreatedTo.Value);
        }

        return await query
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
```

### 3. Paging Patterns

#### Current FHIRAI Pagination (Simple)
```csharp
// Using existing PaginatedList from template
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        AddRange(items);
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}

// Extension method for easy pagination
public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable, int pageNumber, int pageSize, 
        CancellationToken cancellationToken = default) where TDestination : class
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
}
```

#### Enhanced Paging with Base Classes (Enterprise)
```csharp
// Base Response for consistent error handling
public abstract record BaseResponse
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();
    public Guid RequestId { get; init; }
    public DateTime RespondedAt { get; init; } = DateTime.UtcNow;
    public int StatusCode { get; init; } = 200;
}

// Enhanced Paged Response
public record PagedResponse<T> : BaseResponse
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}

// Base Paged Request
public abstract record BasePagedRequest<TResponse> : BaseRequest<TResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string SortOrder { get; init; } = "asc";
    public string? SearchTerm { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
}
```

### 4. Endpoint Patterns

#### Current FHIRAI Endpoints (Minimal APIs)
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

### 5. Entity Patterns

#### Domain Entity (Current Template)
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

### 6. DTO Patterns

#### Request/Response DTOs
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

### 7. AutoMapper Patterns

#### Profile Configuration
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

### 8. Domain Event Patterns

#### Domain Event
```csharp
public class TodoItemCompletedEvent : BaseEvent
{
    public TodoItemCompletedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}
```

#### Event Handler
```csharp
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

## 🔄 MediatR Configuration

### Service Registration
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        });

        return services;
    }
}
```

### Using ISender in Endpoints
```csharp
// Inject ISender and use it to send commands/queries
public async Task<Ok<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination(
    ISender sender, [AsParameters] GetTodoItemsWithPaginationQuery query)
{
    var result = await sender.Send(query);
    return TypedResults.Ok(result);
}
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
    var handler = new CreateTodoItemCommandHandler(mockContext.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeGreaterThan(0);
    mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

## 🎯 Implementation Guidelines

### When to Use Base Classes
- **Use Base Classes**: For enterprise applications requiring audit trails, correlation tracking, and consistent error handling
- **Use Simple Records**: For simple CRUD operations and basic applications
- [ ] Use Base Classes: When you need request/response metadata for monitoring and debugging
- [ ] Use Simple Records: When performance is critical and you want minimal overhead

### Pattern Selection Guide
```csharp
// Simple CRUD - Use Current FHIRAI Template
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
}

// Enterprise Features - Use Base Classes
public record CreateTodoItemCommand : BaseRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
    public PriorityLevel Priority { get; init; } = PriorityLevel.Medium;
    public string? Note { get; init; }
}
```

### Benefits of Base Classes
- **Audit Trail**: Track all requests with RequestId and timestamps
- **Correlation**: Link related requests with CorrelationId
- **Error Handling**: Consistent error response format
- **Monitoring**: Built-in properties for logging and monitoring
- **Multi-tenancy**: Support for TenantId in enterprise applications

---

**🎯 Remember**: 
1. **Start Simple**: Use current FHIRAI template patterns for basic features
2. **Enhance When Needed**: Add base classes for enterprise requirements
3. **Maintain Consistency**: Choose one approach per feature and stick to it
4. **Follow Template**: Respect the existing Clean Architecture structure
5. **Test Thoroughly**: Always include unit and integration tests
6. **Document Changes**: Update documentation when adding new patterns
