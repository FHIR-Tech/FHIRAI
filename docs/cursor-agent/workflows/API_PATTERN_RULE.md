# API Pattern Rules - Complete Guide

## 📋 **TABLE OF CONTENTS**
- [🎯 Cursor AI Auto-Discovery & Implementation Guide](#-cursor-ai-auto-discovery--implementation-guide)
- [📁 File Locations & Naming Patterns](#-file-locations--naming-patterns)
- [🚀 Quick Commands](#-quick-commands)
- [📚 Reference Files](#-reference-files)
- [🎯 Implementation Workflow](#-implementation-workflow)
- [🚨 Conflict Resolution](#-conflict-resolution)

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

**For detailed implementation templates, see**: `CODE_PATTERNS.md`

#### **Quick Reference Templates:**

**FHIR Endpoints (MANDATORY):**
```csharp
public class FhirEndpoints : EndpointGroupBase
{
    public override string? GroupName => "fhir";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetResource, "{resourceType}")
            .RequireAuthorization()
            .WithName("GetFhirResource");
    }
}
```

**Business Controllers (RECOMMENDED):**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntityController : ControllerBase
{
    private readonly ISender _sender;
    
    [HttpGet]
    public async Task<ActionResult<PaginatedList<EntityDto>>> GetEntities([FromQuery] GetEntitiesQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
```

**Special Operations (FLEXIBLE):**
```csharp
public class FeatureEndpoints : EndpointGroupBase
{
    public override string? GroupName => "feature";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStatus, "")
            .WithName("GetFeatureStatus");
    }
}
```

**For complete templates with error handling and validation, see**: `CODE_PATTERNS.md`
**For logging requirements and patterns, see**: `LOGGING_GUIDE.md`

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

🔧 **Next Steps**:
- [ ] Create file structure
- [ ] Implement business logic
- [ ] Add tests
- [ ] Update documentation
```

## 📁 **FILE LOCATIONS & NAMING PATTERNS**

### **FHIR Resources (MANDATORY)**
- **Location**: `src/Web/Endpoints/FhirEndpoints.cs`
- **Route**: `/fhir/{resourceType}`
- **Validation**: See `HEALTHCARE_DATA_PATTERN_REFERENCE.md`

### **Business Resources (RECOMMENDED)**
- **Location**: `src/Web/Controllers/{Entity}Controller.cs`
- **Route**: `/api/{entity}`
- **Validation**: See `IMPLEMENTATION_CHECKLIST.md`

### **Special Operations (FLEXIBLE)**
- **Location**: `src/Web/Endpoints/{Feature}Endpoints.cs`
- **Route**: `/{feature}/*`
- **Validation**: Lightweight, fast response, minimal dependencies

### **Naming Patterns**
- **Entities**: `{EntityName}.cs`
- **Commands**: `{CommandName}Command.cs`, `{CommandName}CommandHandler.cs`
- **Queries**: `{QueryName}Query.cs`, `{QueryName}QueryHandler.cs`
- **DTOs**: `{EntityName}Dto.cs`, `{EntityName}Vm.cs`
- **Endpoints**: `{EntityName}Endpoints.cs`
- **Controllers**: `{EntityName}Controller.cs`

### **Key File Locations**
- **Domain Entities**: `src/Domain/Entities/`
- **Application Commands**: `src/Application/[FeatureName]/Commands/`
- **Application Queries**: `src/Application/[FeatureName]/Queries/`
- **API Endpoints**: `src/Web/Endpoints/`
- **API Controllers**: `src/Web/Controllers/`
- **Database Context**: `src/Infrastructure/Data/ApplicationDbContext.cs`

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

### **For Implementation Templates:**
- **Code Patterns**: `CODE_PATTERNS.md` - **DETAILED** implementation templates with error handling and validation
- **Logging Guide**: `LOGGING_GUIDE.md` - Centralized logging requirements and patterns

### **For FHIR Implementation:**
- **Healthcare Data Patterns**: `HEALTHCARE_DATA_PATTERN_REFERENCE.md` - FHIR compliance requirements

### **For User & Security:**
- **User Handling**: `USER_HANDLING_GUIDE.md` - User context patterns and IUser implementation
- **Security Guide**: `SECURITY_GUIDE.md` - Authentication & authorization patterns

### **For Complex Features:**
- **Field Organization**: `FIELD_ORGANIZATION_PATTERN.md` - Entity field organization standards
- **Implementation Checklist**: `IMPLEMENTATION_CHECKLIST.md` - Step-by-step implementation guide

### **For Architecture:**
- **Architecture Guide**: `ARCHITECTURE_GUIDE.md` - Clean architecture patterns
- **Database Guide**: `DATABASE_GUIDE.md` - Database patterns and optimization

### **For Development Workflow:**
- **Cursor AI Rules**: `CURSOR_AI_RULES.md` - Core development rules and guidelines
- **Quick Reference**: `CURSOR_AI_QUICK_REFERENCE.md` - Quick reference for common patterns
- **Documentation Structure**: `DOCUMENTATION_STRUCTURE.md` - AI navigation guide

## 🎯 **IMPLEMENTATION WORKFLOW**

### **For Any API Feature Request:**

1. **Extract Keywords** from user request
2. **Apply Decision Algorithm** using the matrix above
3. **Select Template** based on pattern decision
4. **Create Implementation** using appropriate template
5. **Follow Reference Files** for detailed validation
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

---

**🎯 REMEMBER**: 
1. **This rule contains everything needed** for quick decision-making and implementation
2. **Reference other files ONLY when needed** for specific details
3. **Follow the implementation workflow** for consistent results