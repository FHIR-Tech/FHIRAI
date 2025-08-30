# FHIRAI - Cursor AI Development Rules & Guidelines

## 📋 Project Overview

FHIRAI là một ứng dụng web được xây dựng theo **Clean Architecture pattern** với ASP.NET Core, sử dụng CQRS pattern và được deploy trên Azure cloud infrastructure. Dự án được tạo từ template Clean.Architecture.Solution.Template version 9.0.12.

## 🏗️ Architecture Checklist

### ✅ Core Architecture Principles
- [ ] **Clean Architecture**: Tuân thủ 4-layer architecture (Domain, Application, Infrastructure, Presentation)
- [ ] **Dependency Inversion**: Domain layer không phụ thuộc vào Infrastructure
- [ ] **CQRS Pattern**: Sử dụng MediatR để tách Commands và Queries
- [ ] **SOLID Principles**: Áp dụng đầy đủ các nguyên tắc SOLID
- [ ] **Domain-Driven Design**: Entities, Value Objects, Domain Events

### ✅ Layer Responsibilities

#### Domain Layer (`src/Domain/`)
- [ ] **Entities**: Business objects với identity
- [ ] **Value Objects**: Immutable objects không có identity
- [ ] **Domain Events**: Events phát sinh từ domain logic
- [ ] **Domain Exceptions**: Custom exceptions cho business rules
- [ ] **Constants**: Roles, Policies, và business constants

#### Application Layer (`src/Application/`)
- [ ] **Commands**: Write operations với MediatR
- [ ] **Queries**: Read operations với MediatR
- [ ] **Event Handlers**: Xử lý domain events
- [ ] **Behaviours**: Cross-cutting concerns (Authorization, Validation, Logging)
- [ ] **Interfaces**: Contracts cho external dependencies
- [ ] **DTOs**: Data Transfer Objects

#### Infrastructure Layer (`src/Infrastructure/`)
- [ ] **Data Access**: Entity Framework Core với PostgreSQL
- [ ] **Identity**: ASP.NET Core Identity implementation
- [ ] **External Services**: FHIR services, external APIs
- [ ] **Configuration**: Dependency injection setup

#### Presentation Layer (`src/Web/`)
- [ ] **Endpoints**: Minimal API endpoints
- [ ] **Services**: Current user service, middleware
- [ ] **Configuration**: Program.cs, appsettings

## 📁 Directory Structure Rules

### ✅ Naming Conventions
- **PascalCase**: Classes, methods, properties, namespaces
- **camelCase**: Variables, parameters, local variables
- **UPPER_CASE**: Constants, static readonly fields
- **snake_case**: PostgreSQL database objects
- **Descriptive Names**: Self-documenting code

### ✅ File Organization
```
src/
├── Application/
│   ├── Common/
│   │   ├── Behaviours/          # Cross-cutting concerns
│   │   ├── Exceptions/          # Application exceptions
│   │   ├── Interfaces/          # Application contracts
│   │   ├── Models/              # DTOs, ViewModels
│   │   └── Security/            # Authorization attributes
│   └── [FeatureName]/           # Feature modules
│       ├── Commands/            # Write operations
│       │   └── [CommandName]/
│       │       ├── [CommandName].cs
│       │       └── [CommandName]Validator.cs
│       ├── Queries/             # Read operations
│       │   └── [QueryName]/
│       │       ├── [QueryName].cs
│       │       └── [QueryName]Validator.cs
│       └── EventHandlers/       # Domain event handlers
│
├── Domain/
│   ├── Common/                  # Base classes
│   ├── Constants/               # Business constants
│   ├── Entities/                # Domain entities
│   ├── Events/                  # Domain events
│   ├── Exceptions/              # Domain exceptions
│   └── ValueObjects/            # Value objects
│
├── Infrastructure/
│   ├── Data/                    # Database context, configurations
│   ├── Identity/                # Identity implementation
│   ├── Fhir/                    # FHIR services
│   └── DependencyInjection.cs   # DI configuration
│
└── Web/
    ├── Endpoints/               # API endpoints
    ├── Services/                # Web services
    ├── Infrastructure/          # Web infrastructure
    └── Program.cs               # Application entry point
```

## 🔧 Design Patterns Checklist

### ✅ CQRS Pattern
- [ ] **Commands**: Implement `IRequest<TResponse>` cho write operations
- [ ] **Queries**: Implement `IRequest<TResponse>` cho read operations
- [ ] **Handlers**: Implement `IRequestHandler<TRequest, TResponse>`
- [ ] **Validators**: FluentValidation cho input validation
- [ ] **MediatR**: Pipeline behaviors cho cross-cutting concerns

### ✅ Repository Pattern
- [ ] **Interface**: Define trong Application layer
- [ ] **Implementation**: Implement trong Infrastructure layer
- [ ] **Unit of Work**: Sử dụng DbContext

### ✅ Factory Pattern
- [ ] **Entity Creation**: Factory methods cho complex entities
- [ ] **Value Object Creation**: Factory methods cho validation

### ✅ Observer Pattern
- [ ] **Domain Events**: Sử dụng MediatR để publish events
- [ ] **Event Handlers**: Xử lý domain events

### ✅ Strategy Pattern
- [ ] **Behaviours**: Pipeline behaviors cho different strategies
- [ ] **Services**: Interface-based service implementations

## 🛡️ Security & Authentication Rules

### ✅ Authentication
- [ ] **Bearer Token**: JWT authentication
- [ ] **Identity Service**: ASP.NET Core Identity
- [ ] **User Context**: CurrentUser service
- [ ] **Claims**: User ID và roles từ JWT

### ✅ Authorization
- [ ] **AuthorizationBehaviour**: Middleware cho authorization
- [ ] **Role-based**: Kiểm tra user roles
- [ ] **Policy-based**: Custom policies với IIdentityService
- [ ] **AuthorizeAttribute**: Mark endpoints cần authorization

### ✅ Security Headers
- [ ] **HTTPS**: Force HTTPS trong production
- [ ] **CORS**: Configure CORS policies
- [ ] **Security Headers**: X-Frame-Options, X-Content-Type-Options

## 🗄️ Database Rules

### ✅ Entity Framework Core
- [ ] **DbContext**: ApplicationDbContext implement IApplicationDbContext
- [ ] **Configurations**: Entity configurations trong Infrastructure
- [ ] **Migrations**: Database migrations
- [ ] **Interceptors**: Audit interceptors

### ✅ PostgreSQL
- [ ] **Connection String**: Azure Key Vault integration
- [ ] **Naming**: snake_case cho database objects
- [ ] **Indexing**: Proper indexes cho performance

## 🧪 Testing Rules

### ✅ Test Structure
```
tests/
├── Application.UnitTests/       # Unit tests cho Application layer
├── Domain.UnitTests/            # Unit tests cho Domain layer
├── Infrastructure.IntegrationTests/  # Integration tests
└── Application.FunctionalTests/ # Functional tests
```

### ✅ Testing Standards
- [ ] **Unit Tests**: Test individual components
- [ ] **Integration Tests**: Test component interactions
- [ ] **Functional Tests**: Test complete workflows
- [ ] **Test Coverage**: Maintain high test coverage
- [ ] **Test Data**: Use test data builders

## 🚀 Deployment Rules

### ✅ Azure Infrastructure
- [ ] **Bicep Templates**: Infrastructure as Code
- [ ] **Resource Naming**: Consistent naming conventions
- [ ] **Environment Variables**: Azure Key Vault integration
- [ ] **Monitoring**: Application Insights setup

### ✅ Configuration
- [ ] **appsettings.json**: Development configuration
- [ ] **Azure Key Vault**: Production secrets
- [ ] **Environment Variables**: Runtime configuration

## 📝 Code Quality Standards

### ✅ Code Organization
- [ ] **Field Grouping**: Group related fields together
- [ ] **Method Organization**: Logical method ordering
- [ ] **Regions**: Use regions for logical separation
- [ ] **Consistent Ordering**: Maintain consistent ordering across classes

### ✅ Documentation
- [ ] **XML Comments**: All public APIs
- [ ] **README**: Comprehensive setup instructions
- [ ] **Architecture**: Document design decisions
- [ ] **API Documentation**: OpenAPI specifications

### ✅ Performance
- [ ] **Async/Await**: Use throughout cho I/O operations
- [ ] **Caching**: Implement appropriate caching strategies
- [ ] **Database Optimization**: Efficient queries và indexing
- [ ] **Memory Management**: Proper disposal of resources

## 🔄 Development Workflow

### ✅ Feature Development
1. **Domain Layer**: Define entities, value objects, events
2. **Application Layer**: Create commands/queries và handlers
3. **Infrastructure Layer**: Implement external dependencies
4. **Presentation Layer**: Create endpoints
5. **Testing**: Write unit và integration tests

### ✅ Code Generation
```bash
# Tạo command mới
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int

# Tạo query mới
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm
```

### ✅ Validation Rules
- [ ] **FluentValidation**: Input validation cho commands/queries
- [ ] **Domain Validation**: Business rule validation
- [ ] **Error Handling**: Proper exception handling

## 🎯 Cursor AI Specific Rules

### ✅ When Creating New Features
1. **Follow Clean Architecture**: Maintain layer separation
2. **Use CQRS Pattern**: Separate commands và queries
3. **Implement Validation**: Use FluentValidation
4. **Add Authorization**: Apply appropriate security
5. **Write Tests**: Unit và integration tests
6. **Update Documentation**: Keep documentation current

### ✅ When Modifying Existing Code
1. **Maintain Patterns**: Follow existing patterns
2. **Update Tests**: Ensure test coverage
3. **Check Dependencies**: Verify layer dependencies
4. **Validate Changes**: Test thoroughly

### ✅ When Adding New Dependencies
1. **Check Necessity**: Only add if truly needed
2. **Update Project Files**: Add to appropriate .csproj
3. **Configure DI**: Update DependencyInjection.cs
4. **Document Usage**: Update documentation

### ✅ When Creating New Entities
1. **Inherit Base Classes**: Use BaseEntity hoặc BaseAuditableEntity
2. **Add Domain Events**: For important state changes
3. **Configure EF Core**: Add entity configuration
4. **Create DTOs**: For data transfer
5. **Add Validation**: Business rule validation

### ✅ When Creating New Endpoints
1. **Use Minimal APIs**: Follow existing endpoint patterns
2. **Add Authorization**: Apply appropriate security
3. **Handle Errors**: Proper error responses
4. **Document API**: Update OpenAPI documentation

## 🚨 Common Pitfalls to Avoid

### ❌ Anti-patterns
- [ ] **Circular Dependencies**: Between layers
- [ ] **Anemic Domain Model**: Business logic in services
- [ ] **God Objects**: Classes doing too much
- [ ] **Tight Coupling**: Direct dependencies between layers
- [ ] **Hard-coded Values**: Use configuration

### ❌ Security Issues
- [ ] **SQL Injection**: Use parameterized queries
- [ ] **XSS**: Validate và sanitize input
- [ ] **CSRF**: Use anti-forgery tokens
- [ ] **Information Disclosure**: Don't expose sensitive data

### ❌ Performance Issues
- [ ] **N+1 Queries**: Use Include() và proper loading
- [ ] **Memory Leaks**: Dispose resources properly
- [ ] **Blocking Calls**: Use async/await consistently
- [ ] **Inefficient Queries**: Optimize database queries

## 📊 Quality Metrics

### ✅ Code Quality
- [ ] **Test Coverage**: >80% coverage
- [ ] **Code Complexity**: Keep methods simple
- [ ] **Documentation**: All public APIs documented
- [ ] **Static Analysis**: No critical warnings

### ✅ Performance
- [ ] **Response Time**: <200ms cho typical requests
- [ ] **Memory Usage**: Monitor memory consumption
- [ ] **Database Queries**: Optimize query performance
- [ ] **Scalability**: Design for horizontal scaling

### ✅ Security
- [ ] **Authentication**: Proper user authentication
- [ ] **Authorization**: Role và policy-based access
- [ ] **Data Protection**: Encrypt sensitive data
- [ ] **Audit Trail**: Log security events

## 🔧 Tools & Extensions

### ✅ Required Tools
- [ ] **.NET 9.0**: Development framework
- [ ] **Entity Framework Core**: ORM
- [ ] **MediatR**: CQRS implementation
- [ ] **FluentValidation**: Input validation
- [ ] **AutoMapper**: Object mapping
- [ ] **NSwag**: API documentation

### ✅ Development Tools
- [ ] **Visual Studio 2022**: IDE
- [ ] **Azure CLI**: Azure management
- [ ] **PostgreSQL**: Database
- [ ] **Git**: Version control

## 📚 Additional Resources

- [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)

---

**Remember**: Always follow the established patterns và maintain consistency across the codebase. When in doubt, refer to existing implementations trong the project.
