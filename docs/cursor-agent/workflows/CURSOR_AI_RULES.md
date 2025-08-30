# FHIRAI - Cursor AI Rules

## 🎯 **OPTIMIZED API PATTERN RULE**

**CRITICAL**: For all API development tasks, Cursor AI MUST use the optimized rule:

### **Main Rule File**: `docs/cursor-agent/workflows/API_PATTERN_RULE.md`

This single rule contains:
- ✅ **Auto-discovery algorithm** for pattern decision
- ✅ **Implementation templates** for all patterns
- ✅ **Validation checklists** for each pattern
- ✅ **Quick commands** for development
- ✅ **File locations** and naming patterns
- ✅ **Conflict resolution** rules

### **Decision Matrix (Quick Reference)**:
```
IF "patient" OR "fhir" OR "healthcare" OR "medical" OR "clinical"
    → USE Minimal API Endpoints (FHIR compliance - MANDATORY)
ELSE IF "user" OR "todo" OR "task" OR "admin" OR "system" OR "analytics"
    → USE Controllers (Business logic - RECOMMENDED)
ELSE IF "health" OR "webhook" OR "integration" OR "export" OR "import"
    → USE Minimal API Endpoints (Special operations - FLEXIBLE)
ELSE
    → USE Controllers (Default)
```

## 🚀 **WORKFLOW FOR API DEVELOPMENT**

### **Step 1: Pattern Decision**
1. Extract keywords from user request
2. Apply decision matrix from optimized rule
3. Determine pattern (Endpoints/Controllers)

### **Step 2: Implementation**
1. Use appropriate template from optimized rule
2. Follow validation checklist for chosen pattern
3. Implement with proper file structure

### **Step 3: Response Format**
Always respond with the standardized format from optimized rule:
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
- [ ] [Specific validation items]

🔧 **Next Steps**:
- [ ] Create file structure
- [ ] Implement business logic
- [ ] Add tests
- [ ] Update documentation
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

## 🎯 **KEY PRINCIPLES**

1. **Single Source of Truth**: Use `API_PATTERN_RULE.md` for all API decisions
2. **Auto-Discovery**: Automatically detect patterns from user keywords
3. **Standardized Response**: Always use the response format from optimized rule
4. **Validation First**: Follow validation checklists for each pattern
5. **Reference When Needed**: Only consult other files for specific requirements

## 🚨 **IMPORTANT RULES**

2. **ALWAYS** start with the optimized rule for API development
3. **ALWAYS** follow the standardized response format
4. **ALWAYS** include validation checklists in responses
5. **ONLY** reference other files when specific details are needed

---

**🎯 REMEMBER**: The optimized rule contains everything needed for quick decision-making and implementation. Use it as the single source of truth for all API development tasks!
