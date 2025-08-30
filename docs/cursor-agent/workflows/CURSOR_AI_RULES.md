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

### ✅ Advanced Documentation Structure
```
docs/
├── DOCUMENTATION_STRUCTURE.md   # AI Navigation Guide
├── CURSOR_AI_RULES.md          # This file - Development rules
├── CURSOR_AI_QUICK_REFERENCE.md # Quick reference guide
├── IMPLEMENTATION_CHECKLIST.md  # Implementation checklists
├── api/                         # API Documentation
│   ├── README.md               # API overview and navigation
│   ├── specifications/         # Technical specifications
│   │   └── {resource-type}_specification.md
│   ├── guides/                 # Usage guides and tutorials
│   │   └── {feature}_guide.md
│   ├── reports/                # API implementation reports
│   │   └── {report-type}_{date}.md
│   └── examples/               # Code examples and samples
│       └── {use-case}_example.md
├── architecture/               # Architecture documentation
│   ├── decisions/             # Architecture Decision Records (ADRs)
│   └── diagrams/              # System diagrams and flows
├── cursor-agent/               # Cursor Agent Reports & Documentation
│   ├── README.md              # Cursor Agent overview
│   ├── reports/               # Implementation reports
│   │   ├── template_implementation_report.md
│   │   └── {feature}_{date}_report.md
│   ├── logs/                  # Session logs and transcripts
│   │   └── {session-type}_{date}_{time}.md
│   ├── decisions/             # AI-generated architecture decisions
│   │   └── ADR_{number}_{title}.md
│   └── tasks/                 # Task tracking and milestones
│       └── {task-type}_{date}_summary.md
└── deployment/                 # Deployment documentation
    ├── guides/                # Deployment guides
    └── configurations/        # Environment configs
```

### ✅ Sample Data & Scripts Organization
```
scripts/
├── samples/                    # Sample data organization
│   ├── bundles/               # FHIR Bundle samples
│   ├── resources/             # Individual resource samples
│   └── test-data/             # Test data samples
├── database/                  # Database scripts
├── api/                       # API testing scripts
└── deployment/                # Deployment scripts
```

### ✅ Advanced Naming Conventions
```
Documentation Files:
- Specifications: {resource-type}_specification.md
- Guides: {feature}_guide.md
- Reports: {report-type}_{date}.md
- Examples: {use-case}_example.md
- Samples: {resource-type}_{purpose}_{date}.json
- Logs: {session-type}_{date}_{time}.md
- Decisions: ADR_{number}_{title}.md
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

## 🤖 Cursor Agent Documentation Rules

### ✅ When Creating Documentation
- [ ] **Always use templates**: Use `docs/cursor-agent/reports/template_implementation_report.md` for implementation reports
- [ ] **Include metadata**: Date, Agent name, Session ID, Status, Duration
- [ ] **Follow naming conventions**: Use exact patterns specified in naming conventions
- [ ] **Update index**: Always update `docs/DOCUMENTATION_STRUCTURE.md` when adding new documentation
- [ ] **Cross-reference**: Link related documents and maintain traceability

### ✅ When Generating Reports
- [ ] **Use structured format**: Follow the template structure exactly
- [ ] **Include technical details**: Architecture decisions, implementation approach, challenges
- [ ] **Document code quality**: Patterns used, testing coverage, validation results
- [ ] **Track metrics**: Performance impact, success metrics, issues found
- [ ] **Provide next steps**: Immediate actions, future improvements, recommendations

### ✅ When Logging Sessions
- [ ] **Record session metadata**: Date, time, duration, agent version
- [ ] **Document decisions**: Architecture decisions, technology choices, implementation strategies
- [ ] **Track progress**: Task completion, milestone achievements, feature delivery
- [ ] **Maintain audit trail**: All changes, reasons, and outcomes

### ✅ Documentation Quality Standards
- [ ] **Completeness**: All sections of templates must be filled
- [ ] **Accuracy**: Technical details must be precise and verifiable
- [ ] **Clarity**: Use clear, professional language
- [ ] **Consistency**: Follow established patterns and conventions
- [ ] **Traceability**: Link to related documents and code changes

## 🎯 Cursor AI Specific Rules

### ✅ When Creating New Features
1. **Follow Clean Architecture**: Maintain layer separation
2. **Use CQRS Pattern**: Separate commands và queries
3. **Implement Validation**: Use FluentValidation
4. **Add Authorization**: Apply appropriate security
5. **Write Tests**: Unit và integration tests
6. **Update Documentation**: Keep documentation current
7. **Create Implementation Report**: Document the implementation process
8. **Update Session Logs**: Record decisions and progress

### ✅ When Modifying Existing Code
1. **Maintain Patterns**: Follow existing patterns
2. **Update Tests**: Ensure test coverage
3. **Check Dependencies**: Verify layer dependencies
4. **Validate Changes**: Test thoroughly
5. **Update Documentation**: Reflect changes in documentation
6. **Log Changes**: Record modifications in session logs

### ✅ When Adding New Dependencies
1. **Check Necessity**: Only add if truly needed
2. **Update Project Files**: Add to appropriate .csproj
3. **Configure DI**: Update DependencyInjection.cs
4. **Document Usage**: Update documentation
5. **Create ADR**: Document architecture decision if significant

### ✅ When Creating New Entities
1. **Inherit Base Classes**: Use BaseEntity hoặc BaseAuditableEntity
2. **Add Domain Events**: For important state changes
3. **Configure EF Core**: Add entity configuration
4. **Create DTOs**: For data transfer
5. **Add Validation**: Business rule validation
6. **Create Samples**: Add sample data in `/scripts/samples/`
7. **Update API Docs**: Add to API specifications

### ✅ When Creating New Endpoints
1. **Use Minimal APIs**: Follow existing endpoint patterns
2. **Add Authorization**: Apply appropriate security
3. **Handle Errors**: Proper error responses
4. **Document API**: Update OpenAPI documentation
5. **Create Examples**: Add usage examples in `/docs/api/examples/`
6. **Update Guides**: Add to feature guides if needed

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

### ❌ Documentation Violations
- [ ] **Create documentation without using proper templates**
- [ ] **Skip metadata in reports** (date, agent, status)
- [ ] **Use inconsistent naming conventions**
- [ ] **Fail to update documentation index**
- [ ] **Place API docs outside `/docs/api/` structure**
- [ ] **Place Cursor Agent reports outside `/docs/cursor-agent/` structure**
- [ ] **Create documentation without cross-references**
- [ ] **Skip technical details in implementation reports**
- [ ] **Place documentation files in root directory** (except README.md)
- [ ] **Mix sample data with source code**
- [ ] **Use generic names for sample files**
- [ ] **Create unstructured file hierarchies**

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

### ✅ Documentation Quality
- [ ] **Completeness**: All required sections filled
- [ ] **Accuracy**: Technical details precise and verifiable
- [ ] **Clarity**: Clear, professional language
- [ ] **Consistency**: Follow established patterns
- [ ] **Traceability**: Link to related documents

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

**Remember**: Always follow the established patterns và maintain consistency across the codebase. When in doubt, refer to existing implementations trong the project. **Document everything** and maintain comprehensive audit trails for all changes.
