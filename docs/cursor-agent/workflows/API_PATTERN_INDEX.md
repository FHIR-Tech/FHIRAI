# API Pattern Rules - Complete Guide

## 🎯 **CURSOR AI AUTO-DISCOVERY GUIDE**

**IMPORTANT**: When starting a new conversation about API development, Cursor AI MUST:
1. **Read this file first** and apply the decision algorithm
2. **For healthcare/FHIR features**: Also read `HEALTHCARE_DATA_PATTERN_REFERENCE.md` for FHIR compliance requirements
3. **For complex features**: Check `CODE_PATTERNS.md` for implementation patterns
4. **For entity creation**: Check `FIELD_ORGANIZATION_PATTERN.md` for field organization standards

## 🚀 **QUICK DECISION ALGORITHM**

### Decision Matrix
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

### Priority Rules
1. **FHIR Keywords** = **HIGHEST PRIORITY** → Endpoints (MANDATORY)
2. **Business Keywords** = **MEDIUM PRIORITY** → Controllers (RECOMMENDED)
3. **Special Keywords** = **LOW PRIORITY** → Endpoints (FLEXIBLE)
4. **Default** = **Controllers** (DEFAULT)

## 📋 **IMPLEMENTATION TEMPLATES**

### FHIR Endpoints Template
```csharp
public class FhirEndpoints : EndpointGroupBase
{
    public override string? GroupName => "fhir";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetResource, "{resourceType}").RequireAuthorization();
        groupBuilder.MapGet(GetResourceById, "{resourceType}/{id}").RequireAuthorization();
        groupBuilder.MapPost(CreateResource, "{resourceType}").RequireAuthorization();
        groupBuilder.MapPut(UpdateResource, "{resourceType}/{id}").RequireAuthorization();
        groupBuilder.MapDelete(DeleteResource, "{resourceType}/{id}").RequireAuthorization();
    }
}
```

### Business Controllers Template
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
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
    [ProducesResponseType(typeof(IEnumerable<EntityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntities([FromQuery] GetEntitiesQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
```

### Special Operations Template
```csharp
public class FeatureEndpoints : EndpointGroupBase
{
    public override string? GroupName => "feature";
    
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetStatus, "");
        groupBuilder.MapPost(ProcessData, "process");
    }
}
```

## 🎯 **RESPONSE FORMAT**

When implementing, ALWAYS respond with this format:

```
🎯 **Pattern Decision**: [Controller/Endpoint]
📍 **File Location**: [Exact file path]
🛣️ **Route Pattern**: [Route structure]
📋 **Reason**: [Why this pattern was chosen]
⚡ **Priority**: [MANDATORY/RECOMMENDED/FLEXIBLE/DEFAULT]
🔍 **Keywords Detected**: [List of detected keywords]

📝 **Implementation**:
[Code implementation here]

🔧 **Validation Requirements**:
- [ ] [Specific validation items]

🔧 **Next Steps**:
- [ ] Create file structure
- [ ] Implement business logic
- [ ] Add tests
- [ ] Update documentation
```

## 📁 **FILE LOCATIONS**

### FHIR Resources
- **Location**: `src/Web/Endpoints/FhirEndpoints.cs`
- **Route**: `/fhir/{resourceType}`
- **Validation**: FHIR R4 compliance, proper validation, error handling

### Business Resources
- **Location**: `src/Web/Controllers/{Entity}Controller.cs`
- **Route**: `/api/{entity}`
- **Validation**: RESTful patterns, authorization, business validation

### Special Operations
- **Location**: `src/Web/Endpoints/{Feature}Endpoints.cs`
- **Route**: `/{feature}/*`
- **Validation**: Lightweight, fast response, minimal dependencies

## ✅ **VALIDATION CHECKLISTS**

### FHIR Resources:
- [ ] Route follows `/fhir/{resourceType}` pattern
- [ ] FHIR R4 compliant response format
- [ ] Proper FHIR resource validation
- [ ] FHIR-specific error handling (OperationOutcome)

### Business Resources:
- [ ] Route follows `/api/{entity}` pattern
- [ ] Proper authorization attributes
- [ ] Business logic validation
- [ ] Standard HTTP status codes

### Special Operations:
- [ ] Lightweight implementation
- [ ] Fast response times (< 100ms)
- [ ] Minimal dependencies
- [ ] Proper error handling

## 🔧 **IMPLEMENTATION STEPS**

### For Any API Feature Request:

1. **Extract Keywords** from user request
2. **Apply Decision Algorithm** using the matrix above
3. **Select Template** based on pattern decision
4. **Create Implementation** using appropriate template
5. **Follow Validation Checklist** for the chosen pattern
6. **Update Route Mapping** if needed
7. **Add Tests** and documentation

### Route Mapping Update (if needed):
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

### When Multiple Keywords Apply:
1. **FHIR keywords** → Override all others (MANDATORY)
2. **Business keywords** → Override special keywords (RECOMMENDED)
3. **Special keywords** → Lowest priority (FLEXIBLE)
4. **Default** → Controllers (DEFAULT)

### Route Conflicts:
- **FHIR routes**: Always `/fhir/*` (no overlap)
- **Business routes**: Always `/api/*` (no overlap)
- **Special routes**: Use specific prefixes like `/health/*`, `/auth/*`

## 📚 **REFERENCE FILES**

### Core Implementation Files:
- **Healthcare Data Patterns**: `HEALTHCARE_DATA_PATTERN_REFERENCE.md` - FHIR compliance and healthcare data standards
- **Architecture Guide**: `ARCHITECTURE_GUIDE.md` - Clean architecture patterns
- **Code Patterns**: `CODE_PATTERNS.md` - Implementation patterns and best practices
- **Field Organization**: `FIELD_ORGANIZATION_PATTERN.md` - Entity field organization standards
- **Security Guide**: `SECURITY_GUIDE.md` - Security and compliance requirements
- **Database Guide**: `DATABASE_GUIDE.md` - Database patterns and optimization

### Project Structure:
- **Domain**: `src/Domain/Entities/`
- **Application**: `src/Application/[FeatureName]/`
- **Infrastructure**: `src/Infrastructure/Data/`
- **Web**: `src/Web/Endpoints/` or `src/Web/Controllers/`

---

**🎯 REMEMBER**: Cursor AI MUST automatically apply these rules when implementing any API feature in a new conversation!
