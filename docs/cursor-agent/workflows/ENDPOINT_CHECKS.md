# Endpoint Testing & Validation Guide

## 🎯 **PURPOSE & SCOPE**
- **Standardize endpoint testing procedures** for all API implementations
- **Automate validation** with standardized scripts and templates
- **Ensure FHIR compliance** for healthcare endpoints
- **Integrate with existing workflow rules** and validation patterns

## 🧰 **TESTING TOOLS & SETUP**

### **Primary Tools (Choose One)**
- **VS Code REST Client**: Open `.http` files and execute by `@name` (Recommended)
- **curl**: Quick smoke tests: `curl -k https://localhost:5001/api/WeatherForecasts`
- **Local API Startup**: `dotnet run --project src/Web --launch-profile FHIRAI.Web`

### **Port Configuration**
- **HTTPS**: Port 5001 (Primary)
- **HTTP**: Port 5000 (Fallback)
- **Profile**: FHIRAI.Web

## 🚀 **QUICK TESTING WORKFLOW**

### **Step 1: API Startup**
```bash
# Check port usage
netstat -an | findstr :5001

# Start API (if not running)
dotnet run --project src/Web --launch-profile FHIRAI.Web
```

### **Step 2: Execute Tests**
- **VS Code REST Client**: Open `.http` file and run by `@name`
- **Automated Scripts**: Use provided validation scripts
- **Manual Testing**: Execute individual requests as needed

### **Step 3: Validate Results**
Expected outcomes:
- ✅ **PASS**: Endpoint working correctly
- ❌ **FAIL**: Endpoint errors (status code, JSON validation)
- ⏭️ **SKIP**: Endpoint not implemented (e.g., `/health`, `/metadata`)

**Requirement**: All FHIR resources must **PASS** (Search, Create, Read, Update, Delete).

## 📄 **HTTP FILE TEMPLATES**

### **Standard Template: `scripts/http/_TEMPLATE.http`**
Use this template for creating new `.http` files for new resources:

```http
@Web_HostAddress = https://localhost:5001
@BearerToken = <YourToken>

# Resource-specific variables
@ResourceId = <resource-id>
@ResourceVersion = <version-id>

### @name resource_search
GET {Web_HostAddress}/api/resource?page=1&pageSize=20
Authorization: Bearer {{BearerToken}}
Accept: application/json

### @name resource_create
POST {Web_HostAddress}/api/resource
Authorization: Bearer {{BearerToken}}
Content-Type: application/json
Accept: application/json

{
  "name": "Test Resource",
  "description": "Test description"
}

### @name resource_read
GET {Web_HostAddress}/api/resource/{ResourceId}
Authorization: Bearer {{BearerToken}}
Accept: application/json

### @name resource_update
PUT {Web_HostAddress}/api/resource/{ResourceId}
Authorization: Bearer {{BearerToken}}
Content-Type: application/json
Accept: application/json

{
  "name": "Updated Resource",
  "description": "Updated description"
}

### @name resource_delete
DELETE {Web_HostAddress}/api/resource/{ResourceId}
Authorization: Bearer {{BearerToken}}
```

### **FHIR Template: `scripts/http/_FHIR_TEMPLATE.http`**
For FHIR-compliant endpoints:

```http
@Web_HostAddress = https://localhost:5001
@BearerToken = <YourToken>

# FHIR-specific variables
@PatientId = <patient-id>
@PatientVersion = <version-id>
@IdSystem = https://example.org/mrn
@IdValue = MRN-00001

### @name patient_search
GET {Web_HostAddress}/fhir/Patient?_count=10
Authorization: Bearer {{BearerToken}}
Accept: application/fhir+json

### @name patient_create
POST {Web_HostAddress}/fhir/Patient
Authorization: Bearer {{BearerToken}}
Content-Type: application/fhir+json
Accept: application/fhir+json
Prefer: return=representation

{
  "resourceType": "Patient",
  "identifier": [
    {
      "system": "{{IdSystem}}",
      "value": "{{IdValue}}"
    }
  ],
  "name": [
    {
      "use": "official",
      "text": "John Doe"
    }
  ]
}

### @name patient_read
GET {Web_HostAddress}/fhir/Patient/{PatientId}
Authorization: Bearer {{BearerToken}}
Accept: application/fhir+json

### @name patient_update
PUT {Web_HostAddress}/fhir/Patient/{PatientId}
Authorization: Bearer {{BearerToken}}
Content-Type: application/fhir+json
Accept: application/fhir+json
If-Match: W/"{PatientVersion}"

{
  "resourceType": "Patient",
  "id": "{PatientId}",
  "identifier": [
    {
      "system": "{{IdSystem}}",
      "value": "{{IdValue}}"
    }
  ],
  "name": [
    {
      "use": "official",
      "text": "John Doe Updated"
    }
  ]
}
```

## 🌍 **ENVIRONMENT VARIABLES & CONFIGURATION**

### **Variable Declaration Priority (Highest to Lowest)**
1. **Local variables** in `.http` file (at file beginning) → **Highest priority**
2. **Inline variables** per request (rarely used)

### **Standard Variable Declaration**
```http
@Web_HostAddress = https://localhost:5001
@BearerToken = <YourToken>

# Resource-specific variables (override defaults if needed)
@Email = administrator@localhost
@Password = Administrator1!
@IdSystem = https://example.org/mrn
@IdValue = MRN-00001
```

**Important**: Declare all variables at the **beginning of the file** for transparency and easy updates.

## 📁 **HTTP FILE ORGANIZATION RULES**

### **File Structure**
- **Separate files per resource**: `scripts/http/Auth.http`, `scripts/http/Fhir.http`, etc.
- **Use standard templates**: Copy from `_TEMPLATE.http` or `_FHIR_TEMPLATE.http`
- **Meaningful names**: Use `@name` for each request (e.g., `@name auth_login`)
- **No hard-coded URLs**: Use `{Web_HostAddress}` variable
- **Proper headers**: Follow FHIR standards for healthcare endpoints

### **Naming Convention**
- **Authentication**: `Auth.http`
- **FHIR Resources**: `Fhir.http`
- **Business Logic**: `{Entity}.http` (e.g., `TodoItems.http`)
- **System Operations**: `System.http`

## 🧪 **TESTING EXAMPLES**

### **Standard REST Endpoints**
```http
### @name todo_search
GET {Web_HostAddress}/api/todoitems?page=1&pageSize=20
Authorization: Bearer {{BearerToken}}
Accept: application/json

### @name todo_create
POST {Web_HostAddress}/api/todoitems
Authorization: Bearer {{BearerToken}}
Content-Type: application/json
Accept: application/json

{
  "title": "Test Todo",
  "description": "Test description"
}
```

### **FHIR Endpoints**
```http
### @name patient_search
GET {Web_HostAddress}/fhir/Patient?_count=10
Authorization: Bearer {{BearerToken}}
Accept: application/fhir+json

### @name patient_create
POST {Web_HostAddress}/fhir/Patient
Authorization: Bearer {{BearerToken}}
Content-Type: application/fhir+json
Accept: application/fhir+json
Prefer: return=representation

{
  "resourceType": "Patient",
  "identifier": [
    {
      "system": "https://example.org/mrn",
      "value": "MRN-00001"
    }
  ]
}
```

**Note**: For requests with **body content**, always include `Content-Type` header. For updates, include `If-Match` header with version information.

## 🚦 **TESTING WORKFLOW (Patient Example)**

### **1. Smoke Test**
```http
GET {Web_HostAddress}/fhir/Patient
```
**Expected**: `200 OK` with valid FHIR Bundle or resource list

### **2. Complete Flow**
1. **Search**: `patient_search` → Validate Bundle response
2. **Create**: `patient_create` → Check `201 Created` + `Location` header
3. **Read**: `patient_read` → Verify resource content
4. **Update**: `patient_update` → Check version increment + ETag handling
5. **Delete**: `patient_delete` → Verify removal (if applicable)

### **3. Validation Points**
- **Location header** on creation
- **meta.versionId** increment after updates
- **resourceType** consistency
- **FHIR compliance** validation

## 🧰 **EXECUTION METHODS**

### **VS Code REST Client (Recommended)**
1. Open `.http` file
2. Click on request `@name` to execute
3. View response in separate panel
4. Use "Send Request" button for individual requests

### **curl (Quick Testing)**
```bash
# Basic test
curl -k https://localhost:5001/fhir/Patient

# With authentication
curl -k -H "Authorization: Bearer <token>" https://localhost:5001/fhir/Patient
```

## 🧪 **RESULT REPORTING TEMPLATE**

### **Test Results Summary**
| Step | Name | Status | Resource Type | ID | Version ID | Notes |
|------|------|--------|---------------|----|------------|-------|
| 1 | patient_search | 200 | Bundle | — | — | Valid FHIR response |
| 2 | patient_create | 201 | Patient | 123 | 1 | Location header present |
| 3 | patient_read | 200 | Patient | 123 | 1 | Content verified |
| 4 | patient_update | 200 | Patient | 123 | 2 | Version incremented |
| 5 | patient_delete | 204 | — | — | — | Resource removed |

### **Status Codes**
- **200**: Success with content
- **201**: Created successfully
- **204**: Success without content
- **400**: Bad request
- **401**: Unauthorized
- **404**: Not found
- **409**: Conflict
- **422**: Validation error

## 🔧 **TROUBLESHOOTING GUIDE**

### **Common Issues & Solutions**

#### **Server/Port Issues**
```bash
# Check if port is in use
netstat -an | findstr :5001

# Restart API
dotnet run --project src/Web --launch-profile FHIRAI.Web
```

#### **SSL Certificate Issues**
```bash
# Trust development certificate
dotnet dev-certs https --trust

# Or disable SSL verification in REST Client (dev only)
```

#### **Authentication Issues**
- Verify `@BearerToken` variable is set correctly
- Check token expiration
- Ensure proper authorization headers

#### **Variable Issues**
- Verify variables declared at file beginning
- Check for typos in variable names
- Ensure proper syntax: `{VariableName}`

## 🔐 **SECURITY CONSIDERATIONS**

### **Development Environment**
- **Temporary tokens** allowed in `.http` files for local testing
- **Never commit** production tokens or secrets
- **Use environment variables** for sensitive data in production

### **Production Guidelines**
- **Secret management** for production deployments
- **Token rotation** policies
- **Access control** validation

## 🔄 **ERROR RESOLUTION & FEEDBACK LOOP**

### **When Endpoint Tests Fail**

#### **Step 0: Review Related Rules (MANDATORY)**
Before making changes, **read the relevant rules**:
- **Domain/EF & FHIR mapping**: See `HEALTHCARE_DATA_PATTERN_REFERENCE.md`
- **AutoMapper & exception mapping**: See `CODE_PATTERNS.md`
- **API pattern rules**: See `API_PATTERN_RULE.md`
- **Current endpoint testing rules**: This document

#### **Step 1: Issue Diagnosis**
1. **Identify failure point**: Request name, status code, error details
2. **Categorize error type**:
   - **Mapping/Serialization**: DTO ↔ Domain ↔ FHIR conversion
   - **Schema/Entity/EF**: Missing fields, cardinality issues, data type mismatches
   - **Headers/Protocol**: Accept/Content-Type/ETag/Prefer header issues
   - **Test data**: Missing variables, invalid test data
3. **Collect evidence**: Request/response logs, error messages

#### **Step 2: Change Scope Definition**
- **AutoMapper mapping**: Fix in Profile classes following exception mapping rules
- **Domain/EF schema**: Update according to domain entity rules, create new migrations
- **Headers/endpoints**: Modify `.http` files or controllers while maintaining compliance

#### **Step 3: Specification Note Creation**
- **Document FHIR element path** (R4/R5), cardinality, data types, bindings
- **Explain change rationale** and compatibility strategy
- **Note migration impact** and data transition steps

#### **Step 4: Controlled Implementation**
- **Minimal code changes** required
- **Follow "no inference" principle**: Only add fields present in R4 or valid Extensions
- **Update unit tests** (round-trip, null vs empty semantics)

#### **Step 5: Re-run Endpoint Tests**
- **Repeat testing workflow** for affected resources
- **Document results**: Request names, status codes, resource types, versions, notes

#### **Step 6: Completion & PR Checklist**
- **Update specification notes** in PR with rule references
- **Include endpoint test logs** after successful validation
- **Complete Definition of Done** checklist

### **Bug Fix Checklist (Required Before Merge)**
- [ ] **Reviewed rules**: `HEALTHCARE_DATA_PATTERN_REFERENCE.md`, `CODE_PATTERNS.md`, `API_PATTERN_RULE.md`, `ENDPOINT_CHECKS.md`
- [ ] **Created specification notes** with FHIR R4/R5 details
- [ ] **No inference violations**: Changes align with R4/valid Extensions
- [ ] **Unit tests pass**: `AssertConfigurationIsValid()` validation successful
- [ ] **Endpoint tests pass**: Search/Create/Read/Update/Delete operations successful
- [ ] **Results documented**: Test logs and documentation included with PR

## ✅ **DEFINITION OF DONE (ENDPOINT TESTING)**

### **Success Criteria**
- **Automated tests pass**: All FHIR resources (11+ tests) must PASS
- **Complete test coverage**: Search, Create, Read, Update, Delete, History operations
- **Valid responses**: Proper status codes, FHIR-compliant response bodies
- **Version management**: Proper ETag handling and version increments
- **Mapping validation**: Round-trip DTO ↔ FHIR conversion working correctly

### **Documentation Requirements**
- **Test execution logs** for all operations
- **Specification notes** for any exception mappings
- **Template usage** for new `.http` files
- **Integration with workflow completion reporting**

### **Quality Standards**
- **FHIR compliance** maintained for healthcare endpoints
- **Performance validation** for response times
- **Error handling** properly implemented
- **Security validation** for authentication and authorization

---

**🎯 REMEMBER**: 
1. **Always start with rule review** before making changes
2. **Follow the testing workflow** consistently
3. **Document all test results** and any fixes applied
4. **Integrate with workflow completion reporting** for comprehensive audit trails
5. **Use standard templates** for new endpoint testing files

