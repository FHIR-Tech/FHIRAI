# FHIRAI - Hướng dẫn Mẫu Code (Code Patterns Guide)

## Mục lục (Table of Contents)

- [1. Tổng quan (Overview)](#1-tổng-quan-overview)
  - [1.1. Các Tầng Kiến trúc (Architecture Layers)](#11-các-tầng-kiến-trúc-architecture-layers)
- [2. Cấu trúc & Quy ước (Structure & Conventions)](#2-cấu-trúc--quy-ước-structure--conventions)
  - [2.1. Quy ước Đặt tên (Naming Conventions)](#21-quy-ước-đặt-tên-naming-conventions)
  - [2.2. Cấu trúc Thư mục (File Structure Patterns)](#22-cấu-trúc-thư-mục-file-structure-patterns)
- [3. Mẫu Lớp Domain (Domain Layer Patterns)](#3-mẫu-lớp-domain-domain-layer-patterns)
  - [3.1. Mẫu Entity (Entity Patterns)](#31-mẫu-entity-entity-patterns)
  - [3.2. Mẫu Cấu hình Entity (Entity Configuration Pattern)](#32-mẫu-cấu-hình-entity-entity-configuration-pattern)
  - [3.3. Mẫu Sự kiện Domain (Domain Event Pattern)](#33-mẫu-sự-kiện-domain-domain-event-pattern)
- [4. Mẫu Lớp Ứng dụng (Application Layer Patterns)](#4-mẫu-lớp-ứng-dụng-application-layer-patterns)
  - [4.1. Command Pattern (Thao tác Ghi - Write Operations)](#41-command-pattern-thao-tác-ghi---write-operations)
  - [4.2. Query Pattern (Thao tác Đọc - Read Operations)](#42-query-pattern-thao-tác-đọc---read-operations)
  - [4.3. Mẫu GetById/GetDetail (Chi tiết đối tượng)](#43-mẫu-getbyidgetdetail-chi-tiết-đối-tượng)
  - [4.4. Mẫu Phân trang (Paging Patterns)](#44-mẫu-phân-trang-paging-patterns)
  - [4.5. Mẫu DTO (Data Transfer Objects)](#45-mẫu-dto-data-transfer-objects)
  - [4.6. Mẫu AutoMapper (AutoMapper Patterns)](#46-mẫu-automapper-automapper-patterns)
- [5. Mẫu Lớp Web & API (Web & API Layer Patterns)](#5-mẫu-lớp-web--api-web--api-layer-patterns)
  - [5.1. Mẫu Endpoint (Minimal APIs)](#51-mẫu-endpoint-minimal-apis)
  - [5.2. Mẫu Triển khai API (API Implementation Templates)](#52-mẫu-triển-khai-api-api-implementation-templates)

---

## 1. Tổng quan (Overview)

FHIRAI sử dụng **Clean Architecture** với **MediatR**, **AutoMapper**, và **Entity Framework Core**. Tài liệu này cung cấp các mẫu (patterns) chuẩn để triển khai tính năng một cách nhất quán và hiệu quả.

### 1.1. Các Tầng Kiến trúc (Architecture Layers)
```
Domain Layer (Entities, Value Objects, Domain Events)
    ↑
Application Layer (Commands, Queries, Handlers, DTOs)
    ↑
Infrastructure Layer (DbContext, Repositories, External Services)
    ↑
Web Layer (Endpoints, Controllers, Middleware)
```

---

## 2. Cấu trúc & Quy ước (Structure & Conventions)

### 2.1. Quy ước Đặt tên (Naming Conventions)

#### C# Naming Standards
- **PascalCase**: Classes, methods, properties, namespaces, constants.
- **camelCase**: Variables, parameters, local variables, private fields.
- **UPPER_CASE**: Constants, static readonly fields, enum values.
- **snake_case**: PostgreSQL database objects, configuration keys.

### 2.2. Cấu trúc Thư mục (File Structure Patterns)
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

---

## 3. Mẫu Lớp Domain (Domain Layer Patterns)

### 3.1. Mẫu Entity (Entity Patterns)

#### Hướng dẫn Kế thừa Base Entity
Sử dụng `BaseEntity` cho các thực thể đơn giản và `BaseAuditableEntity` khi cần theo dõi lịch sử thay đổi.

```csharp
// Dùng BaseEntity cho các thực thể không yêu cầu audit.
public class SimpleEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    // Kế thừa: Id, DomainEvents, AddDomainEvent(), RemoveDomainEvent(), ClearDomainEvents()
}

// Dùng BaseAuditableEntity cho các thực thể cần theo dõi audit trail.
public class AuditableEntity : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    // Kế thừa: Id, Created, CreatedBy, LastModified, LastModifiedBy, DomainEvents
    // Các trường audit được tự động điền bởi AuditableEntityInterceptor.
}
```

#### Checklist khi Tạo Entity
```csharp
public class TodoItem : BaseAuditableEntity
{
    // 1. Thuộc tính bắt buộc
    public int ListId { get; set; }
    public string? Title { get; set; }
    
    // 2. Thuộc tính tùy chọn
    public string? Note { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime? Reminder { get; set; }
    
    // 3. Thuộc tính điều hướng (Navigation Properties)
    public TodoList List { get; set; } = null!;
    
    // 4. Trường private để đóng gói logic
    private bool _done;
    
    // 5. Thuộc tính public chứa logic nghiệp vụ
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
    
    // 6. Phương thức chứa logic nghiệp vụ
    public void MarkComplete()
    {
        if (Done) return;
        Done = true;
        AddDomainEvent(new TodoItemCompletedEvent(this));
    }

    // 7. Phương thức kiểm tra (Validation) (Tùy chọn)
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Title) && ListId > 0;
    }
}
```

### 3.2. Mẫu Cấu hình Entity (Entity Configuration Pattern)
Sử dụng `IEntityTypeConfiguration` để định nghĩa cấu trúc của entity trong database.

```csharp
// src/Infrastructure/Data/Configurations/TodoItemConfiguration.cs
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Cấu hình bảng
        builder.ToTable("TodoItems");
        
        // Khóa chính
        builder.HasKey(x => x.Id);
        
        // Cấu hình thuộc tính
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();
            
        // Cấu hình mối quan hệ
        builder.HasOne(x => x.List)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Cấu hình chỉ mục (Indexes)
        builder.HasIndex(x => x.ListId);
    }
}
```

### 3.3. Mẫu Sự kiện Domain (Domain Event Pattern)
Sử dụng Domain Events để xử lý các tác vụ phụ (side effects) sau khi một hành động trong domain hoàn tất.

#### Định nghĩa Domain Event
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

#### Xử lý Domain Event (Event Handler)
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
        
        // Xử lý các tác vụ phụ ở đây (ví dụ: gửi thông báo, cập nhật thống kê)
        await Task.CompletedTask;
    }
}
```

---

## 4. Mẫu Lớp Ứng dụng (Application Layer Patterns)

### 4.1. Command Pattern (Thao tác Ghi - Write Operations)

#### Command Cơ bản
```csharp
// Command
public record CreateTodoItemCommand : IRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
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
        var entity = new TodoItem { ListId = request.ListId, Title = request.Title };

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
        RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
        RuleFor(v => v.ListId).GreaterThan(0);
    }
}
```

#### Command Nâng cao (với Base Classes)
```csharp
// Base Request cho các tính năng enterprise
public abstract record BaseRequest<TResponse> : IRequest<TResponse>
{
    public Guid RequestId { get; init; } = Guid.NewGuid();
    public string? CorrelationId { get; init; }
    public string? UserId { get; init; }
}

// Command kế thừa từ BaseRequest
public record CreateTodoItemCommand : BaseRequest<int>
{
    public int ListId { get; init; }
    public string? Title { get; init; }
}

// Handler nâng cao với logging và error handling
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
            var entity = new TodoItem { ListId = request.ListId, Title = request.Title };
            _context.TodoItems.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created TodoItem {Id}", entity.Id);
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

### 4.2. Query Pattern (Thao tác Đọc - Read Operations)

#### Query Cơ bản
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

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(GetTodoItemsWithPaginationQuery request, CancellationToken ct)
    {
        return await _context.TodoItems
            .Where(x => x.ListId == request.ListId)
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize, ct);
    }
}
```

#### Query Nâng cao (với Filtering)
```csharp
// Query với các tham số lọc
public record GetTodoItemsQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public int ListId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchString { get; init; }
    public bool? Done { get; init; }
}

// Handler với logic lọc động
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

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(GetTodoItemsQuery request, CancellationToken ct)
    {
        var query = _context.TodoItems.Where(x => x.ListId == request.ListId);

        if (!string.IsNullOrEmpty(request.SearchString))
        {
            query = query.Where(x => x.Title.Contains(request.SearchString));
        }

        if (request.Done.HasValue)
        {
            query = query.Where(x => x.Done == request.Done.Value);
        }

        return await query
            .OrderBy(x => x.Title)
            .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize, ct);
    }
}
```

### 4.3. Mẫu GetById/GetDetail (Chi tiết đối tượng)

#### Query GetById Cơ bản
```csharp
// Query
public record GetTodoItemByIdQuery : IRequest<TodoItemDetailDto?>
{
    public int Id { get; init; }
}

// Handler
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, TodoItemDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<TodoItemDetailDto?> Handle(GetTodoItemByIdQuery request, CancellationToken ct)
    {
        var todoItem = await _context.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (todoItem == null) return null;

        return _mapper.Map<TodoItemDetailDto>(todoItem);
    }
}
```

#### Query GetById Nâng cao (với Result Pattern và Caching)
```csharp
// Query
public record GetTodoItemByIdQuery : IRequest<Result<TodoItemDetailDto>>
{
    public int Id { get; init; }
}

// Handler
public class GetTodoItemByIdQueryHandler : IRequestHandler<GetTodoItemByIdQuery, Result<TodoItemDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetTodoItemByIdQueryHandler> _logger;

    public async Task<Result<TodoItemDetailDto>> Handle(GetTodoItemByIdQuery request, CancellationToken ct)
    {
        try
        {
            var cacheKey = $"TodoItem:{request.Id}";
            var cachedResult = await _cacheService.GetAsync<TodoItemDetailDto>(cacheKey, ct);
            
            if (cachedResult != null)
            {
                _logger.LogInformation("Retrieved TodoItem {Id} from cache", request.Id);
                return Result<TodoItemDetailDto>.Success(cachedResult);
            }

            var todoItem = await _context.TodoItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (todoItem == null)
            {
                return Result<TodoItemDetailDto>.Failure("TodoItem not found.");
            }

            var dto = _mapper.Map<TodoItemDetailDto>(todoItem);
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), ct);
            
            return Result<TodoItemDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TodoItem with Id {Id}", request.Id);
            return Result<TodoItemDetailDto>.Failure("An error occurred.");
        }
    }
}
```

### 4.4. Mẫu Phân trang (Paging Patterns)

#### Phân trang Cơ bản
```csharp
// Lớp PaginatedList có sẵn trong template
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        // ... implementation ...
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        // ... implementation ...
    }
}

// Extension method để dễ sử dụng
public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(
        this IQueryable<TDestination> queryable, int pageNumber, int pageSize)
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize);
}
```

#### Phân trang Nâng cao (với Base Classes)
```csharp
// Base Request cho phân trang
public abstract record BasePagedRequest<TResponse> : BaseRequest<TResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string SortOrder { get; init; } = "asc";
}

// Base Response cho dữ liệu phân trang
public record PagedResponse<T> : BaseResponse
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
```

### 4.5. Mẫu DTO (Data Transfer Objects)
Sử dụng DTOs để định hình dữ liệu trả về cho client.

```csharp
// DTO cho danh sách (ít chi tiết)
public record TodoItemBriefDto
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
}

// DTO cho chi tiết (đầy đủ thông tin)
public record TodoItemDetailDto
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public bool Done { get; init; }
    public string? Note { get; init; }
    public DateTime Created { get; init; }
    public string? CreatedBy { get; init; }
}
```

### 4.6. Mẫu AutoMapper (AutoMapper Patterns)
Sử dụng `Profile` để cấu hình mapping giữa các object.

```csharp
public class TodoItemProfile : Profile
{
    public TodoItemProfile()
    {
        CreateMap<TodoItem, TodoItemBriefDto>();
        CreateMap<TodoItem, TodoItemDetailDto>();
        CreateMap<CreateTodoItemCommand, TodoItem>();
    }
}
```

---

## 5. Mẫu Lớp Web & API (Web & API Layer Patterns)

### 5.1. Mẫu Endpoint (Minimal APIs)
Sử dụng Minimal APIs cho các endpoint đơn giản, nhanh gọn.

```csharp
public class TodoItems : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTodoItemsWithPagination);
        groupBuilder.MapPost(CreateTodoItem);
        groupBuilder.MapPut(UpdateTodoItem, "{id}");
        groupBuilder.MapDelete(DeleteTodoItem, "{id}");
    }

    public async Task<PaginatedList<TodoItemBriefDto>> GetTodoItemsWithPagination(
        ISender sender, [AsParameters] GetTodoItemsWithPaginationQuery query)
    {
        return await sender.Send(query);
    }

    public async Task<int> CreateTodoItem(ISender sender, CreateTodoItemCommand command)
    {
        return await sender.Send(command);
    }
    
    // ... other methods
}
```

### 5.2. Mẫu Triển khai API (API Implementation Templates)

#### Mẫu FHIR Endpoints (BẮT BUỘC)
```csharp
public class FhirEndpoints : EndpointGroupBase
{
    public override string? GroupName => "fhir";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetResource, "{resourceType}")
            .WithName("GetFhirResource")
            .WithSummary("Get FHIR resources by type");
            
        groupBuilder.MapPost(CreateResource, "{resourceType}")
            .WithName("CreateFhirResource")
            .WithSummary("Create new FHIR resource");
    }
    
    public async Task<Results<Ok<Bundle>, NotFound, BadRequest<OperationOutcome>>> GetResource(
        ISender sender, 
        string resourceType, 
        [AsParameters] GetFhirResourceQuery query)
    {
        try
        {
            var result = await sender.Send(query);
            return TypedResults.Ok(result);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (ValidationException ex)
        {
            var operationOutcome = new OperationOutcome { /* ... */ };
            return TypedResults.BadRequest(operationOutcome);
        }
    }
}
```

#### Mẫu Business Controllers (KHUYẾN KHÍCH)
Sử dụng `ApiController` cho các logic nghiệp vụ phức tạp hơn.
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntityController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<EntityController> _logger;

    public EntityController(ISender sender, ILogger<EntityController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<EntityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedList<EntityDto>>> GetEntities([FromQuery] GetEntitiesQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities");
            return StatusCode(500, "An error occurred.");
        }
    }
}
```

#### Mẫu Special Operations (LINH HOẠT)
Dùng cho các endpoint đặc thù không theo chuẩn CRUD.
```csharp
public class FeatureEndpoints : EndpointGroupBase
{
    public override string? GroupName => "feature";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStatus, "status")
            .WithName("GetFeatureStatus")
            .WithTags("Feature");
    }
    
    public async Task<Ok<FeatureStatus>> GetStatus(ILogger<FeatureEndpoints> logger)
    {
        logger.LogInformation("Getting feature status");
        var status = new FeatureStatus { IsHealthy = true, Timestamp = DateTime.UtcNow };
        return TypedResults.Ok(status);
    }
}
```
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
