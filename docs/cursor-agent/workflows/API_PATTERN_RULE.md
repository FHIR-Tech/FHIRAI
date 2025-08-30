# API Pattern Rules - Complete Guide

## 📋 **TABLE OF CONTENTS**
- [🎯 Cursor AI Auto-Discovery & Implementation Guide](#-cursor-ai-auto-discovery--implementation-guide)
- [📁 File Locations & Routes](#-file-locations--routes)
- [✅ Validation Checklists](#-validation-checklists)
- [🚀 Quick Commands](#-quick-commands)
- [📚 Reference Files](#-reference-files)
- [🎯 When to Reference Other Files](#-when-to-reference-other-files)
- [🎯 Implementation Workflow](#-implementation-workflow)
- [🚨 Conflict Resolution](#-conflict-resolution)
- [📁 Key File Locations](#-key-file-locations)
- [🏷️ Naming Patterns](#️-naming-patterns)

## 🎯 **CURSOR AI AUTO-DISCOVERY & IMPLEMENTATION GUIDE**

**CRITICAL**: When implementing any API feature, Cursor AI MUST follow this optimized workflow:

### **STEP 1: PATTERN DECISION (AUTO-DISCOVERY)**
```
IF "patient" OR "observation" OR "medication" OR "fhir" OR "healthcare" OR "medical" OR "clinical" OR "condition" OR "encounter" OR "diagnosis" OR "treatment"
    → USE Minimal API Endpoints (FHIR compliance - MANDATORY)
ELSE IF "user" OR "todo" OR "task" OR "admin" OR "system" OR "analytics" OR "report" OR "dashboard" OR "organization" OR "department"
    → USE Controllers (Business logic - RECOMMENDED)
ELSE IF "health" OR "webhook" OR "integration" OR "export" OR "import" OR "auth" OR "callback" OR "third-party"
    → USE Minimal API Endpoints (Special operations - FLEXIBLE)
ELSE
    → USE Controllers (Default)
```

### **STEP 2: IMPLEMENTATION TEMPLATES**

#### **FHIR Endpoints Template (MANDATORY)**
```csharp
public class FhirEndpoints : EndpointGroupBase
{
    public override string? GroupName => "fhir";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetResource, "{resourceType}")
            .RequireAuthorization()
            .WithName("GetFhirResource")
            .WithSummary("Get FHIR resources by type");
            
        groupBuilder.MapGet(GetResourceById, "{resourceType}/{id}")
            .RequireAuthorization()
            .WithName("GetFhirResourceById")
            .WithSummary("Get specific FHIR resource");
            
        groupBuilder.MapPost(CreateResource, "{resourceType}")
            .RequireAuthorization()
            .WithName("CreateFhirResource")
            .WithSummary("Create new FHIR resource");
            
        groupBuilder.MapPut(UpdateResource, "{resourceType}/{id}")
            .RequireAuthorization()
            .WithName("UpdateFhirResource")
            .WithSummary("Update existing FHIR resource");
            
        groupBuilder.MapDelete(DeleteResource, "{resourceType}/{id}")
            .RequireAuthorization()
            .WithName("DeleteFhirResource")
            .WithSummary("Delete FHIR resource");
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
            var operationOutcome = new OperationOutcome
            {
                Issue = new[] { new OperationOutcomeIssue { Severity = "error", Code = "invalid", Diagnostics = ex.Message } }
            };
            return TypedResults.BadRequest(operationOutcome);
        }
    }
}
```

#### **Business Controllers Template (RECOMMENDED)**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<EntityDto>>> GetEntities([FromQuery] GetEntitiesQuery query)
    {
        try
        {
            _logger.LogInformation("Getting entities with query: {@Query}", query);
            
            var result = await _sender.Send(query);
            
            if (result == null || !result.Items.Any())
            {
                _logger.LogWarning("No entities found for query: {@Query}", query);
                return NotFound();
            }
            
            _logger.LogInformation("Retrieved {Count} entities", result.Items.Count);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            return BadRequest(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Entity not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entities");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}
```

#### **Special Operations Template (FLEXIBLE)**
```csharp
public class FeatureEndpoints : EndpointGroupBase
{
    public override string? GroupName => "feature";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStatus, "")
            .WithName("GetFeatureStatus")
            .WithSummary("Get feature status")
            .WithTags("Feature");
            
        groupBuilder.MapPost(ProcessData, "process")
            .WithName("ProcessFeatureData")
            .WithSummary("Process feature data")
            .WithTags("Feature");
    }
    
    public async Task<Results<Ok<FeatureStatus>, BadRequest<string>>> GetStatus(
        ILogger<FeatureEndpoints> logger)
    {
        try
        {
            logger.LogInformation("Getting feature status");
            
            var status = new FeatureStatus
            {
                IsHealthy = true,
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            };
            
            return TypedResults.Ok(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting feature status");
            return TypedResults.BadRequest("Unable to get feature status");
        }
    }
    
    public async Task<Results<Ok<ProcessResult>, BadRequest<string>>> ProcessData(
        ProcessDataRequest request,
        ILogger<FeatureEndpoints> logger)
    {
        try
        {
            logger.LogInformation("Processing data: {@Request}", request);
            
            // Lightweight processing logic
            var result = new ProcessResult
            {
                Success = true,
                ProcessedAt = DateTime.UtcNow,
                DataCount = request.Data?.Length ?? 0
            };
            
            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing data");
            return TypedResults.BadRequest("Unable to process data");
        }
    }
}
```

### **STEP 3: RESPONSE FORMAT (MANDATORY)**
```
🎯 **Pattern Decision**: [Controller/Endpoint]
📍 **File Location**: [Exact file path]
🛣️ **Route Pattern**: [Route structure]
📋 **Reason**: [Why this pattern was chosen]
⚡ **Priority**: [MANDATORY/RECOMMENDED/FLEXIBLE/DEFAULT]
🔍 **Keywords Detected**: [List of detected keywords]

📝 **Implementation**:
[Code implementation here]

🔧 **Validation Checklist**:
- [ ] [Specific validation items for the chosen pattern]

🔧 **Next Steps**:
- [ ] Create file structure
- [ ] Implement business logic
- [ ] Add tests
- [ ] Update documentation
```

## 📁 **FILE LOCATIONS & ROUTES**

### **FHIR Resources (MANDATORY)**
- **Location**: `src/Web/Endpoints/FhirEndpoints.cs`
- **Route**: `/fhir/{resourceType}`
- **Validation**: FHIR R4 compliance, proper validation, error handling

### **Business Resources (RECOMMENDED)**
- **Location**: `src/Web/Controllers/{Entity}Controller.cs`
- **Route**: `/api/{entity}`
- **Validation**: RESTful patterns, authorization, business validation

### **Special Operations (FLEXIBLE)**
- **Location**: `src/Web/Endpoints/{Feature}Endpoints.cs`
- **Route**: `/{feature}/*`
- **Validation**: Lightweight, fast response, minimal dependencies

## ✅ **VALIDATION CHECKLISTS**

### **FHIR Resources (MANDATORY):**
- [ ] Route follows `/fhir/{resourceType}` pattern
- [ ] FHIR R4 compliant response format (Bundle, Resource, OperationOutcome)
- [ ] Proper FHIR resource validation (required fields, data types)
- [ ] FHIR-specific error handling (OperationOutcome with proper codes)
- [ ] Resource versioning support (ETag headers)
- [ ] Conditional requests support (If-Match, If-None-Match)
- [ ] Search parameters validation and processing
- [ ] Proper content negotiation (application/fhir+json)

### **Business Resources (RECOMMENDED):**
- [ ] Route follows `/api/{entity}` pattern
- [ ] Proper authorization attributes ([Authorize], policies)
- [ ] Business logic validation (FluentValidation)
- [ ] Standard HTTP status codes (200, 201, 400, 401, 403, 404, 500)
- [ ] Comprehensive logging (structured logging with correlation IDs)
- [ ] Input sanitization and validation
- [ ] Proper error responses with meaningful messages
- [ ] Pagination support for list endpoints
- [ ] Sorting and filtering capabilities

### **Special Operations (FLEXIBLE):**
- [ ] Lightweight implementation (minimal dependencies)
- [ ] Fast response times (< 100ms target)
- [ ] Minimal dependencies (avoid heavy frameworks)
- [ ] Proper error handling with fallback responses
- [ ] Health check endpoints for monitoring
- [ ] Rate limiting considerations
- [ ] Caching strategies where appropriate
- [ ] Async/await patterns for non-blocking operations

## 🚀 **QUICK COMMANDS**
```bash
# Build & Run
dotnet build -tl
cd ./src/Web/ && dotnet watch run

# Code Generation
dotnet new ca-usecase --name Create[Entity] --feature-name [FeatureName] --usecase-type command --return-type int
dotnet new ca-usecase -n Get[Entities] -fn [FeatureName] -ut query -rt [Entities]Vm

# Testing
dotnet test

# Azure Deployment
az login && azd up
```

## 📚 **REFERENCE FILES (ONLY WHEN NEEDED)**

### **For FHIR Implementation:**
- **Healthcare Data Patterns**: `HEALTHCARE_DATA_PATTERN_REFERENCE.md` - FHIR compliance requirements

### **For Complex Features:**
- **Code Patterns**: `CODE_PATTERNS.md` - Implementation patterns and best practices
- **Field Organization**: `FIELD_ORGANIZATION_PATTERN.md` - Entity field organization standards

### **For Architecture:**
- **Architecture Guide**: `ARCHITECTURE_GUIDE.md` - Clean architecture patterns
- **Security Guide**: `SECURITY_GUIDE.md` - Security and compliance requirements
- **Database Guide**: `DATABASE_GUIDE.md` - Database patterns and optimization

### **For Development Workflow:**
- **Cursor AI Rules**: `CURSOR_AI_RULES.md` - Core development rules and guidelines
- **Quick Reference**: `CURSOR_AI_QUICK_REFERENCE.md` - Quick reference for common patterns
- **Implementation Checklist**: `IMPLEMENTATION_CHECKLIST.md` - Step-by-step implementation guide
- **Documentation Structure**: `DOCUMENTATION_STRUCTURE.md` - AI navigation guide



## 🎯 **WHEN TO REFERENCE OTHER FILES**

### **Reference CURSOR_AI_RULES.md when:**
- Setting up new development environment
- Understanding core development principles
- Following project-wide guidelines

### **Reference CURSOR_AI_QUICK_REFERENCE.md when:**
- Need quick commands and patterns
- Looking for common code snippets
- Fast development reference

### **Reference IMPLEMENTATION_CHECKLIST.md when:**
- Implementing complex features
- Need step-by-step guidance
- Ensuring completeness of implementation

### **Reference DOCUMENTATION_STRUCTURE.md when:**
- Navigating documentation
- Understanding file organization
- Finding specific information

### **Reference HEALTHCARE_DATA_PATTERN_REFERENCE.md when:**
- Implementing FHIR resources
- Need FHIR compliance details
- Healthcare data standards

### **Reference CODE_PATTERNS.md when:**
- Complex implementation patterns
- Advanced coding techniques
- Best practices for specific scenarios

### **Reference FIELD_ORGANIZATION_PATTERN.md when:**
- Creating new entities
- Organizing entity fields
- Data structure design

### **Reference ARCHITECTURE_GUIDE.md when:**
- Understanding Clean Architecture
- Design patterns and principles
- System architecture decisions

### **Reference SECURITY_GUIDE.md when:**
- Authentication and authorization
- Security best practices
- Compliance requirements

### **Reference DATABASE_GUIDE.md when:**
- Database design and optimization
- Entity Framework configuration
- Data access patterns

## 🎯 **IMPLEMENTATION WORKFLOW**

### **For Any API Feature Request:**

1. **Extract Keywords** from user request
2. **Apply Decision Algorithm** using the matrix above
3. **Select Template** based on pattern decision
4. **Create Implementation** using appropriate template
5. **Follow Validation Checklist** for the chosen pattern
6. **Update Route Mapping** if needed
7. **Add Tests** and documentation

### **Route Mapping Update (if needed):**
```csharp
// In WebApplicationExtensions.cs
var routePrefix = instance.GroupName switch
{
    "fhir" => "/fhir",
    "health" => "/health",
    "auth" => "/auth",
    _ => $"/api/{instance.GroupName}"
};
```

## 🚨 **CONFLICT RESOLUTION**

### **When Multiple Keywords Apply:**
1. **FHIR keywords** → Override all others (MANDATORY)
2. **Business keywords** → Override special keywords (RECOMMENDED)
3. **Special keywords** → Lowest priority (FLEXIBLE)
4. **Default** → Controllers (DEFAULT)

### **Route Conflicts:**
- **FHIR routes**: Always `/fhir/*` (no overlap)
- **Business routes**: Always `/api/*` (no overlap)
- **Special routes**: Use specific prefixes like `/health/*`, `/auth/*`

## 📁 **KEY FILE LOCATIONS**
- **Domain Entities**: `src/Domain/Entities/`
- **Application Commands**: `src/Application/[FeatureName]/Commands/`
- **Application Queries**: `src/Application/[FeatureName]/Queries/`
- **API Endpoints**: `src/Web/Endpoints/`
- **API Controllers**: `src/Web/Controllers/`
- **Database Context**: `src/Infrastructure/Data/ApplicationDbContext.cs`
- **Infrastructure**: `infra/main.bicep`
- **API Documentation**: `docs/api/`
- **Implementation Reports**: `docs/cursor-agent/reports/`
- **Sample Data**: `scripts/samples/`
- **Cursor Agent Rules**: `docs/cursor-agent/workflows/CURSOR_AI_RULES.md`

## 🏷️ **NAMING PATTERNS**
- **Entities**: `{EntityName}.cs`
- **Commands**: `{CommandName}Command.cs`, `{CommandName}CommandHandler.cs`
- **Queries**: `{QueryName}Query.cs`, `{QueryName}QueryHandler.cs`
- **DTOs**: `{EntityName}Dto.cs`, `{EntityName}Vm.cs`
- **Endpoints**: `{EntityName}.cs`
- **Controllers**: `{EntityName}Controller.cs`
- **Configurations**: `{EntityName}Configuration.cs`
- **Reports**: `{report-type}_{date}.md`
- **Samples**: `{resource-type}_{purpose}_{date}.json`

---

**🎯 REMEMBER**: 
1. **Cursor AI MUST automatically apply these rules** when implementing any API feature
2. **This rule contains everything needed** for quick decision-making and implementation
3. **Reference other files ONLY when needed** for specific details (see "WHEN TO REFERENCE OTHER FILES" section)
4. **All workflow files are now properly referenced** for comprehensive development guidance