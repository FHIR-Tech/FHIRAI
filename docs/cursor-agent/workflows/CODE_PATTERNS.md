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

#### Base Entity Inheritance Guide

##### When to Use BaseEntity vs BaseAuditableEntity

```csharp
// Use BaseEntity for simple entities without audit requirements
public class SimpleEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Inherits: Id, DomainEvents, AddDomainEvent(), RemoveDomainEvent(), ClearDomainEvents()
}

// Use BaseAuditableEntity for entities requiring audit trail
public class AuditableEntity : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Inherits: Id, Created, CreatedBy, LastModified, LastModifiedBy, DomainEvents
    // Auto-populated by AuditableEntityInterceptor
}
```

##### Entity Creation Checklist

```csharp
public class TodoItem : BaseAuditableEntity
{
    // 1. Required Properties
    public int ListId { get; set; }
    public string? Title { get; set; }
    
    // 2. Optional Properties
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }
    
    // 3. Navigation Properties
    public TodoList List { get; set; } = null!;
    
    // 4. Private Fields for Encapsulation
    private bool _done;
    
    // 5. Public Properties with Business Logic
    public bool Done
    {
        get => _done;
        set
        {
            if (value && !_done)
            {
                AddDomainEvent(new TodoItemCompletedEvent(this));
            }
            _done = value;
        }
    }
    
    // 6. Business Logic Methods
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
    
    // 7. Validation Methods (Optional)
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Title) && ListId > 0;
    }
}
```

##### Entity Configuration Pattern

```csharp
// src/Infrastructure/Data/Configurations/TodoItemConfiguration.cs
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Table configuration
        builder.ToTable("TodoItems");
        
        // Primary key
        builder.HasKey(x => x.Id);
        
        // Properties configuration
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(x => x.Note)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);
            
        // Relationships
        builder.HasOne(x => x.List)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Indexes
        builder.HasIndex(x => x.ListId);
        builder.HasIndex(x => x.Done);
        builder.HasIndex(x => x.Priority);
        
        // Query filters (for soft delete if implemented)
        // builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
```

##### Domain Event Pattern

```csharp
// Domain Event
public class TodoItemCompletedEvent : BaseEvent
{
    public TodoItemCompletedEvent(TodoItem item)
    {
        Item = item;
    }

    public TodoItem Item { get; }
}

// Event Handler
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

## 🔧 Enhanced Implementation Patterns

### 9. GetById/GetDetail Pattern

#### Basic GetById Query
```csharp
// Query
public record GetTodoItemByIdQuery : IRequest<TodoItemDetailDto?>
{
    public int Id { get; init; }
}

// Handler with Error Handling
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, TodoItemDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public async Task<TodoItemDetailDto?> Handle(
        GetTodoItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting TodoItem with Id {Id} for user {UserId}", 
                request.Id, _currentUserService.UserId);

            var todoItem = await _context.TodoItems
                .Include(x => x.List)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (todoItem == null)
            {
                _logger.LogWarning("TodoItem with Id {Id} not found", request.Id);
                return null;
            }

            // Authorization check
            if (!await HasAccessToTodoItem(todoItem, cancellationToken))
            {
                _logger.LogWarning("User {UserId} does not have access to TodoItem {Id}", 
                    _currentUserService.UserId, request.Id);
                throw new ForbiddenAccessException();
            }

            var dto = _mapper.Map<TodoItemDetailDto>(todoItem);
            
            _logger.LogInformation("Successfully retrieved TodoItem {Id}", request.Id);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TodoItem with Id {Id}", request.Id);
            throw;
        }
    }

    private async Task<bool> HasAccessToTodoItem(TodoItem todoItem, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var userRoles = _currentUserService.Roles;

        if (userRoles.Contains("Admin")) return true;

        // Check ownership
        var todoList = await _context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == todoItem.ListId, cancellationToken);

        return todoList?.CreatedBy == userId;
    }
}

// Validator
public class GetTodoItemByIdQueryValidator : AbstractValidator<GetTodoItemByIdQuery>
{
    public GetTodoItemByIdQueryValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");
    }
}
```

#### Enhanced GetById with Result Pattern
```csharp
// Enhanced Query with Result pattern
public record GetTodoItemByIdQuery : IRequest<Result<TodoItemDetailDto>>
{
    public int Id { get; init; }
}

// Enhanced Handler with proper error handling
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, Result<TodoItemDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;

    public async Task<Result<TodoItemDetailDto>> Handle(
        GetTodoItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cacheKey = $"TodoItem:{request.Id}:{_currentUserService.UserId}";
            var cachedResult = await _cacheService.GetAsync<TodoItemDetailDto>(cacheKey, cancellationToken);
            
            if (cachedResult != null)
            {
                _logger.LogInformation("Retrieved TodoItem {Id} from cache", request.Id);
                return Result<TodoItemDetailDto>.Success(cachedResult);
            }

            _logger.LogInformation("Getting TodoItem with Id {Id} for user {UserId}", 
                request.Id, _currentUserService.UserId);

            var todoItem = await _context.TodoItems
                .Include(x => x.List)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (todoItem == null)
            {
                _logger.LogWarning("TodoItem with Id {Id} not found", request.Id);
                return Result<TodoItemDetailDto>.NotFound($"TodoItem with Id {request.Id} not found");
            }

            // Authorization check
            if (!await HasAccessToTodoItem(todoItem, cancellationToken))
            {
                _logger.LogWarning("User {UserId} does not have access to TodoItem {Id}", 
                    _currentUserService.UserId, request.Id);
                return Result<TodoItemDetailDto>.Failure("Access denied", statusCode: 403);
            }

            var dto = _mapper.Map<TodoItemDetailDto>(todoItem);
            
            // Cache the result for 5 minutes
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), cancellationToken);
            
            _logger.LogInformation("Successfully retrieved TodoItem {Id}", request.Id);
            return Result<TodoItemDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TodoItem with Id {Id}", request.Id);
            return Result<TodoItemDetailDto>.Failure("An error occurred while retrieving the TodoItem");
        }
    }
}
```

### 10. Error Handling Patterns

#### Result Pattern
```csharp
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();
    public int StatusCode { get; init; } = 200;

    public static Result<T> Success(T value, string? message = null) =>
        new() { IsSuccess = true, Value = value, Message = message };

    public static Result<T> Failure(string message, List<string>? errors = null, int statusCode = 400) =>
        new() { IsSuccess = false, Message = message, Errors = errors ?? new List<string>(), StatusCode = statusCode };

    public static Result<T> NotFound(string message = "Entity not found") =>
        new() { IsSuccess = false, Message = message, StatusCode = 404 };
}
```

#### Custom Exceptions
```csharp
public class NotFoundException : Exception
{
    public NotFoundException()
        : base()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("Access denied") { }
    public ForbiddenAccessException(string message) : base(message) { }
}
```

### 11. Caching Patterns

#### Cache Service Interface
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}
```

#### Redis Cache Implementation
```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(value))
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key {Key}", key);
        }
    }
}
```

### 12. Authorization Patterns

#### Authorization in Handlers
```csharp
private async Task<bool> HasAccessToTodoItem(TodoItem todoItem, CancellationToken cancellationToken)
{
    var userId = _currentUserService.UserId;
    var userRoles = _currentUserService.Roles;

    if (userRoles.Contains("Admin")) return true;

    // Check ownership
    var todoList = await _context.TodoLists
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == todoItem.ListId, cancellationToken);

    return todoList?.CreatedBy == userId;
}
```

#### Authorization Attributes
```csharp
[Authorize(Policy = "TodoItemAccess")]
public record GetTodoItemByIdQuery : IRequest<TodoItemDetailDto?>
{
    public int Id { get; init; }
}
```

### 13. Performance Monitoring

#### Performance Behaviour
```csharp
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        IUser user,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await _identityService.GetUserNameAsync(userId);
            }

            _logger.LogWarning("FHIRAI Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
```

### 14. Comprehensive Logging Patterns

#### Logging Behaviour (Request/Response Logging)
```csharp
public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("FHIRAI Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
```

#### Performance Behaviour (Slow Request Detection)
```csharp
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        IUser user,
        IIdentityService identityService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _user.Id ?? string.Empty;
            var userName = string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                userName = await _identityService.GetUserNameAsync(userId);
            }

            _logger.LogWarning("FHIRAI Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
```

#### Unhandled Exception Behaviour
```csharp
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex, "FHIRAI Request: Unhandled Exception for Request {Name} {@Request}", requestName, request);

            throw;
        }
    }
}
```

#### Handler-Level Logging Patterns

##### Command Handler Logging
```csharp
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTodoItemCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateTodoItemCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTodoItemCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating TodoItem for List {ListId} by User {UserId}", 
                request.ListId, _currentUserService.UserId);

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

            _logger.LogInformation("Successfully created TodoItem {Id} for List {ListId}", 
                entity.Id, request.ListId);
            
            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create TodoItem for List {ListId} by User {UserId}", 
                request.ListId, _currentUserService.UserId);
            throw;
        }
    }
}
```

##### Query Handler Logging
```csharp
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, TodoItemDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public async Task<TodoItemDetailDto?> Handle(GetTodoItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting TodoItem {Id} for User {UserId}", 
                request.Id, _currentUserService.UserId);

            var todoItem = await _context.TodoItems
                .Include(x => x.List)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (todoItem == null)
            {
                _logger.LogWarning("TodoItem {Id} not found for User {UserId}", 
                    request.Id, _currentUserService.UserId);
                return null;
            }

            var dto = _mapper.Map<TodoItemDetailDto>(todoItem);
            
            _logger.LogInformation("Successfully retrieved TodoItem {Id} for User {UserId}", 
                request.Id, _currentUserService.UserId);
            
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TodoItem {Id} for User {UserId}", 
                request.Id, _currentUserService.UserId);
            throw;
        }
    }
}
```

#### Domain Event Handler Logging
```csharp
public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public TodoItemCompletedEventHandler(
        ILogger<TodoItemCompletedEventHandler> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FHIRAI Domain: TodoItem {Id} was completed by User {UserId}", 
            notification.Item.Id, _currentUserService.UserId);
        
        // Handle side effects here
        // e.g., send notifications, update statistics, etc.
        
        await Task.CompletedTask;
    }
}
```

#### Logging Configuration
```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "FHIRAI": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
      }
    }
  }
}

// Program.cs
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Indented = true
    };
});
```

#### Logging Best Practices

##### Structured Logging Guidelines
```csharp
// ✅ Good: Structured logging with proper context
_logger.LogInformation("User {UserId} created TodoItem {TodoItemId} in List {ListId}", 
    userId, todoItemId, listId);

// ❌ Bad: String concatenation
_logger.LogInformation("User " + userId + " created TodoItem " + todoItemId);

// ✅ Good: Exception logging with context
_logger.LogError(ex, "Failed to create TodoItem {TodoItemId} for User {UserId}", 
    todoItemId, userId);

// ❌ Bad: Exception logging without context
_logger.LogError(ex, "Failed to create TodoItem");
```

##### Log Levels Guidelines
```csharp
// Trace: Detailed diagnostic information
_logger.LogTrace("Entering method {MethodName} with parameters {@Parameters}", methodName, parameters);

// Debug: Diagnostic information for debugging
_logger.LogDebug("Processing TodoItem {Id} with priority {Priority}", id, priority);

// Information: General information about application flow
_logger.LogInformation("User {UserId} created TodoItem {Id}", userId, id);

// Warning: Unexpected situations that don't stop execution
_logger.LogWarning("TodoItem {Id} has high priority but no due date", id);

// Error: Error conditions that don't stop execution
_logger.LogError(ex, "Failed to save TodoItem {Id}", id);

// Critical: Critical errors that may cause application failure
_logger.LogCritical(ex, "Database connection failed for TodoItem {Id}", id);
```

### 15. Enhanced Endpoint với Error Handling

```csharp
public class TodoItems : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTodoItemById, "{id}").RequireAuthorization();
        // ... other endpoints
    }

    // Basic version
    public async Task<Results<Ok<TodoItemDetailDto>, NotFound>> GetTodoItemById(
        ISender sender, int id)
    {
        var query = new GetTodoItemByIdQuery { Id = id };
        var result = await sender.Send(query);
        
        return result != null 
            ? TypedResults.Ok(result) 
            : TypedResults.NotFound();
    }

    // Enhanced version with Result pattern
    public async Task<Results<Ok<TodoItemDetailDto>, NotFound, BadRequest>> GetTodoItemByIdEnhanced(
        ISender sender, int id)
    {
        var query = new GetTodoItemByIdQuery { Id = id };
        var result = await sender.Send(query);
        
        return result.IsSuccess 
            ? TypedResults.Ok(result.Value!) 
            : result.StatusCode switch
            {
                404 => TypedResults.NotFound(),
                403 => TypedResults.BadRequest(result.Message),
                _ => TypedResults.BadRequest(result.Errors)
            };
    }
}
```

## 🔄 MediatR Configuration

### Service Registration with Logging Behaviours
```csharp
public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Add behaviours in execution order
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));           // 1. Log request
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));         // 2. Catch exceptions
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));              // 3. Check authorization
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));                 // 4. Validate request
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));                // 5. Monitor performance
        });
    }
}
```

### Behaviour Execution Order
```csharp
// Execution order for MediatR pipeline:
// 1. LoggingBehaviour (PreProcessor) - Log incoming request
// 2. UnhandledExceptionBehaviour - Catch and log exceptions
// 3. AuthorizationBehaviour - Check user permissions
// 4. ValidationBehaviour - Validate request data
// 5. Handler - Execute business logic
// 6. PerformanceBehaviour - Monitor execution time
// 7. LoggingBehaviour (PostProcessor) - Log response (if implemented)
```

### Behaviour Configuration Options
```csharp
// Custom configuration for behaviours
public static class MediatRConfiguration
{
    public static void ConfigureMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configure logging behaviour with custom settings
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            
            // Configure performance monitoring with custom threshold
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            
            // Configure validation with custom error handling
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            
            // Configure authorization with custom policies
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            
            // Configure exception handling with custom logging
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });
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

## 🚀 Advanced Patterns Summary

### ✅ Đã được cải thiện:
1. **BaseEntity Inheritance**: Hướng dẫn chi tiết về khi nào sử dụng BaseEntity vs BaseAuditableEntity
2. **Entity Creation Checklist**: 7 bước tạo entity hoàn chỉnh với business logic
3. **Entity Configuration**: Pattern cho EntityTypeConfiguration với indexes và relationships
4. **GetById/GetDetail Pattern**: Pattern hoàn chỉnh cho việc lấy chi tiết entity
5. **Error Handling**: Result pattern và custom exceptions
6. **Caching**: Redis cache service với interface chuẩn
7. **Authorization**: Authorization patterns trong handlers
8. **Comprehensive Logging**: 5 loại logging patterns (Request, Performance, Exception, Handler, Domain Event)
9. **MediatR Configuration**: Execution order và configuration options cho behaviours
10. **Logging Best Practices**: Structured logging guidelines và log levels

### 🎯 Khi nào sử dụng:
- **Patterns cơ bản**: Cho CRUD operations đơn giản
- **Enhanced Patterns**: Cho enterprise applications với yêu cầu cao về security, performance, monitoring
- **Logging Patterns**: Áp dụng cho tất cả handlers để có audit trail đầy đủ

---

**🎯 Remember**: 
1. **Start Simple**: Use current FHIRAI template patterns for basic features
2. **Enhance When Needed**: Add base classes for enterprise requirements
3. **Maintain Consistency**: Choose one approach per feature and stick to it
4. **Follow Template**: Respect the existing Clean Architecture structure
5. **Test Thoroughly**: Always include unit and integration tests
6. **Document Changes**: Update documentation when adding new patterns
7. **Monitor Performance**: Use performance behaviours for slow request detection
8. **Implement Caching**: Use caching for frequently accessed data
9. **Handle Errors Gracefully**: Use Result pattern for consistent error handling
10. **Secure Your APIs**: Implement proper authorization and validation
