# FHIRAI - Cursor AI Quick Reference

## 🎯 Core Rules for Cursor AI

### ✅ Always Follow These Patterns

1. **Clean Architecture**: 4 layers (Domain → Application → Infrastructure → Web)
2. **CQRS**: Separate Commands (write) and Queries (read) using MediatR
3. **Dependency Inversion**: Domain never depends on Infrastructure
4. **SOLID Principles**: Apply all SOLID principles consistently

### 📁 File Structure Rules

```
src/
├── Domain/           # Business entities, value objects, events
├── Application/      # Commands, queries, handlers, DTOs
├── Infrastructure/   # Database, external services, identity
└── Web/             # API endpoints, configuration
```

### 🔧 Implementation Patterns

#### Creating New Entity
1. **Domain Layer**: Create entity in `src/Domain/Entities/`
2. **Application Layer**: Create commands/queries in `src/Application/[FeatureName]/`
3. **Infrastructure Layer**: Add to `ApplicationDbContext` and create configuration
4. **Web Layer**: Create endpoints in `src/Web/Endpoints/`

#### Command Pattern
```csharp
// Command
public record CreateEntityCommand : IRequest<int>
{
    public string Name { get; init; }
}

// Handler
public class CreateEntityCommandHandler : IRequestHandler<CreateEntityCommand, int>
{
    private readonly IApplicationDbContext _context;
    
    public async Task<int> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}

// Validator
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty();
    }
}
```

#### Query Pattern
```csharp
// Query
public record GetEntitiesQuery : IRequest<PaginatedList<EntityDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

// Handler
public class GetEntitiesQueryHandler : IRequestHandler<GetEntitiesQuery, PaginatedList<EntityDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public async Task<PaginatedList<EntityDto>> Handle(GetEntitiesQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

#### Endpoint Pattern
```csharp
public class Entities : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetEntities).RequireAuthorization();
        groupBuilder.MapPost(CreateEntity).RequireAuthorization();
    }
    
    public async Task<Results<Ok<PaginatedList<EntityDto>>, BadRequest<string>, NotFound>> GetEntities(
        ISender sender, 
        [AsParameters] GetEntitiesQuery query,
        ILogger<Entities> logger)
    {
        try
        {
            logger.LogInformation("Getting entities with query: {@Query}", query);
            
            var result = await sender.Send(query);
            
            if (result == null || !result.Items.Any())
            {
                logger.LogWarning("No entities found");
                return TypedResults.NotFound();
            }
            
            return TypedResults.Ok(result);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation error: {Message}", ex.Message);
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving entities");
            return TypedResults.BadRequest("An error occurred");
        }
    }
}
```

### 🛡️ Security Rules

1. **Always add authorization**: `.RequireAuthorization()` on endpoints
2. **Use authorization attributes**: `[Authorize(Roles = "Admin")]` on commands/queries
3. **Validate input**: Use FluentValidation for all inputs
4. **Handle errors**: Proper exception handling and HTTP status codes
5. **Input sanitization**: Validate and sanitize all user inputs
6. **Rate limiting**: Consider rate limiting for public endpoints
7. **CORS configuration**: Proper CORS setup for web clients
8. **HTTPS enforcement**: Always use HTTPS in production
9. **Audit logging**: Log security-relevant events
10. **Principle of least privilege**: Grant minimum required permissions

### 🧪 Testing Rules

1. **Unit tests**: Test commands, queries, validators
2. **Integration tests**: Test database operations
3. **Functional tests**: Test complete workflows
4. **Test coverage**: Maintain >80% coverage

### 🚨 Common Mistakes to Avoid

❌ **Don't**:
- Create circular dependencies between layers
- Put business logic in Infrastructure layer
- Skip validation on inputs
- Forget to add authorization
- Create "God objects" doing too much
- Use blocking calls in async methods

✅ **Do**:
- Follow existing naming conventions
- Add XML comments for public APIs
- Use async/await consistently
- Implement proper error handling
- Add meaningful tests
- Update documentation

### 🔄 Development Workflow

1. **Domain**: Create entities, value objects, events
2. **Application**: Create commands/queries with handlers
3. **Infrastructure**: Implement data access and external services
4. **Web**: Create API endpoints
5. **Testing**: Write unit and integration tests
6. **Documentation**: Update docs and comments

### 📚 Key Files to Reference

- `src/Application/TodoItems/` - Example feature implementation
- `src/Web/Endpoints/TodoItems.cs` - Example endpoints
- `src/Infrastructure/Data/ApplicationDbContext.cs` - Database context
- `src/Application/Common/Behaviours/` - Cross-cutting concerns

### 🛠️ Code Generation Commands

```bash
# Create new command
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int

# Create new query
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm
```

### 📊 Quality Checklist

- [ ] Solution builds successfully
- [ ] All tests pass (unit, integration, functional)
- [ ] No critical warnings or errors
- [ ] Follows naming conventions consistently
- [ ] Has proper authorization and security
- [ ] Includes comprehensive validation
- [ ] Has meaningful tests with good coverage
- [ ] Documentation updated and accurate
- [ ] Error handling implemented properly
- [ ] Logging configured appropriately
- [ ] Performance considerations addressed
- [ ] Security best practices followed
- [ ] Code follows SOLID principles
- [ ] No code duplication
- [ ] Proper async/await usage

---

**Remember**: When in doubt, look at existing implementations in the `TodoItems` feature as a reference pattern.
