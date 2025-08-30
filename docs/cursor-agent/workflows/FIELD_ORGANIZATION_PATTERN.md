# FHIRAI - Field Organization Pattern

## Field Organization Pattern

### Mandatory Field Ordering

All domain entities MUST follow this exact field ordering pattern:

```csharp
public class EntityName : BaseEntity
{
    // ========================================
    // FOREIGN KEY FIELDS
    // ========================================
    // Foreign key relationships to other entities
    
    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    // Essential identifying information (names, IDs, etc.)
    
    // ========================================
    // BASIC INFORMATION FIELDS
    // ========================================
    // Fundamental entity data (contact info, addresses, etc.)
    
    // ========================================
    // STATUS & CONFIGURATION FIELDS
    // ========================================
    // State management and configuration data
    
    // ========================================
    // SECURITY & ACCESS FIELDS
    // ========================================
    // Authentication, authorization, and security data
    
    // ========================================
    // TIMING FIELDS
    // ========================================
    // Date and time related data
    
    // ========================================
    // ADDITIONAL DATA FIELDS
    // ========================================
    // Supplementary information and metadata
    
    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    // Calculated or derived properties (marked with [NotMapped])
    
    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    // Entity relationships and collections
}
```

## Section Definitions

### 1. FOREIGN KEY FIELDS
**Purpose**: Primary and foreign key relationships
**Examples**: `UserId`, `PatientId`, `ResourceId`
**Rules**:
- Must be marked with `[Required]` if not nullable
- Use `Guid` type for entity relationships
- Place at the top for immediate visibility

### 2. CORE IDENTITY FIELDS
**Purpose**: Essential identifying information
**Examples**: `Username`, `Email`, `FirstName`, `LastName`, `FhirPatientId`
**Rules**:
- Required fields should be marked with `[Required]`
- Use appropriate `[MaxLength]` constraints
- Include validation attributes (`[EmailAddress]`, etc.)

### 3. BASIC INFORMATION FIELDS
**Purpose**: Fundamental entity data
**Examples**: `Phone`, `Address`, `DateOfBirth`, `Gender`
**Rules**:
- Group related information together
- Use appropriate data types and constraints
- Include validation where necessary

### 4. STATUS & CONFIGURATION FIELDS
**Purpose**: State management and configuration
**Examples**: `Status`, `Role`, `IsActive`, `IsEnabled`
**Rules**:
- Use enums for status fields when possible
- Provide default values where appropriate
- Include check constraints in configurations

### 5. SECURITY & ACCESS FIELDS
**Purpose**: Authentication, authorization, and security
**Examples**: `PasswordHash`, `FailedLoginAttempts`, `LockedUntil`
**Rules**:
- Group security-related fields together
- Use appropriate encryption and hashing
- Include audit trail fields

### 6. TIMING FIELDS
**Purpose**: Date and time related data
**Examples**: `CreatedAt`, `ModifiedAt`, `ExpiresAt`, `LastLoginAt`
**Rules**:
- Use `DateTime` or `DateTime?` types
- Group timing fields logically
- Include timezone considerations

### 7. ADDITIONAL DATA FIELDS
**Purpose**: Supplementary information and metadata
**Examples**: `Tags`, `SearchParameters`, `EventData`
**Rules**:
- Use JSONB for complex data structures
- Include appropriate indexes for performance
- Document complex data structures

### 8. COMPUTED PROPERTIES
**Purpose**: Calculated or derived properties
**Examples**: `DisplayName`, `Age`, `IsActive`
**Rules**:
- Must be marked with `[NotMapped]`
- Should be read-only properties
- Include clear documentation

### 9. NAVIGATION PROPERTIES
**Purpose**: Entity relationships and collections
**Examples**: `User`, `Patient`, `ICollection<UserScope>`
**Rules**:
- Use `virtual` keyword for EF Core lazy loading
- Initialize collections in constructor or property initializer
- Include proper documentation

## Visual Separators

### Mandatory Section Headers
Each section MUST be clearly marked with:

```csharp
// ========================================
// SECTION NAME
// ========================================
```

### Formatting Rules
- Use exactly 39 equals signs (`=`) on each line
- Section name should be in UPPERCASE
- Include one blank line before and after each section
- Use consistent indentation (4 spaces)

## Documentation Standards

### XML Documentation
Every field MUST have XML documentation:

```csharp
/// <summary>
/// Brief description of the field
/// </summary>
/// <remarks>
/// Additional details if needed
/// </remarks>
public string FieldName { get; set; }
```

### Section Comments
Each section should have a brief description:

```csharp
// ========================================
// CORE IDENTITY FIELDS
// ========================================
// Essential identifying information for the entity
```

## Validation Attributes

### Required Fields
```csharp
[Required]
public string RequiredField { get; set; } = string.Empty;
```

### String Length Constraints
```csharp
[MaxLength(255)]
public string LimitedString { get; set; } = string.Empty;
```

### Email Validation
```csharp
[EmailAddress]
[MaxLength(255)]
public string Email { get; set; } = string.Empty;
```

### JSONB Columns
```csharp
[Column(TypeName = "jsonb")]
public string? JsonData { get; set; }
```

### Not Mapped Properties
```csharp
[NotMapped]
public string ComputedProperty => $"{FirstName} {LastName}".Trim();
```

## Entity-Specific Patterns

### User Entity Pattern
```csharp
// CORE IDENTITY FIELDS
// STATUS & CONFIGURATION FIELDS
// SECURITY & AUTHENTICATION FIELDS
// FHIR INTEGRATION FIELDS
// TIMING & TRACKING FIELDS
// COMPUTED PROPERTIES
// NAVIGATION PROPERTIES
```

### Patient Entity Pattern
```csharp
// FHIR INTEGRATION FIELDS
// CORE IDENTITY FIELDS
// CONTACT INFORMATION FIELDS
// STATUS & CONFIGURATION FIELDS
// CONSENT FIELDS
// EMERGENCY CONTACT FIELDS
// COMPUTED PROPERTIES
// NAVIGATION PROPERTIES
```

### FhirResource Entity Pattern
```csharp
// CORE IDENTITY FIELDS
// FHIR DATA FIELDS
// STATUS & TIMING FIELDS
// SEARCH & SECURITY FIELDS
// ADDITIONAL DATA FIELDS
```

### Session/Scope Entity Pattern
```csharp
// FOREIGN KEY FIELDS
// CORE SESSION/SCOPE FIELDS
// TIMING FIELDS
// SECURITY & TRACKING FIELDS
// REVOCATION FIELDS
// COMPUTED PROPERTIES
// NAVIGATION PROPERTIES
```

### Access Control Entity Pattern
```csharp
// FOREIGN KEY FIELDS
// CORE ACCESS FIELDS
// TIMING FIELDS
// AUTHORIZATION FIELDS
// EMERGENCY ACCESS FIELDS
// STATUS FIELDS
// COMPUTED PROPERTIES
// NAVIGATION PROPERTIES
```

### Audit Entity Pattern
```csharp
// CORE EVENT FIELDS
// USER CONTEXT FIELDS
// RESOURCE CONTEXT FIELDS
// ADDITIONAL DATA FIELDS
// TIMING FIELDS
```

## Code Quality Rules

### Naming Conventions
- **Properties**: PascalCase
- **Fields**: camelCase (if private fields are used)
- **Constants**: UPPER_CASE
- **Enums**: PascalCase

### Type Usage
- **Primary Keys**: `Guid` with `uuid_generate_v4()` default
- **Foreign Keys**: `Guid` for entity relationships
- **Strings**: Use `string` with appropriate `MaxLength`
- **Dates**: Use `DateTime` or `DateTime?`
- **Booleans**: Use `bool` with descriptive names
- **Enums**: Use strongly-typed enums

### Default Values
```csharp
// Good
public string Name { get; set; } = string.Empty;
public bool IsActive { get; set; } = true;
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// Avoid
public string Name { get; set; } = null!;
public bool IsActive { get; set; }
```

## Enforcement

### Automatic Validation
- All new entities MUST follow this pattern
- Existing entities MUST be updated to follow this pattern
- Code reviews MUST verify compliance

### Review Checklist
- [ ] Fields are organized according to the pattern
- [ ] Visual separators are properly formatted
- [ ] XML documentation is complete
- [ ] Validation attributes are appropriate
- [ ] Navigation properties are properly configured
- [ ] Computed properties are marked with `[NotMapped]`

## Examples

### Complete Entity Example
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthTech.Domain.Enums;

namespace HealthTech.Domain.Entities;

/// <summary>
/// Example entity demonstrating proper field organization
/// </summary>
public class ExampleEntity : BaseEntity
{
    // ========================================
    // FOREIGN KEY FIELDS
    // ========================================
    
    /// <summary>
    /// Related entity ID
    /// </summary>
    [Required]
    public Guid RelatedEntityId { get; set; }

    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string UniqueIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string DisplayName { get; set; } = string.Empty;

    // ========================================
    // STATUS & CONFIGURATION FIELDS
    // ========================================
    
    /// <summary>
    /// Current status
    /// </summary>
    [Required]
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    /// <summary>
    /// Whether entity is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// When entity expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Whether entity is currently active
    /// </summary>
    [NotMapped]
    public bool IsActive => IsEnabled && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);

    // ========================================
    // NAVIGATION PROPERTIES
    // ========================================
    
    /// <summary>
    /// Related entity
    /// </summary>
    public virtual RelatedEntity RelatedEntity { get; set; } = null!;
}
```

## Integration with Cursor AI

### Automatic Application
When creating or modifying domain entities, Cursor AI should:

1. **Automatically apply** the field organization pattern
2. **Use the correct section headers** with proper formatting
3. **Group related fields** according to the defined categories
4. **Apply validation attributes** based on field type and purpose
5. **Include XML documentation** for all fields
6. **Follow naming conventions** consistently

### Prompt Integration
Include this pattern in prompts when working with domain entities:

```
"Create a new domain entity following the FHIR-AI Backend field organization pattern:
1. Foreign Key Fields
2. Core Identity Fields  
3. Basic Information Fields
4. Status & Configuration Fields
5. Security & Access Fields
6. Timing Fields
7. Additional Data Fields
8. Computed Properties
9. Navigation Properties

Use proper visual separators and XML documentation."
```

## Current FHIRAI Entities Status

### TodoItem Entity
- **Status**: ✅ Refactored to follow pattern
- **File**: `src/Domain/Entities/TodoItem.cs`
- **Improvements Made**: 
  - ✅ Added visual separators with proper formatting
  - ✅ Organized fields according to the pattern
  - ✅ Added XML documentation for all properties
  - ✅ Added validation attributes (`[Required]`, `[MaxLength]`)
  - ✅ Added computed properties (`IsOverdue`, `IsDueSoon`)
  - ✅ Maintained existing business logic (domain events)

### TodoList Entity
- **Status**: ✅ Refactored to follow pattern
- **File**: `src/Domain/Entities/TodoList.cs`
- **Improvements Made**:
  - ✅ Added visual separators with proper formatting
  - ✅ Organized fields according to the pattern
  - ✅ Added XML documentation for all properties
  - ✅ Added validation attributes (`[Required]`, `[MaxLength]`)
  - ✅ Added computed properties for list statistics
  - ✅ Maintained existing collection initialization

### Implementation Complete
- ✅ All existing entities refactored to follow the pattern
- ✅ Proper documentation and validation added
- ✅ Entity configurations should be reviewed for consistency
- ✅ New entities should follow this pattern from creation

### Benefits Achieved
1. **Code Consistency**: All entities follow the same organizational pattern
2. **Improved Readability**: Clear section separation and logical grouping
3. **Enhanced Maintainability**: XML documentation and validation attributes
4. **Better Developer Experience**: Consistent patterns reduce cognitive load
