# FHIRAI - Documentation Structure & Navigation Guide

## 📁 Project Documentation Structure

### Root Level Documentation (`/`)
```
FHIRAI/
├── README.md                    # Project overview, setup, architecture overview
├── LICENSE                      # Project license information
├── global.json                  # .NET version specification
├── FHIRAI.sln                   # Solution file with project references
├── Directory.Build.props        # Common build properties
├── Directory.Packages.props     # Centralized package version management
├── build.cake                   # Build automation scripts
├── azure.yaml                   # Azure Developer CLI configuration
└── .editorconfig               # Code formatting and style rules
```

### Core Documentation (`/docs/`)
```
docs/
├── DOCUMENTATION_STRUCTURE.md   # This file - Documentation navigation guide
├── CURSOR_AI_RULES.md          # Comprehensive development rules & guidelines
│   ├── Architecture Checklist   # Clean Architecture compliance rules
│   ├── Layer Responsibilities   # Domain/Application/Infrastructure/Web layers
│   ├── Design Patterns          # CQRS, Repository, Factory, Observer patterns
│   ├── Security & Authentication # JWT, Authorization, Security headers
│   ├── Database Rules           # EF Core, PostgreSQL, Migrations
│   ├── Testing Rules            # Unit, Integration, Functional tests
│   ├── Deployment Rules         # Azure, Bicep, Configuration
│   ├── Code Quality Standards   # Naming, Documentation, Performance
│   ├── Development Workflow     # Feature development process
│   └── Cursor AI Specific Rules # AI-specific development guidelines
├── CURSOR_AI_QUICK_REFERENCE.md # Quick reference for common tasks
│   ├── Core Rules               # Essential patterns to follow
│   ├── File Structure Rules     # Directory organization patterns
│   ├── Implementation Patterns  # Command/Query/Endpoint patterns
│   ├── Security Rules           # Authentication/Authorization patterns
│   ├── Testing Rules            # Test structure and coverage
│   ├── Common Mistakes          # Anti-patterns to avoid
│   ├── Development Workflow     # Step-by-step development process
│   ├── Key Files Reference      # Important files and their purposes
│   ├── Code Generation          # Template commands for scaffolding
│   └── Quality Checklist        # Pre-commit quality gates
└── IMPLEMENTATION_CHECKLIST.md  # Detailed implementation guides
    ├── Pre-Implementation       # Requirements analysis, architecture review
    ├── Domain Layer             # Entity, Value Object, Event creation
    ├── Application Layer        # Command, Query, Validator implementation
    ├── Infrastructure Layer     # EF Core, Repository, Service implementation
    ├── Presentation Layer       # Endpoint, Service, Configuration
    ├── Testing Implementation   # Unit, Integration, Functional tests
    ├── Security Implementation  # Authentication, Authorization, Data protection
    ├── Database Implementation  # Migrations, Indexing, Data seeding
    ├── Documentation            # Code docs, API docs, Architecture docs
    ├── Deployment               # Configuration, Infrastructure, Monitoring
    ├── Code Review              # Quality, Architecture, Security, Performance
    ├── Common Issues            # Architecture, Security, Performance problems
    ├── Quality Gates            # Pre-commit, Pre-merge, Pre-deployment
    └── Maintenance              # Regular maintenance tasks
```

### Source Code Documentation (`/src/`)
```
src/
├── Domain/                      # Domain Layer Documentation
│   ├── Common/                  # Base classes and common patterns
│   │   ├── BaseEntity.cs        # Base entity with ID and audit fields
│   │   ├── BaseAuditableEntity.cs # Auditable entity with created/modified
│   │   ├── BaseEvent.cs         # Base domain event structure
│   │   └── ValueObject.cs       # Base value object implementation
│   ├── Constants/               # Business constants and policies
│   │   ├── Policies.cs          # Authorization policy definitions
│   │   └── Roles.cs             # User role definitions
│   ├── Entities/                # Domain entities
│   │   ├── TodoItem.cs          # Example entity implementation
│   │   └── TodoList.cs          # Example entity with relationships
│   ├── Events/                  # Domain events
│   │   ├── TodoItemCompletedEvent.cs # Example domain event
│   │   ├── TodoItemCreatedEvent.cs   # Example domain event
│   │   └── TodoItemDeletedEvent.cs   # Example domain event
│   ├── Exceptions/              # Domain-specific exceptions
│   │   └── UnsupportedColourException.cs # Example domain exception
│   └── ValueObjects/            # Value objects
│       └── Colour.cs            # Example value object implementation
├── Application/                 # Application Layer Documentation
│   ├── Common/                  # Cross-cutting concerns
│   │   ├── Behaviours/          # MediatR pipeline behaviors
│   │   │   ├── AuthorizationBehaviour.cs # Authorization middleware
│   │   │   ├── LoggingBehaviour.cs      # Request/response logging
│   │   │   ├── PerformanceBehaviour.cs  # Performance monitoring
│   │   │   ├── UnhandledExceptionBehaviour.cs # Error handling
│   │   │   └── ValidationBehaviour.cs   # Input validation
│   │   ├── Exceptions/          # Application-specific exceptions
│   │   ├── Interfaces/          # Application contracts
│   │   │   ├── IIdentityService.cs      # Identity service contract
│   │   │   ├── IUser.cs                 # User interface
│   │   │   └── IApplicationDbContext.cs # Database context interface
│   │   ├── Models/              # DTOs and ViewModels
│   │   ├── Mappings/            # AutoMapper profiles
│   │   └── Security/            # Security-related classes
│   │       └── AuthorizeAttribute.cs    # Custom authorization attribute
│   ├── TodoItems/               # Example feature module
│   │   ├── Commands/            # Write operations
│   │   │   ├── CreateTodoItem/  # Command with handler and validator
│   │   │   ├── UpdateTodoItem/  # Command with handler and validator
│   │   │   └── DeleteTodoItem/  # Command with handler and validator
│   │   ├── Queries/             # Read operations
│   │   │   ├── GetTodoItems/    # Query with handler
│   │   │   └── GetTodoItem/     # Query with handler
│   │   └── EventHandlers/       # Domain event handlers
│   ├── TodoLists/               # Example feature module
│   ├── Patients/                # FHIR-related feature
│   ├── FhirBundles/             # FHIR bundle operations
│   └── WeatherForecasts/        # Example feature module
├── Infrastructure/              # Infrastructure Layer Documentation
│   ├── Data/                    # Data access layer
│   │   ├── ApplicationDbContext.cs      # EF Core database context
│   │   ├── ApplicationDbContextInitialiser.cs # Database initialization
│   │   ├── Configurations/      # Entity configurations
│   │   │   ├── TodoItemConfiguration.cs # EF Core entity config
│   │   │   └── TodoListConfiguration.cs # EF Core entity config
│   │   └── Interceptors/        # EF Core interceptors
│   │       ├── AuditableEntitySaveChangesInterceptor.cs # Audit interceptor
│   │       └── DispatchDomainEventsInterceptor.cs # Domain event dispatcher
│   ├── Identity/                # Identity and authentication
│   │   ├── ApplicationUser.cs   # Custom user entity
│   │   ├── IdentityService.cs   # Identity service implementation
│   │   └── IdentityResultExtensions.cs # Identity result extensions
│   ├── Fhir/                    # FHIR service implementations
│   │   └── Services/            # FHIR-related services
│   └── DependencyInjection.cs   # Infrastructure DI configuration
└── Web/                         # Presentation Layer Documentation
    ├── Endpoints/               # API endpoints
    │   ├── TodoItems.cs         # TodoItems API endpoints
    │   ├── TodoLists.cs         # TodoLists API endpoints
    │   ├── Users.cs             # User management endpoints
    │   └── WeatherForecasts.cs  # Weather API endpoints
    ├── Services/                # Web-specific services
    │   └── CurrentUser.cs       # Current user service
    ├── Infrastructure/          # Web infrastructure
    ├── Program.cs               # Application entry point
    ├── DependencyInjection.cs   # Web DI configuration
    ├── appsettings.json         # Application configuration
    ├── appsettings.Development.json # Development configuration
    ├── config.nswag             # NSwag configuration
    └── Web.http                 # HTTP request examples
```

### Test Documentation (`/tests/`)
```
tests/
├── Application.UnitTests/       # Application layer unit tests
│   ├── Common/                  # Common test utilities
│   └── TodoItems/               # TodoItems feature tests
├── Domain.UnitTests/            # Domain layer unit tests
│   └── ValueObjects/            # Value object tests
├── Infrastructure.IntegrationTests/ # Infrastructure integration tests
└── Application.FunctionalTests/ # End-to-end functional tests
    ├── BaseTestFixture.cs       # Base test fixture
    ├── CustomWebApplicationFactory.cs # Custom web app factory
    ├── ITestDatabase.cs         # Test database interface
    ├── PostgreSQLTestcontainersTestDatabase.cs # Test database implementation
    ├── PostgreSQLTestDatabase.cs # Alternative test database
    ├── TestDatabaseFactory.cs   # Test database factory
    ├── Testing.cs               # Testing utilities
    ├── TodoItems/               # TodoItems functional tests
    └── TodoLists/               # TodoLists functional tests
```

### Infrastructure Documentation (`/infra/`)
```
infra/
├── main.bicep                   # Main infrastructure template
├── main.parameters.json         # Infrastructure parameters
├── abbreviations.json           # Naming convention abbreviations
├── core/                        # Core infrastructure components
│   ├── ai/                      # AI/ML infrastructure
│   │   ├── cognitiveservices.bicep # Azure Cognitive Services
│   │   ├── hub-dependencies.bicep  # AI Hub dependencies
│   │   ├── hub.bicep               # AI Hub configuration
│   │   └── project.bicep           # AI project setup
│   ├── config/                  # Configuration management
│   │   └── configstore.bicep    # Azure App Configuration
│   ├── database/                # Database infrastructure
│   │   ├── cosmos/              # Cosmos DB configuration
│   │   ├── mysql/               # MySQL configuration
│   │   ├── postgresql/          # PostgreSQL configuration
│   │   └── sqlserver/           # SQL Server configuration
│   ├── gateway/                 # API Gateway
│   │   └── apim.bicep           # Azure API Management
│   ├── host/                    # Hosting infrastructure
│   │   ├── ai-environment.bicep # AI environment setup
│   │   ├── aks-agent-pool.bicep # AKS agent pool
│   │   ├── aks-managed-cluster.bicep # AKS cluster
│   │   ├── aks.bicep            # AKS configuration
│   │   ├── appservice-appsettings.bicep # App Service settings
│   │   ├── appservice.bicep     # App Service configuration
│   │   ├── appserviceplan.bicep # App Service Plan
│   │   ├── container-app-upsert.bicep # Container App updates
│   │   ├── container-app.bicep  # Container App configuration
│   │   ├── container-apps-environment.bicep # Container Apps environment
│   │   ├── container-apps.bicep # Container Apps setup
│   │   ├── container-registry.bicep # Container Registry
│   │   ├── functions.bicep      # Azure Functions
│   │   └── staticwebapp.bicep   # Static Web App
│   ├── monitor/                 # Monitoring infrastructure
│   │   ├── applicationinsights-dashboard.bicep # App Insights dashboard
│   │   ├── applicationinsights.bicep # App Insights configuration
│   │   ├── loganalytics.bicep   # Log Analytics
│   │   └── monitoring.bicep     # Monitoring setup
│   ├── networking/              # Networking infrastructure
│   │   ├── cdn-endpoint.bicep   # CDN endpoint
│   │   ├── cdn-profile.bicep    # CDN profile
│   │   └── cdn.bicep            # CDN configuration
│   ├── search/                  # Search infrastructure
│   │   └── search-services.bicep # Azure Search Services
│   ├── security/                # Security infrastructure
│   │   ├── aks-managed-cluster-access.bicep # AKS access control
│   │   ├── configstore-access.bicep # Config store access
│   │   ├── keyvault-access.bicep # Key Vault access
│   │   ├── keyvault-secret.bicep # Key Vault secrets
│   │   ├── keyvault.bicep       # Key Vault configuration
│   │   ├── registry-access.bicep # Registry access control
│   │   └── role.bicep           # Role definitions
│   ├── storage/                 # Storage infrastructure
│   │   └── storage-account.bicep # Storage account
│   └── testing/                 # Testing infrastructure
│       └── loadtesting.bicep    # Load testing setup
└── services/                    # Service-specific infrastructure
    └── web.bicep                # Web service infrastructure
```

## 🏷️ Naming Conventions & Patterns

### File Naming Patterns
```
{EntityName}.cs                  # Domain entities
{EntityName}Configuration.cs     # EF Core entity configurations
{CommandName}Command.cs          # Application commands
{CommandName}CommandHandler.cs   # Command handlers
{CommandName}CommandValidator.cs # Command validators
{QueryName}Query.cs              # Application queries
{QueryName}QueryHandler.cs       # Query handlers
{EntityName}Dto.cs               # Data transfer objects
{EntityName}Vm.cs                # View models
{EntityName}.cs                  # API endpoints
{ServiceName}Service.cs          # Service implementations
{ServiceName}Service.cs          # Interface implementations
{BehaviourName}Behaviour.cs      # MediatR behaviors
{EventName}Event.cs              # Domain events
{EventName}EventHandler.cs       # Event handlers
{ExceptionName}Exception.cs      # Custom exceptions
{ValueObjectName}.cs             # Value objects
{TestName}Tests.cs               # Test classes
{ResourceType}.bicep             # Infrastructure templates
```

### Directory Naming Patterns
```
{FeatureName}/                   # Feature modules
├── Commands/                    # Write operations
├── Queries/                     # Read operations
├── EventHandlers/               # Domain event handlers
└── {CommandName}/               # Command-specific folders

{LayerName}/                     # Architecture layers
├── Common/                      # Shared components
├── {FeatureName}/               # Feature-specific components
└── {ComponentType}/             # Component types

{InfrastructureType}/            # Infrastructure components
├── {ServiceType}/               # Service-specific configurations
└── {ResourceType}.bicep         # Resource templates
```

### Code Naming Patterns
```
PascalCase                       # Classes, methods, properties, namespaces
camelCase                        # Variables, parameters, local variables
UPPER_CASE                       # Constants, static readonly fields
snake_case                       # PostgreSQL database objects
kebab-case                       # URL paths, file names
```

## 🔍 Information Location Mapping

### Architecture Information
- **Clean Architecture Overview**: `README.md` → Architecture section
- **Layer Responsibilities**: `docs/CURSOR_AI_RULES.md` → Layer Responsibilities
- **Design Patterns**: `docs/CURSOR_AI_RULES.md` → Design Patterns Checklist
- **CQRS Implementation**: `docs/CURSOR_AI_QUICK_REFERENCE.md` → Implementation Patterns

### Development Guidelines
- **Code Generation**: `docs/CURSOR_AI_QUICK_REFERENCE.md` → Code Generation Commands
- **Implementation Process**: `docs/IMPLEMENTATION_CHECKLIST.md` → Feature Implementation Checklist
- **Quality Standards**: `docs/CURSOR_AI_RULES.md` → Code Quality Standards
- **Testing Guidelines**: `docs/CURSOR_AI_RULES.md` → Testing Rules

### Security & Authentication
- **Authentication Flow**: `README.md` → Authentication Flow diagram
- **Security Rules**: `docs/CURSOR_AI_RULES.md` → Security & Authentication Rules
- **Authorization Patterns**: `docs/CURSOR_AI_QUICK_REFERENCE.md` → Security Rules

### Infrastructure & Deployment
- **Azure Infrastructure**: `README.md` → Azure Infrastructure section
- **Bicep Templates**: `infra/` → All infrastructure templates
- **Deployment Process**: `docs/IMPLEMENTATION_CHECKLIST.md` → Deployment Implementation

### API Development
- **Endpoint Patterns**: `docs/CURSOR_AI_QUICK_REFERENCE.md` → Endpoint Pattern
- **API Documentation**: `README.md` → API Documentation section
- **Example Implementations**: `src/Web/Endpoints/` → All endpoint examples

### Database & Data Access
- **Entity Framework**: `docs/CURSOR_AI_RULES.md` → Database Rules
- **Migration Process**: `docs/IMPLEMENTATION_CHECKLIST.md` → Database Implementation
- **Entity Configurations**: `src/Infrastructure/Data/Configurations/` → All entity configs

## 🎯 Quick Reference Commands

### Development Commands
```bash
# Build solution
dotnet build -tl

# Run web application
cd ./src/Web/ && dotnet watch run

# Run tests
dotnet test

# Generate new command
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int

# Generate new query
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm

# Install template
dotnet new install Clean.Architecture.Solution.Template::9.0.12
```

### Infrastructure Commands
```bash
# Azure login
az login

# Deploy infrastructure
azd up

# Deploy application
azd deploy
```

## 📋 Documentation Maintenance

### When Adding New Features
1. **Update Implementation Checklist**: Add new patterns to `docs/IMPLEMENTATION_CHECKLIST.md`
2. **Update Quick Reference**: Add common patterns to `docs/CURSOR_AI_QUICK_REFERENCE.md`
3. **Update Rules**: Add new guidelines to `docs/CURSOR_AI_RULES.md`
4. **Update This Index**: Add new documentation locations to this file

### When Modifying Architecture
1. **Update README**: Modify architecture diagrams and descriptions
2. **Update Rules**: Modify architecture rules in `docs/CURSOR_AI_RULES.md`
3. **Update Examples**: Ensure example implementations reflect changes
4. **Update Infrastructure**: Modify Bicep templates as needed

### When Adding New Documentation
1. **Follow Naming Conventions**: Use established naming patterns
2. **Update This Index**: Add new documentation to appropriate sections
3. **Cross-Reference**: Link related documentation sections
4. **Maintain Consistency**: Follow established documentation patterns

---

**🎯 Purpose**: This document serves as the master navigation guide for Cursor AI to understand the project's documentation structure, locate specific information, and follow established patterns when working with the FHIRAI codebase.

**📚 Related Documents**: 
- `docs/CURSOR_AI_RULES.md` - Comprehensive development rules
- `docs/CURSOR_AI_QUICK_REFERENCE.md` - Quick reference guide
- `docs/IMPLEMENTATION_CHECKLIST.md` - Implementation checklists
- `README.md` - Project overview and setup
