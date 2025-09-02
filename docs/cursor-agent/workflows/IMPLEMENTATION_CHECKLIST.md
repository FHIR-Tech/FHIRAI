# FHIRAI - Implementation Checklist

## 📋 Feature Implementation Checklist

### 🎯 Pre-Implementation Phase
- [ ] **Requirements Analysis**: Hiểu rõ yêu cầu business
- [ ] **Architecture Review**: Xác định layer và components cần thiết
- [ ] **Database Design**: Thiết kế entities và relationships
- [ ] **API Design**: Thiết kế endpoints và DTOs
- [ ] **Security Review**: Xác định authorization requirements
- [ ] **Testing Strategy**: Lập kế hoạch testing

### 🏗️ Domain Layer Implementation
- [ ] **Entity Creation**: Tạo domain entities
  - [ ] Inherit từ BaseEntity hoặc BaseAuditableEntity
  - [ ] Implement business logic trong entities
  - [ ] Add domain events cho state changes
  - [ ] Add validation logic
- [ ] **Value Objects**: Tạo value objects nếu cần
  - [ ] Implement IEquatable<T>
  - [ ] Add validation logic
  - [ ] Make immutable
- [ ] **Domain Events**: Tạo domain events
  - [ ] Inherit từ BaseEvent
  - [ ] Include relevant data
  - [ ] Add to entity khi cần
- [ ] **Domain Exceptions**: Tạo custom exceptions
  - [ ] Inherit từ appropriate base exception
  - [ ] Add meaningful error messages
- [ ] **Constants**: Add business constants
  - [ ] Roles và policies
  - [ ] Business rules constants

### 🔧 Application Layer Implementation
- [ ] **Commands**: Tạo write operations
  - [ ] Implement IRequest<TResponse>
  - [ ] Add validation attributes
  - [ ] Create command handler
  - [ ] Add authorization attributes
  - [ ] Implement business logic
- [ ] **Queries**: Tạo read operations
  - [ ] Implement IRequest<TResponse>
  - [ ] Add validation attributes
  - [ ] Create query handler
  - [ ] Add authorization attributes
  - [ ] Implement data retrieval logic
- [ ] **Validators**: Tạo FluentValidation validators
  - [ ] Validate input data
  - [ ] Add business rule validation
  - [ ] Provide meaningful error messages
- [ ] **DTOs**: Tạo data transfer objects
  - [ ] Create request DTOs
  - [ ] Create response DTOs
  - [ ] Add AutoMapper profiles
- [ ] **Event Handlers**: Tạo domain event handlers
  - [ ] Implement INotificationHandler<T>
  - [ ] Handle side effects
  - [ ] Log important events
- [ ] **Interfaces**: Tạo application contracts
  - [ ] Define service interfaces
  - [ ] Define repository interfaces
  - [ ] Add to IApplicationDbContext nếu cần

### 🏗️ Infrastructure Layer Implementation
- [ ] **Entity Configuration**: Configure EF Core
  - [ ] Create entity configuration class
  - [ ] Configure relationships
  - [ ] Configure indexes
  - [ ] Configure constraints
- [ ] **Repository Implementation**: Implement repositories
  - [ ] Implement repository interfaces
  - [ ] Add to ApplicationDbContext
  - [ ] Implement CRUD operations
- [ ] **Service Implementation**: Implement external services
  - [ ] Implement service interfaces
  - [ ] Add error handling
  - [ ] Add logging
- [ ] **Dependency Injection**: Configure DI
  - [ ] Register services trong DependencyInjection.cs
  - [ ] Configure lifetime scopes
  - [ ] Add configuration options

### 🌐 Presentation Layer Implementation
- [ ] **Endpoints**: Tạo API endpoints
  - [ ] Create endpoint class
  - [ ] Implement HTTP methods
  - [ ] Add authorization requirements
  - [ ] Handle errors properly
  - [ ] Return appropriate HTTP status codes
- [ ] **Services**: Tạo web services
  - [ ] Implement current user service nếu cần
  - [ ] Add middleware nếu cần
- [ ] **Configuration**: Update configuration
  - [ ] Update Program.cs nếu cần
  - [ ] Update appsettings.json nếu cần
  - [ ] Add environment variables nếu cần

### 🧪 Testing Implementation
- [ ] **Unit Tests**: Tạo unit tests
  - [ ] Test domain entities
  - [ ] Test value objects
  - [ ] Test commands và queries
  - [ ] Test validators
  - [ ] Test business logic
- [ ] **Integration Tests**: Tạo integration tests
  - [ ] Test database operations
  - [ ] Test external service integration
  - [ ] Test authentication/authorization
- [ ] **Functional Tests**: Tạo functional tests
  - [ ] Test complete workflows
  - [ ] Test API endpoints
  - [ ] Test error scenarios

### 🔒 Security Implementation
- [ ] **Authentication**: Configure authentication
  - [ ] Add JWT authentication nếu cần
  - [ ] Configure identity service
  - [ ] Add user management
- [ ] **Authorization**: Configure authorization
  - [ ] Add role-based authorization
  - [ ] Add policy-based authorization
  - [ ] Configure authorization attributes
- [ ] **Data Protection**: Implement data protection
  - [ ] Encrypt sensitive data
  - [ ] Add audit logging
  - [ ] Implement data retention policies

### 🗄️ Database Implementation
- [ ] **Migrations**: Create database migrations
  - [ ] Generate migration
  - [ ] Review migration script
  - [ ] Test migration
  - [ ] Update database
- [ ] **Indexing**: Add database indexes
  - [ ] Add performance indexes
  - [ ] Add unique constraints
  - [ ] Add foreign key constraints
- [ ] **Data Seeding**: Add seed data nếu cần
  - [ ] Create seed data
  - [ ] Add to migration
  - [ ] Test seed data
- [ ] **PostgreSQL Naming Conventions**: Follow strict naming rules
  - [ ] **Table Names**: Use snake_case, Plural, Quoted (e.g., "todo_items")
  - [ ] **Column Names**: Use snake_case, Quoted (e.g., "title", "created_at")
  - [ ] **Index Names**: Use snake_case pattern (e.g., "ix_todo_items_list_id")
  - [ ] **Constraint Names**: Use snake_case pattern (e.g., "fk_todo_items_todo_lists_list_id")
  - [ ] **PostgreSQL Features**: Use jsonb, UUID, arrays appropriately

### 📚 Documentation Implementation
- [ ] **Code Documentation**: Add code comments
  - [ ] Add XML comments cho public APIs
  - [ ] Add inline comments cho complex logic
  - [ ] Update README nếu cần
- [ ] **API Documentation**: Update API docs
  - [ ] Update OpenAPI specification
  - [ ] Add example requests/responses
  - [ ] Document error codes
- [ ] **Architecture Documentation**: Update architecture docs
  - [ ] Document design decisions
  - [ ] Update architecture diagrams
  - [ ] Document dependencies

### 🚀 Deployment Implementation
- [ ] **Configuration**: Update deployment config
  - [ ] Update Azure configuration
  - [ ] Add environment variables
  - [ ] Update connection strings
- [ ] **Infrastructure**: Update infrastructure code
  - [ ] Update Bicep templates nếu cần
  - [ ] Add new Azure resources nếu cần
  - [ ] Update resource naming
- [ ] **Monitoring**: Add monitoring
  - [ ] Add Application Insights
  - [ ] Add health checks
  - [ ] Add logging

## 📋 **WORKFLOW COMPLETION REPORTING (QUICK REFERENCE)**

**CRITICAL**: After completing ANY implementation task, Cursor AI MUST follow the workflow completion reporting guide.

### **Quick Reference**:
- **Primary Guide**: `docs/cursor-agent/workflows/WORKFLOW_COMPLETION_REPORTING.md`
- **Required Actions**: Generate reports, create logs, update summaries
- **Quality Standards**: Follow professional documentation standards
- **Integration**: All workflows must integrate with reporting requirements

**For complete workflow details, see**: `WORKFLOW_COMPLETION_REPORTING.md`

## 🔍 Code Review Checklist

### ✅ Code Quality
- [ ] **Clean Code**: Code dễ đọc và maintain
- [ ] **SOLID Principles**: Tuân thủ SOLID principles
- [ ] **DRY Principle**: Không duplicate code
- [ ] **Single Responsibility**: Mỗi class có một responsibility
- [ ] **Open/Closed Principle**: Open for extension, closed for modification

### ✅ Architecture Compliance
- [ ] **Layer Separation**: Không violate layer boundaries
- [ ] **Dependency Direction**: Dependencies flow inward
- [ ] **Interface Segregation**: Use specific interfaces
- [ ] **Dependency Inversion**: Depend on abstractions

### ✅ Security Review
- [ ] **Input Validation**: Validate all inputs
- [ ] **Authorization**: Proper authorization checks
- [ ] **Data Protection**: Sensitive data protected
- [ ] **Error Handling**: No information disclosure

### ✅ Performance Review
- [ ] **Async/Await**: Use async/await properly
- [ ] **Database Queries**: Optimized queries
- [ ] **Memory Usage**: Efficient memory usage
- [ ] **Caching**: Appropriate caching strategy

### ✅ Testing Review
- [ ] **Test Coverage**: Adequate test coverage
- [ ] **Test Quality**: Meaningful tests
- [ ] **Test Data**: Proper test data setup
- [ ] **Integration Tests**: End-to-end testing

## 🚨 Common Issues Checklist

### ❌ Architecture Issues
- [ ] **Circular Dependencies**: Check for circular references
- [ ] **Layer Violations**: Check layer boundaries
- [ ] **Tight Coupling**: Check for tight coupling
- [ ] **God Objects**: Check for classes doing too much

### ❌ Security Issues
- [ ] **SQL Injection**: Check for SQL injection vulnerabilities
- [ ] **XSS**: Check for XSS vulnerabilities
- [ ] **CSRF**: Check for CSRF protection
- [ ] **Information Disclosure**: Check for sensitive data exposure

### ❌ Performance Issues
- [ ] **N+1 Queries**: Check for N+1 query problems
- [ ] **Memory Leaks**: Check for memory leaks
- [ ] **Blocking Calls**: Check for blocking operations
- [ ] **Inefficient Algorithms**: Check for inefficient code

### ❌ Code Quality Issues
- [ ] **Code Duplication**: Check for duplicate code
- [ ] **Complex Methods**: Check for overly complex methods
- [ ] **Poor Naming**: Check for poor naming conventions
- [ ] **Missing Documentation**: Check for missing documentation

## 📊 Quality Gates

### ✅ Pre-Commit Checks
- [ ] **Build Success**: Solution builds successfully
- [ ] **Tests Pass**: All tests pass
- [ ] **Code Analysis**: No critical warnings
- [ ] **Style Compliance**: Code follows style guidelines

### ✅ Pre-Merge Checks
- [ ] **Code Review**: Code reviewed by team member
- [ ] **Integration Tests**: Integration tests pass
- [ ] **Performance Tests**: Performance benchmarks met
- [ ] **Security Scan**: Security scan passed

### ✅ Pre-Deployment Checks
- [ ] **Staging Deployment**: Successfully deployed to staging
- [ ] **Staging Tests**: All tests pass in staging
- [ ] **Performance Validation**: Performance validated in staging
- [ ] **Security Validation**: Security validated in staging

## 🔄 Maintenance Checklist

### ✅ Regular Maintenance
- [ ] **Dependency Updates**: Update dependencies regularly
- [ ] **Security Patches**: Apply security patches
- [ ] **Performance Monitoring**: Monitor performance metrics
- [ ] **Error Monitoring**: Monitor error rates

### ✅ Code Maintenance
- [ ] **Refactoring**: Regular code refactoring
- [ ] **Technical Debt**: Address technical debt
- [ ] **Documentation Updates**: Keep documentation current
- [ ] **Test Maintenance**: Maintain test quality

### ✅ Infrastructure Maintenance
- [ ] **Azure Updates**: Keep Azure resources updated
- [ ] **Database Maintenance**: Regular database maintenance
- [ ] **Backup Verification**: Verify backups regularly
- [ ] **Monitoring Alerts**: Review and update alerts

## 🎯 **FINAL COMPLETION CHECKLIST**

### **Before Marking Task Complete**:
- [ ] **All Implementation Items**: All above checkboxes completed
- [ ] **Workflow Reporting**: Follow `WORKFLOW_COMPLETION_REPORTING.md`
- [ ] **Documentation Updated**: All related documentation current
- [ ] **Cross-references Valid**: All document links working
- [ ] **Quality Standards Met**: All implementation standards satisfied
- [ ] **User Notified**: User informed of completion and next steps

### **Completion Confirmation**:
- [ ] **Task Status**: Marked as "Completed" in task tracking
- [ ] **Workflow Reports**: All required reports generated
- [ ] **Next Steps**: Clear next steps provided to user
- [ ] **Maintenance Notes**: Any future maintenance requirements documented

---

**Note**: Use this checklist for every feature implementation to ensure consistency và quality across the project. **ALWAYS follow workflow completion reporting** to maintain comprehensive audit trails and documentation standards.
