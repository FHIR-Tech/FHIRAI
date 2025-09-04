# HTTP Testing Scripts

This directory contains HTTP testing files for validating API endpoints in the FHIRAI project.

## 📁 **File Structure**

### **Templates**
- **`_TEMPLATE.http`** - Standard REST API testing template
- **`_FHIR_TEMPLATE.http`** - FHIR-compliant endpoint testing template

### **Example Files**
- **`Patient.http`** - FHIR Patient resource testing example
- **`TodoItems.http`** - Standard REST API testing example

## 🚀 **Quick Start**

### **1. Choose Your Template**
- **For FHIR endpoints** (Patient, Observation, etc.): Use `_FHIR_TEMPLATE.http`
- **For business logic** (TodoItems, Users, etc.): Use `_TEMPLATE.http`

### **2. Copy and Customize**
```bash
# For FHIR resources
cp _FHIR_TEMPLATE.http Patient.http
# Edit Patient.http with your specific resource details

# For business resources
cp _TEMPLATE.http TodoItems.http
# Edit TodoItems.http with your specific endpoint details
```

### **3. Set Variables**
Update the variables at the top of your file:
```http
@Web_HostAddress = https://localhost:5001
@BearerToken = <YourToken>
@ResourceId = <your-resource-id>
```

### **4. Test Your Endpoints**
- Open the `.http` file in VS Code
- Click on any `@name` to execute that request
- View responses in the REST Client panel

## 🧪 **Testing Workflow**

### **Standard REST API Flow**
1. **Health Check** - Verify API is running
2. **Authentication** - Login to get token
3. **Search** - List resources
4. **Create** - Add new resource
5. **Read** - Get specific resource
6. **Update** - Modify existing resource
7. **Delete** - Remove resource

### **FHIR API Flow**
1. **Capabilities** - Check FHIR metadata
2. **Search** - Query resources with FHIR parameters
3. **Create** - POST new FHIR resource
4. **Read** - GET specific FHIR resource
5. **Update** - PUT with version handling
6. **Delete** - Remove FHIR resource
7. **History** - View resource version history

## 🔧 **Configuration**

### **Port Settings**
- **HTTPS**: Port 5001 (Primary)
- **HTTP**: Port 5000 (Fallback)
- **Profile**: FHIRAI.Web

### **Headers**
- **Standard API**: `Accept: application/json`
- **FHIR API**: `Accept: application/fhir+json`
- **Authentication**: `Authorization: Bearer <token>`

## 📋 **Best Practices**

1. **Use meaningful names** for each request (`@name`)
2. **Declare variables** at the top of the file
3. **Test complete workflows** (CRUD operations)
4. **Validate responses** for proper status codes
5. **Check FHIR compliance** for healthcare endpoints
6. **Document any failures** and resolutions

## 🚨 **Troubleshooting**

### **Common Issues**
- **Port conflicts**: Check if 5001 is available
- **SSL certificates**: Trust dev certificate with `dotnet dev-certs https --trust`
- **Authentication**: Verify token is valid and not expired
- **Variables**: Ensure all variables are properly declared

### **Getting Help**
- **Endpoint Testing Guide**: See `docs/cursor-agent/workflows/ENDPOINT_CHECKS.md`
- **API Pattern Rules**: See `docs/cursor-agent/workflows/API_PATTERN_RULE.md`
- **FHIR Compliance**: See `docs/cursor-agent/workflows/HEALTHCARE_DATA_PATTERN_REFERENCE.md`

## 🔄 **Integration**

This testing framework integrates with:
- **VS Code REST Client** for easy execution
- **Cursor AI workflows** for automated testing
- **FHIR compliance validation** for healthcare endpoints
- **Workflow completion reporting** for audit trails

---

**🎯 Remember**: Always test your endpoints after implementing new features or making changes. Use the appropriate template and follow the testing workflow to ensure quality and compliance.
