# FHIRAI - User Handling Guide

## 📋 **TABLE OF CONTENTS**
- [🎯 Overview](#-overview)
- [🔐 User System Architecture](#-user-system-architecture)
- [📋 Implementation Patterns](#-implementation-patterns)
- [✅ Validation Checklists](#-validation-checklists)
- [🚀 Quick Reference](#-quick-reference)
- [📚 Reference Files](#-reference-files)

## 🎯 **OVERVIEW**

This guide provides standardized patterns for handling user information (`IUser _user`) in FHIRAI applications based on Clean Architecture v9 template. It covers authentication, authorization, audit trails, and logging patterns that are essential for security and compliance.

### **Key Principles**
- **Consistent User Context**: All handlers must include user information
- **Security First**: Authorization checks before business logic
- **Audit Trail**: Complete tracking of user actions
- **Safe Access**: Null-safe user information access
- **Clean Architecture**: Follow dependency inversion principles

## 🔐 **USER SYSTEM ARCHITECTURE**

### **Clean Architecture v9 User Flow**
```
┌─────────────────────────────────────────────────────────────┐
│                    Web Layer (Presentation)                 │
├─────────────────────────────────────────────────────────────┤
│  CurrentUser Service (IUser implementation)                │
│  - Extracts user info from HTTP context                    │
│  - Provides Id and Roles                                    │
│  - Implements IUser interface                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  Application Layer                          │
├─────────────────────────────────────────────────────────────┤
│  AuthorizationBehaviour (MediatR Pipeline)                 │
│  - Checks [Authorize] attributes                           │
│  - Validates roles and policies                            │
│  - Throws UnauthorizedAccessException/ForbiddenAccessException │
├─────────────────────────────────────────────────────────────┤
│  Command/Query Handlers                                     │
│  - Inject IUser _user                                       │
│  - Use for business logic and audit                        │
│  - Include in logging                                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                Infrastructure Layer                         │
├─────────────────────────────────────────────────────────────┤
│  AuditableEntityInterceptor (EF Core)                      │
│  - Auto-populates CreatedBy/LastModifiedBy                 │
│  - Uses IUser for audit trail                              │
├─────────────────────────────────────────────────────────────┤
│  IdentityService                                            │
│  - User management operations                               │
│  - Policy-based authorization                               │
└─────────────────────────────────────────────────────────────┘
```

### **Core Components**

#### **IUser Interface (Application Layer)**
```csharp
namespace FHIRAI.Application.Common.Interfaces;

public interface IUser
{
    string? Id { get; }
    List<string>? Roles { get; }
}
```

#### **CurrentUser Service (Web Layer)**
```csharp
public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
}
```

#### **Dependency Injection Setup**
```csharp
// src/Web/DependencyInjection.cs
builder.Services.AddScoped<IUser, CurrentUser>();
builder.Services.AddHttpContextAccessor();
```

## 📋 **IMPLEMENTATION PATTERNS**

### **1. Dependency Injection Pattern (MANDATORY)**

#### **✅ CORRECT: Standard Handler Pattern**
```csharp
public class CreateEntityCommandHandler : IRequestHandler<CreateEntityCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;  // Always use IUser _user naming
    private readonly ILogger<CreateEntityCommandHandler> _logger;

    public CreateEntityCommandHandler(
        IApplicationDbContext context,
        IUser user,  // Inject IUser interface
        ILogger<CreateEntityCommandHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }
}
```

#### **❌ INCORRECT: Missing User Context**
```csharp
public class CreateEntityCommandHandler : IRequestHandler<CreateEntityCommand, int>
{
    private readonly IApplicationDbContext _context;
    // Missing IUser _user - NO AUDIT TRAIL!
    
    public CreateEntityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
}
```

### **2. User Information Access Patterns (MANDATORY)**

#### **✅ CORRECT: Safe User Information Access**
```csharp
public async Task<int> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
{
    // Get user information safely with fallbacks
    var userId = _user.Id ?? "system";
    var userRoles = _user.Roles ?? new List<string>();

    // Log with user context
    _logger.LogInformation("Creating entity for user {UserId} with roles {Roles}", 
        userId, string.Join(",", userRoles));

    // Use for business logic
    var entity = new Entity
    {
        Name = request.Name,
        CreatedBy = userId,  // Will be overridden by AuditableEntityInterceptor
    };

    // Authorization check
    if (!userRoles.Contains("Admin") && !CanCreateEntity(userId, request))
    {
        throw new ForbiddenAccessException("User does not have permission to create this entity");
    }

    _context.Entities.Add(entity);
    await _context.SaveChangesAsync(cancellationToken);

    return entity.Id;
}
```

#### **❌ INCORRECT: Unsafe User Access**
```csharp
public async Task<int> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
{
    // Unsafe - could throw null reference exception
    var userId = _user.Id;  // No null check!
    var userRoles = _user.Roles;  // No null check!
    
    // No user context in logs
    _logger.LogInformation("Creating entity");  // Missing user info!
    
    // No authorization check
    var entity = new Entity { Name = request.Name };
    _context.Entities.Add(entity);
    await _context.SaveChangesAsync(cancellationToken);
    
    return entity.Id;
}
```

### **3. Authorization Patterns (MANDATORY)**

**For detailed authorization patterns, see**: `SECURITY_GUIDE.md`

#### **✅ CORRECT: Role-based Authorization (Quick Reference)**
```csharp
private bool CanCreateEntity(string userId, CreateEntityCommand request)
{
    var userRoles = _user.Roles ?? new List<string>();
    
    // Admin can do everything
    if (userRoles.Contains("Admin")) return true;
    
    // Manager can create entities
    if (userRoles.Contains("Manager")) return true;
    
    // Check specific business rules
    return CheckUserOwnership(userId, request);
}
```

#### **✅ CORRECT: Policy-based Authorization (Quick Reference)**
```csharp
private async Task<bool> CanAccessEntity(string userId, int entityId)
{
    var userRoles = _user.Roles ?? new List<string>();
    
    // Admin can access everything
    if (userRoles.Contains("Admin")) return true;
    
    // Check entity ownership
    var entity = await _context.Entities
        .FirstOrDefaultAsync(e => e.Id == entityId);
    
    return entity?.CreatedBy == userId;
}
```

**For comprehensive authorization patterns, see**: `SECURITY_GUIDE.md`

### **4. Audit Trail Patterns (MANDATORY)**

#### **✅ CORRECT: Audit Trail with User Context**
```csharp
public async Task<int> Handle(UpdateEntityCommand request, CancellationToken cancellationToken)
{
    var userId = _user.Id ?? "system";
    
    var entity = await _context.Entities
        .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
    
    if (entity == null)
        throw new NotFoundException(nameof(Entity), request.Id);

    // Update entity (audit fields handled by AuditableEntityInterceptor)
    entity.Name = request.Name;
    entity.LastModifiedBy = userId;  // Will be overridden by interceptor
    
    await _context.SaveChangesAsync(cancellationToken);
    
    _logger.LogInformation("Entity {Id} updated by user {UserId}", request.Id, userId);
    
    return entity.Id;
}
```

#### **✅ CORRECT: Custom Audit Trail**
```csharp
public async Task<int> Handle(DeleteEntityCommand request, CancellationToken cancellationToken)
{
    var userId = _user.Id ?? "system";
    
    var entity = await _context.Entities
        .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
    
    if (entity == null)
        throw new NotFoundException(nameof(Entity), request.Id);

    // Create audit record before deletion
    var auditRecord = new EntityAuditLog
    {
        EntityId = entity.Id,
        Action = "DELETE",
        PerformedBy = userId,
        PerformedAt = DateTime.UtcNow,
        OldValues = JsonSerializer.Serialize(entity),
        NewValues = null
    };
    
    _context.EntityAuditLogs.Add(auditRecord);
    _context.Entities.Remove(entity);
    
    await _context.SaveChangesAsync(cancellationToken);
    
    _logger.LogInformation("Entity {Id} deleted by user {UserId}", request.Id, userId);
    
    return entity.Id;
}
```

### **5. Logging Patterns (MANDATORY)**

#### **✅ CORRECT: Comprehensive User Logging**
```csharp
public async Task<int> Handle(ProcessDataCommand request, CancellationToken cancellationToken)
{
    var userId = _user.Id ?? "system";
    var userRoles = _user.Roles ?? new List<string>();

    // Entry log with user context
    _logger.LogInformation("User {UserId} with roles {Roles} processing data {DataId}", 
        userId, string.Join(",", userRoles), request.DataId);

    try
    {
        // Business logic
        var result = await ProcessData(request, userId, cancellationToken);
        
        // Success log
        _logger.LogInformation("Data {DataId} successfully processed by user {UserId}", 
            request.DataId, userId);
        
        return result;
    }
    catch (ValidationException ex)
    {
        // Validation error log
        _logger.LogWarning("Validation error for data {DataId} by user {UserId}: {Message}", 
            request.DataId, userId, ex.Message);
        throw;
    }
    catch (Exception ex)
    {
        // Error log
        _logger.LogError(ex, "Error processing data {DataId} by user {UserId}", 
            request.DataId, userId);
        throw;
    }
}
```

#### **✅ CORRECT: Structured Logging with User Context**
```csharp
public async Task<int> Handle(ComplexOperationCommand request, CancellationToken cancellationToken)
{
    var userId = _user.Id ?? "system";
    var userRoles = _user.Roles ?? new List<string>();

    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
        ["UserId"] = userId,
        ["UserRoles"] = string.Join(",", userRoles),
        ["OperationId"] = request.OperationId,
        ["CorrelationId"] = request.CorrelationId
    });

    _logger.LogInformation("Starting complex operation");

    try
    {
        var result = await ExecuteComplexOperation(request, userId, cancellationToken);
        
        _logger.LogInformation("Complex operation completed successfully");
        
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Complex operation failed");
        throw;
    }
}
```

### **6. Error Handling Patterns (MANDATORY)**

#### **✅ CORRECT: User-Aware Error Handling**
```csharp
public async Task<int> Handle(GetEntityByIdQuery request, CancellationToken cancellationToken)
{
    var userId = _user.Id ?? "system";
    
    try
    {
        var entity = await _context.Entities
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Entity {Id} not found for user {UserId}", request.Id, userId);
            throw new NotFoundException(nameof(Entity), request.Id);
        }

        // Check access permissions
        if (!await HasAccessToEntity(userId, entity))
        {
            _logger.LogWarning("User {UserId} denied access to entity {Id}", userId, request.Id);
            throw new ForbiddenAccessException($"User {userId} does not have access to entity {request.Id}");
        }

        return entity.Id;
    }
    catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenAccessException)
    {
        _logger.LogError(ex, "Error retrieving entity {Id} for user {UserId}", request.Id, userId);
        throw;
    }
}
```

## ✅ **VALIDATION CHECKLISTS**

### **For All Handlers (MANDATORY):**
- [ ] `IUser _user` is injected in constructor
- [ ] User ID is safely accessed with null check (`_user.Id ?? "system"`)
- [ ] User roles are safely accessed with null check (`_user.Roles ?? new List<string>()`)
- [ ] User context is included in all log messages
- [ ] Authorization checks are performed before business logic
- [ ] Audit fields are properly set (handled by AuditableEntityInterceptor)
- [ ] Error handling includes user context
- [ ] Business logic respects user permissions
- [ ] No sensitive user data is logged
- [ ] User actions are logged for audit purposes

### **For FHIR Resources (MANDATORY):**
- [ ] User has permission to access FHIR resources
- [ ] User context is logged for all FHIR operations
- [ ] Audit trail includes user information
- [ ] Security labels respect user roles
- [ ] Patient data access follows privacy rules
- [ ] FHIR-specific authorization policies are enforced
- [ ] Resource ownership is validated
- [ ] Access logs are maintained for compliance

### **For Business Resources (RECOMMENDED):**
- [ ] User ownership is checked for entity access
- [ ] Role-based access control is implemented
- [ ] User actions are logged for audit
- [ ] Data access follows business rules
- [ ] User permissions are validated
- [ ] Entity relationships respect user context
- [ ] Business logic includes user validation
- [ ] Audit trail is maintained for all operations

### **For Special Operations (FLEXIBLE):**
- [ ] User has appropriate permissions for operation
- [ ] Rate limiting is enforced per user
- [ ] Operation logs include user context
- [ ] Error handling includes user information
- [ ] Performance monitoring includes user metrics
- [ ] Security checks are performed
- [ ] Audit trail is maintained
- [ ] User quotas are respected

## 🚀 **QUICK REFERENCE**

### **Standard Handler Template**
```csharp
public class [Entity]CommandHandler : IRequestHandler<[Entity]Command, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<[Entity]CommandHandler> _logger;

    public [Entity]CommandHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<[Entity]CommandHandler> logger)
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    public async Task<int> Handle([Entity]Command request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? "system";
        var userRoles = _user.Roles ?? new List<string>();

        _logger.LogInformation("Processing {Entity} for user {UserId}", nameof([Entity]), userId);

        // Authorization check
        if (!CanProcess[Entity](userId, userRoles, request))
        {
            throw new ForbiddenAccessException($"User {userId} cannot process {nameof([Entity])}");
        }

        // Business logic here...

        _logger.LogInformation("Successfully processed {Entity} for user {UserId}", nameof([Entity]), userId);
        return result;
    }
}
```

### **Quick Commands**
```bash
# Generate new command with user handling
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int

# Generate new query with user handling
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm
```

## 📚 **REFERENCE FILES**

### **For User Handling Implementation:**
- **API Pattern Rules**: `API_PATTERN_RULE.md` - User Handling Patterns section
- **Security Guide**: `SECURITY_GUIDE.md` - Authentication & authorization patterns
- **Code Patterns**: `CODE_PATTERNS.md` - Implementation patterns and best practices

### **For Clean Architecture:**
- **Architecture Guide**: `ARCHITECTURE_GUIDE.md` - Clean architecture patterns
- **Quick Reference**: `CURSOR_AI_QUICK_REFERENCE.md` - Common patterns and commands

### **For Development Workflow:**
- **Implementation Checklist**: `IMPLEMENTATION_CHECKLIST.md` - Step-by-step implementation guide
- **Documentation Structure**: `DOCUMENTATION_STRUCTURE.md` - AI navigation guide

---

**🎯 REMEMBER**: 
1. **ALWAYS** inject `IUser _user` in command/query handlers
2. **ALWAYS** use safe access patterns with null checks
3. **ALWAYS** include user context in all log messages
4. **ALWAYS** perform authorization checks before business logic
5. **ALWAYS** follow Clean Architecture v9 patterns
6. **NEVER** bypass user context for security or audit requirements

User context is essential for security, audit trails, and business logic. Follow these patterns consistently across your application to ensure compliance and maintainability.
