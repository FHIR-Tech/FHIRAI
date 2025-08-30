# FHIRAI - Cursor AI Development Rules & Guidelines

## 📋 Project Overview

FHIRAI là một ứng dụng web được xây dựng theo **Clean Architecture pattern** với ASP.NET Core, sử dụng CQRS pattern và được deploy trên Azure cloud infrastructure. Dự án được tạo từ template Clean.Architecture.Solution.Template version 9.0.12.

## 🏗️ Core Development Rules

### ✅ Always Follow These Principles
- **Clean Architecture**: 4-layer pattern (Domain → Application → Infrastructure → Web)
- **CQRS**: Separate Commands (write) and Queries (read) using MediatR
- **Dependency Inversion**: Domain layer never depends on Infrastructure
- **SOLID Principles**: Apply all SOLID principles consistently
- **Domain-Driven Design**: Entities, Value Objects, Domain Events

### ✅ Layer Responsibilities
- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Orchestrates business operations, depends only on Domain
- **Infrastructure Layer**: Implements external concerns, depends on Application
- **Web Layer**: Handles HTTP concerns, depends on Application and Infrastructure

### ✅ File Organization
```
src/
├── Domain/           # Business entities, value objects, events
├── Application/      # Commands, queries, handlers, DTOs
├── Infrastructure/   # Database, external services, identity
└── Web/             # API endpoints, configuration
```

### ✅ Naming Conventions
- **PascalCase**: Classes, methods, properties, namespaces
- **camelCase**: Variables, parameters, local variables
- **UPPER_CASE**: Constants, static readonly fields
- **snake_case**: PostgreSQL database objects
- **Descriptive Names**: Self-documenting code

## 📚 Specialized Guides

### Architecture & Design
- **ARCHITECTURE_GUIDE.md**: Detailed architecture patterns and design principles
- **CODE_PATTERNS.md**: Implementation patterns and coding conventions
- **DATABASE_GUIDE.md**: Database design and Entity Framework configuration

### Security & Deployment
- **SECURITY_GUIDE.md**: Authentication, authorization, and security best practices
- **DEPLOYMENT_GUIDE.md**: Azure deployment and infrastructure management

### Development Workflow
- **CURSOR_AI_QUICK_REFERENCE.md**: Quick reference for common patterns
- **IMPLEMENTATION_CHECKLIST.md**: Step-by-step implementation guide
- **DOCUMENTATION_STRUCTURE.md**: AI navigation guide

## 🚨 Forbidden Practices

### ❌ Never Do
- Let Domain layer depend on EF Core or NHapi
- Bypass Application layer for direct database access
- Disable RLS or audit logging in production
- Hardcode connection strings or secrets
- Skip validation in handlers
- Use synchronous I/O in async contexts
- Log sensitive patient information
- Disable HTTPS in production
- Place documentation files in root directory (except README.md)
- Mix sample data with source code
- Use generic names for sample files
- Create unstructured file hierarchies

### ✅ Always Do
- Follow Clean Architecture principles
- Implement proper error handling
- Use dependency injection
- Write comprehensive tests
- Document public APIs
- Use secure coding practices
- Implement proper logging
- Follow FHIR standards
- Organize files in appropriate directories
- Use descriptive naming conventions
- Maintain clean project structure
- Keep documentation up-to-date

## 🎯 Cursor AI Specific Rules

### When Creating New Features
1. **Start with Domain Layer**: Create entities, value objects, and domain events
2. **Implement Application Layer**: Create commands, queries, and handlers
3. **Configure Infrastructure**: Add database configurations and external services
4. **Create Web Layer**: Implement API endpoints with proper authorization
5. **Write Tests**: Create unit, integration, and functional tests
6. **Update Documentation**: Generate implementation reports and update guides

### When Generating Code
- Use established patterns from existing code
- Follow naming conventions consistently
- Implement proper validation and error handling
- Add appropriate logging and monitoring
- Ensure security best practices are followed
- Create comprehensive tests

### When Creating Documentation
- Use proper templates for reports
- Include metadata (date, agent, session ID, status)
- Follow exact naming conventions
- Update documentation index immediately
- Cross-reference related documents
- Include technical details and metrics

## 📊 Quality Standards

### Code Quality
- **Cyclomatic Complexity**: ≤ 10 per method
- **Lines of Code**: ≤ 50 per method
- **Class Coupling**: Minimize dependencies
- **Depth of Inheritance**: ≤ 4 levels
- **Maintainability Index**: ≥ 65

### Testing Standards
- **Unit Test Coverage**: ≥ 80%
- **Integration Test Coverage**: ≥ 60%
- **Functional Test Coverage**: ≥ 40%
- **Performance Test**: Response time < 200ms
- **Security Test**: All security requirements met

### Documentation Standards
- **Completeness**: All sections of templates must be filled
- **Accuracy**: Technical details must be precise and verifiable
- **Clarity**: Use clear, professional language
- **Consistency**: Follow established patterns and conventions
- **Traceability**: Link to related documents and code changes

---

**🎯 Remember**: Always follow Clean Architecture principles, use CQRS pattern, implement proper validation and authorization, and maintain high test coverage. **Document everything** and maintain comprehensive audit trails for all changes. When in doubt, refer to specialized guides for detailed information.
