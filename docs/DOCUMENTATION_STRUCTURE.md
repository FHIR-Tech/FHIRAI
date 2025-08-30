# FHIRAI - AI Navigation Guide

## 🎯 Purpose
This document provides Cursor AI with efficient navigation to project structure, development rules, and implementation patterns. **Focus on actionable information for development tasks.**

## 📁 Project Structure Overview

### Core Architecture Layers
```
src/
├── Domain/           # Business entities, value objects, events
├── Application/      # Commands, queries, handlers, DTOs  
├── Infrastructure/   # Database, external services, identity
└── Web/             # API endpoints, configuration
```

### Key Directories
```
src/
├── Domain/Entities/              # Create new entities here
├── Domain/ValueObjects/          # Create value objects here
├── Domain/Events/                # Create domain events here
├── Application/[FeatureName]/    # Create feature modules here
│   ├── Commands/                 # Write operations
│   ├── Queries/                  # Read operations
│   └── EventHandlers/            # Domain event handlers
├── Infrastructure/Data/          # Database context & configurations
├── Infrastructure/Identity/      # Authentication & authorization
└── Web/Endpoints/                # API endpoints
```

### Advanced Documentation Structure
```
docs/
├── DOCUMENTATION_STRUCTURE.md   # This file - AI Navigation Guide
├── CURSOR_AI_RULES.md          # Comprehensive development rules
├── CURSOR_AI_QUICK_REFERENCE.md # Quick reference guide
├── IMPLEMENTATION_CHECKLIST.md  # Implementation checklists
├── api/                         # API Documentation
│   ├── README.md               # API overview and navigation
│   ├── specifications/         # Technical specifications
│   ├── guides/                 # Usage guides and tutorials
│   ├── reports/                # API implementation reports
│   └── examples/               # Code examples and samples
├── architecture/               # Architecture documentation
│   ├── decisions/             # Architecture Decision Records (ADRs)
│   └── diagrams/              # System diagrams and flows
├── cursor-agent/               # Cursor Agent Reports & Documentation
│   ├── README.md              # Cursor Agent overview
│   ├── reports/               # Implementation reports
│   ├── logs/                  # Session logs and transcripts
│   ├── decisions/             # AI-generated architecture decisions
│   └── tasks/                 # Task tracking and milestones
└── deployment/                 # Deployment documentation
    ├── guides/                # Deployment guides
    └── configurations/        # Environment configs
```

### Sample Data & Scripts Organization
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

## 🔍 AI Task Navigation

### When Creating New Features
**Primary Reference**: `docs/IMPLEMENTATION_CHECKLIST.md`

#### 1. New Entity Creation
- **Domain Entity**: `src/Domain/Entities/`
- **Value Objects**: `src/Domain/ValueObjects/`
- **Domain Events**: `src/Domain/Events/`
- **Pattern**: Inherit from `BaseEntity` or `BaseAuditableEntity`
- **Example**: See `src/Domain/Entities/TodoItem.cs`
- **Samples**: Create sample data in `scripts/samples/resources/`

#### 2. Application Layer
- **Commands**: `src/Application/[FeatureName]/Commands/`
- **Queries**: `src/Application/[FeatureName]/Queries/`
- **Pattern**: Use MediatR `IRequest<TResponse>`
- **Example**: See `src/Application/TodoItems/Commands/CreateTodoItem/`

#### 3. Infrastructure Layer
- **Entity Config**: `src/Infrastructure/Data/Configurations/`
- **Database Context**: `src/Infrastructure/Data/ApplicationDbContext.cs`
- **Pattern**: Create `{EntityName}Configuration.cs`

#### 4. Web Layer
- **Endpoints**: `src/Web/Endpoints/`
- **Pattern**: Extend `EndpointGroupBase`
- **Example**: See `src/Web/Endpoints/TodoItems.cs`

#### 5. Documentation & Reports
- **API Specs**: `docs/api/specifications/{resource-type}_specification.md`
- **Implementation Report**: `docs/cursor-agent/reports/{feature}_{date}_report.md`
- **Session Log**: `docs/cursor-agent/logs/{session-type}_{date}_{time}.md`
- **ADR**: `docs/cursor-agent/decisions/ADR_{number}_{title}.md`

### When Implementing Security
**Primary Reference**: `docs/CURSOR_AI_RULES.md` → Security & Authentication Rules

- **Authorization**: Add `[Authorize]` attributes to commands/queries
- **Endpoints**: Add `.RequireAuthorization()` to endpoint mappings
- **Identity Service**: Use `IIdentityService` for policy checks
- **Pattern**: See `src/Application/Common/Security/AuthorizeAttribute.cs`

### When Working with Database
**Primary Reference**: `docs/CURSOR_AI_RULES.md` → Database Rules

- **Migrations**: `dotnet ef migrations add [MigrationName]`
- **Entity Config**: Create in `src/Infrastructure/Data/Configurations/`
- **DbContext**: Update `src/Infrastructure/Data/ApplicationDbContext.cs`
- **Pattern**: See `src/Infrastructure/Data/Configurations/TodoItemConfiguration.cs`

### When Creating API Endpoints
**Primary Reference**: `docs/CURSOR_AI_QUICK_REFERENCE.md` → Endpoint Pattern

- **Location**: `src/Web/Endpoints/`
- **Pattern**: Extend `EndpointGroupBase`, use `ISender`
- **Authorization**: Add `.RequireAuthorization()`
- **Example**: See `src/Web/Endpoints/TodoItems.cs`
- **API Docs**: Update `docs/api/specifications/`
- **Examples**: Add to `docs/api/examples/`

### When Creating Documentation
**Primary Reference**: `docs/CURSOR_AI_RULES.md` → Cursor Agent Documentation Rules

- **Use Templates**: Always use proper templates for reports
- **Include Metadata**: Date, Agent name, Session ID, Status, Duration
- **Follow Naming**: Use exact naming conventions
- **Update Index**: Always update this navigation guide
- **Cross-reference**: Link related documents

## 🏷️ Naming Conventions

### File Naming (Strict Patterns)
```
Domain Entities:     {EntityName}.cs
Entity Configs:      {EntityName}Configuration.cs
Commands:           {CommandName}Command.cs
Command Handlers:   {CommandName}CommandHandler.cs
Command Validators: {CommandName}CommandValidator.cs
Queries:            {QueryName}Query.cs
Query Handlers:     {QueryName}QueryHandler.cs
DTOs:               {EntityName}Dto.cs
ViewModels:         {EntityName}Vm.cs
Endpoints:          {EntityName}.cs
Services:           {ServiceName}Service.cs
Behaviours:         {BehaviourName}Behaviour.cs
Domain Events:      {EventName}Event.cs
Event Handlers:     {EventName}EventHandler.cs
Exceptions:         {ExceptionName}Exception.cs
Value Objects:      {ValueObjectName}.cs
Tests:              {TestName}Tests.cs
```

### Documentation Naming (Advanced Patterns)
```
Specifications:     {resource-type}_specification.md
Guides:            {feature}_guide.md
Reports:           {report-type}_{date}.md
Examples:          {use-case}_example.md
Samples:           {resource-type}_{purpose}_{date}.json
Logs:              {session-type}_{date}_{time}.md
Decisions:         ADR_{number}_{title}.md
```

### Directory Structure
```
Feature Module:     src/Application/{FeatureName}/
├── Commands/       # Write operations
├── Queries/        # Read operations
└── EventHandlers/  # Domain event handlers

Command Structure:  src/Application/{FeatureName}/Commands/{CommandName}/
├── {CommandName}.cs
└── {CommandName}CommandValidator.cs
```

### Code Naming
```
Classes/Methods/Properties: PascalCase
Variables/Parameters:       camelCase
Constants:                  UPPER_CASE
Database Objects:          snake_case
URL Paths:                 kebab-case
```

## 🚀 Quick Commands

### Development
```bash
# Build & Run
dotnet build -tl
cd ./src/Web/ && dotnet watch run

# Code Generation
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm

# Testing
dotnet test

# Database
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

### Infrastructure
```bash
# Azure
az login
azd up
azd deploy
```

## 📚 Documentation References

### For Development Tasks
- **Implementation Guide**: `docs/IMPLEMENTATION_CHECKLIST.md`
  - Complete step-by-step process for new features
  - Domain, Application, Infrastructure, Web layer implementation
  - Testing, Security, Database implementation

### For Rules & Patterns
- **Development Rules**: `docs/CURSOR_AI_RULES.md`
  - Architecture principles and layer responsibilities
  - Design patterns (CQRS, Repository, Factory)
  - Security, Database, Testing rules
  - Code quality standards
  - **Cursor Agent Documentation Rules**

### For Quick Reference
- **Quick Reference**: `docs/CURSOR_AI_QUICK_REFERENCE.md`
  - Common patterns and code examples
  - Command/Query/Endpoint templates
  - Security patterns and validation
  - Common mistakes to avoid

### For Documentation Creation
- **API Documentation**: `docs/api/`
  - Specifications, guides, examples, reports
- **Architecture Docs**: `docs/architecture/`
  - ADRs, diagrams, design decisions
- **Cursor Agent Docs**: `docs/cursor-agent/`
  - Implementation reports, session logs, decisions

## 🎯 Common Development Scenarios

### Scenario 1: Create New Entity with Full CRUD
1. **Domain**: Create entity in `src/Domain/Entities/`
2. **Application**: Generate commands/queries using template
3. **Infrastructure**: Add to DbContext and create configuration
4. **Web**: Create endpoints in `src/Web/Endpoints/`
5. **Samples**: Add sample data in `scripts/samples/resources/`
6. **Documentation**: Create API spec in `docs/api/specifications/`
7. **Report**: Generate implementation report in `docs/cursor-agent/reports/`
8. **Reference**: Follow `TodoItems` feature pattern

### Scenario 2: Add Authentication to Feature
1. **Commands/Queries**: Add `[Authorize]` attributes
2. **Endpoints**: Add `.RequireAuthorization()`
3. **Policies**: Define in `src/Domain/Constants/Policies.cs`
4. **Documentation**: Update security documentation
5. **Reference**: See existing authorized endpoints

### Scenario 3: Database Schema Changes
1. **Domain**: Update entity properties
2. **Infrastructure**: Create/update entity configuration
3. **Migration**: Generate and apply migration
4. **Samples**: Update sample data if needed
5. **Reference**: See existing entity configurations

### Scenario 4: Add New API Endpoint
1. **Application**: Create query/command if needed
2. **Web**: Add endpoint method to existing endpoint class
3. **Mapping**: Add to `Map()` method with authorization
4. **API Docs**: Update `docs/api/specifications/`
5. **Examples**: Add to `docs/api/examples/`
6. **Reference**: See `src/Web/Endpoints/TodoItems.cs`

### Scenario 5: Create Implementation Report
1. **Use Template**: `docs/cursor-agent/reports/template_implementation_report.md`
2. **Include Metadata**: Date, agent, session ID, status, duration
3. **Technical Details**: Architecture decisions, implementation approach
4. **Quality Metrics**: Patterns used, testing coverage, validation
5. **Next Steps**: Immediate actions, future improvements
6. **Cross-reference**: Link to related documents and code

## 🚨 Common Issues & Solutions

### Build Issues
- **Template Missing**: `dotnet new install Clean.Architecture.Solution.Template::9.0.12`
- **Package Conflicts**: Check `Directory.Packages.props`
- **Dependencies**: Ensure all projects reference correctly

### Runtime Issues
- **Database Connection**: Check `appsettings.Development.json`
- **Authentication**: Verify JWT configuration
- **Authorization**: Check policy definitions

### Code Issues
- **Naming**: Follow strict naming conventions above
- **Architecture**: Ensure layer separation
- **Dependencies**: Domain should not depend on Infrastructure

### Documentation Issues
- **Template Usage**: Always use proper templates for reports
- **Metadata**: Include all required metadata in reports
- **Naming**: Follow exact naming conventions
- **Structure**: Place files in correct directory structure
- **Cross-references**: Link related documents

## 📋 Quality Checklist

### Before Committing
- [ ] Follows naming conventions
- [ ] Has proper authorization
- [ ] Includes validation
- [ ] Has meaningful tests
- [ ] No circular dependencies
- [ ] Follows Clean Architecture principles
- [ ] Documentation updated
- [ ] Implementation report created
- [ ] Session log updated

### Code Review Points
- [ ] Layer responsibilities respected
- [ ] CQRS pattern followed
- [ ] Security implemented
- [ ] Error handling in place
- [ ] Performance considered
- [ ] Documentation complete
- [ ] Samples created
- [ ] API docs updated

### Documentation Quality
- [ ] Uses proper templates
- [ ] Includes complete metadata
- [ ] Follows naming conventions
- [ ] Cross-references maintained
- [ ] Technical details accurate
- [ ] Professional language used

---

**🎯 AI Instructions**: Use this guide to quickly locate relevant information and follow established patterns. Always reference the specific documentation files for detailed implementation guidance. When in doubt, follow the `TodoItems` feature as the reference pattern. **Document everything** and maintain comprehensive audit trails for all changes.
