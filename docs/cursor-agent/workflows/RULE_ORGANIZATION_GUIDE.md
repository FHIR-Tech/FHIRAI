# FHIRAI - Rule Organization Guide

## 🎯 **PURPOSE**
This guide explains the organization and responsibility division between different rule files to avoid duplication and ensure clear separation of concerns.

## 📁 **FILE RESPONSIBILITY MATRIX**

### **Primary Files (Single Source of Truth)**

| File | Primary Responsibility | Secondary Responsibility | When to Use |
|------|----------------------|-------------------------|-------------|
| `API_PATTERN_RULE.md` | **API Pattern Decision** | Quick user context reference | **ALWAYS** for API development |
| `CURSOR_AI_RULES.md` | **Core Development Rules** | Workflow guidelines | Development setup and principles |
| `USER_HANDLING_GUIDE.md` | **User Context Patterns** | IUser implementation | User-related implementation |
| `SECURITY_GUIDE.md` | **Authentication & Authorization** | Security compliance | Security implementation |

### **Specialized Reference Files**

| File | Responsibility | When to Reference |
|------|----------------|-------------------|
| `HEALTHCARE_DATA_PATTERN_REFERENCE.md` | FHIR compliance | FHIR resource implementation |
| `CODE_PATTERNS.md` | Implementation patterns | Complex feature development |
| `FIELD_ORGANIZATION_PATTERN.md` | Entity structure | Entity design |
| `ARCHITECTURE_GUIDE.md` | Clean architecture | System design |
| `DATABASE_GUIDE.md` | Database patterns | Data access implementation |
| `CURSOR_AI_QUICK_REFERENCE.md` | Quick commands | Fast development reference |
| `IMPLEMENTATION_CHECKLIST.md` | Step-by-step guide | Complex implementation |
| `DOCUMENTATION_STRUCTURE.md` | AI navigation | Documentation organization |

## 🔄 **CONTENT FLOW & REFERENCES**

### **API Development Flow**
```
1. API_PATTERN_RULE.md (Decision & Quick Reference)
   ↓
2. USER_HANDLING_GUIDE.md (User Context Details)
   ↓
3. SECURITY_GUIDE.md (Authorization Details)
   ↓
4. Specialized Guides (As Needed)
```

### **Reference Chain**
```
API_PATTERN_RULE.md
├── USER_HANDLING_GUIDE.md (User patterns)
├── SECURITY_GUIDE.md (Authorization)
└── Specialized Guides (FHIR, Code, etc.)

CURSOR_AI_RULES.md
├── API_PATTERN_RULE.md (Main rule)
└── Reference Files (Organized by category)
```

## 🚫 **DUPLICATION RULES**

### **What NOT to Duplicate**
1. **User Handling Patterns**: Only in `USER_HANDLING_GUIDE.md`
2. **Authorization Patterns**: Only in `SECURITY_GUIDE.md`
3. **Authentication Flow**: Only in `SECURITY_GUIDE.md`
4. **API Decision Logic**: Only in `API_PATTERN_RULE.md`

### **What CAN be Referenced**
1. **Quick Reference**: Essential patterns in main files
2. **Cross-References**: Links to detailed implementations
3. **Context-Specific**: Patterns relevant to specific domains

## 📋 **CONTENT ORGANIZATION PRINCIPLES**

### **1. Single Source of Truth**
- Each topic has ONE primary file
- Other files reference the primary file
- No duplicate detailed implementations

### **2. Progressive Detail**
- Main files: Quick reference + decision logic
- Reference files: Detailed implementation
- Clear path from quick to detailed

### **3. Context-Specific References**
- Files reference each other based on context
- Clear indication of when to use each file
- Organized by development workflow

### **4. Maintenance Efficiency**
- Changes in one place propagate to references
- Clear ownership of each topic
- Easy to update and maintain

## 🎯 **USAGE GUIDELINES**

### **For Cursor AI**
1. **Start with** `API_PATTERN_RULE.md` for all API development
2. **Reference** `USER_HANDLING_GUIDE.md` for user context details
3. **Reference** `SECURITY_GUIDE.md` for authorization details
4. **Use specialized guides** only when needed

### **For Developers**
1. **Follow the flow** from main to reference files
2. **Check references** before implementing detailed patterns
3. **Update primary files** when making changes
4. **Maintain cross-references** when updating content

## 🔧 **MAINTENANCE CHECKLIST**

### **When Adding New Content**
- [ ] Identify the primary file for the topic
- [ ] Add detailed implementation to primary file
- [ ] Add quick reference to relevant main files
- [ ] Update cross-references in other files
- [ ] Update this organization guide if needed

### **When Updating Content**
- [ ] Update the primary file first
- [ ] Update quick references in main files
- [ ] Verify cross-references are still valid
- [ ] Update this guide if organization changes

---

**🎯 REMEMBER**: This organization ensures efficient maintenance, clear responsibility, and optimal developer experience while avoiding content duplication.
