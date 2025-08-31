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
    // SOFT DELETE FIELDS
    // ========================================
    // Soft delete tracking fields
    
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
**Examples**: `CreatedAt`, `LastModifiedAt`, `ExpiresAt`, `LastLoginAt`
**Rules**:
- **MANDATORY**: Use `DateTimeOffset` for all timing fields (FHIR compliance)
- Group timing fields logically
- Include timezone considerations
- Use `DateTimeOffset.UtcNow` for default values

### 7. SOFT DELETE FIELDS
**Purpose**: Soft delete tracking and audit trail
**Examples**: `IsDeleted`, `DeletedAt`, `DeletedBy`
**Rules**:
- **MANDATORY**: Include soft delete fields for all entities
- Use `bool IsDeleted` with default `false`
- Use `DateTimeOffset? DeletedAt` for deletion timestamp
- Use `string? DeletedBy` for user who deleted
- Automatically handled by `AuditableEntityInterceptor`

### 8. ADDITIONAL DATA FIELDS
**Purpose**: Supplementary information and metadata
**Examples**: `Tags`, `SearchParameters`, `EventData`
**Rules**:
- Use JSONB for complex data structures
- Include appropriate indexes for performance
- Document complex data structures

### 9. COMPUTED PROPERTIES
**Purpose**: Calculated or derived properties
**Examples**: `DisplayName`, `Age`, `IsActive`
**Rules**:
- Must be marked with `[NotMapped]`
- Should be read-only properties
- Include clear documentation

### 10. NAVIGATION PROPERTIES
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

### BaseEntity Structure

```csharp
public abstract class BaseEntity
{
    // ========================================
    // CORE IDENTITY FIELDS
    // ========================================
    
    /// <summary>
    /// Primary key identifier for the entity
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
}
```

### BaseAuditableEntity Structure

```csharp
public abstract class BaseAuditableEntity : BaseEntity
{
    // ========================================
    // TIMING FIELDS
    // ========================================
    
    /// <summary>
    /// When the entity was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last modified
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>
    /// Who last modified the entity
    /// </summary>
    public string? LastModifiedBy { get; set; }

    // ========================================
    // SOFT DELETE FIELDS
    // ========================================
    
    /// <summary>
    /// Whether the entity is marked as deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// When the entity was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Who deleted the entity
    /// </summary>
    public string? DeletedBy { get; set; }
}
```

### Timezone Handling:
```csharp
// Server time (UTC)
DateTimeOffset utcNow = DateTimeOffset.UtcNow; // 2024-01-15T10:30:00+00:00

// Local time with offset
DateTimeOffset localTime = DateTimeOffset.Now; // 2024-01-15T17:30:00+07:00

// Convert to specific timezone
TimeZoneInfo vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
DateTimeOffset vietnamTime = TimeZoneInfo.ConvertTime(utcNow, vietnamTz);
```

## Soft Delete Implementation

### Automatic Handling:
```csharp
// AuditableEntityInterceptor automatically handles soft delete
if (entry.State == EntityState.Deleted)
{
    entry.State = EntityState.Modified; // Change to modified instead of deleted
    entry.Entity.IsDeleted = true;
    entry.Entity.DeletedAt = _dateTime.GetUtcNow();
    entry.Entity.DeletedBy = _user.Id;
}
```

### Query Filtering:
```csharp
// Always filter out soft-deleted entities
public IQueryable<T> GetActiveEntities<T>() where T : BaseAuditableEntity
{
    return _context.Set<T>().Where(e => !e.IsDeleted);
}
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

### DateTimeOffset Fields
```csharp
[Required]
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

public DateTimeOffset? LastModifiedAt { get; set; }

public DateTimeOffset? DeletedAt { get; set; }
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

## Type Usage Guidelines

### Naming Conventions
- **Properties**: PascalCase
- **Fields**: camelCase (if private fields are used)
- **Constants**: UPPER_CASE
- **Enums**: PascalCase

### Type Usage
- **Primary Keys**: `Guid` with `Guid.NewGuid()` default (planned migration)
- **Foreign Keys**: `Guid` for entity relationships
- **Strings**: Use `string` with appropriate `MaxLength`
- **Dates**: **MANDATORY** Use `DateTimeOffset` for all timing fields
- **Booleans**: Use `bool` with descriptive names
- **Enums**: Use strongly-typed enums

### Default Values
```csharp
// Good
public string Name { get; set; } = string.Empty;
public bool IsActive { get; set; } = true;
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
public bool IsDeleted { get; set; } = false;
public Guid Id { get; set; } = Guid.NewGuid();

// Avoid
public string Name { get; set; } = null!;
public bool IsActive { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // No timezone
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
- [ ] **DateTimeOffset is used for all timing fields**
- [ ] **Soft delete fields are included**
- [ ] **Timezone considerations are addressed**
- [ ] **Guid migration is planned and documented**

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
public class ExampleEntity : BaseAuditableEntity
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
    public DateTimeOffset? ExpiresAt { get; set; }

    // ========================================
    // COMPUTED PROPERTIES
    // ========================================
    
    /// <summary>
    /// Whether entity is currently active
    /// </summary>
    [NotMapped]
    public bool IsActive => IsEnabled && !IsDeleted && (ExpiresAt == null || ExpiresAt > DateTimeOffset.UtcNow);

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
7. **MANDATORY**: Use `DateTimeOffset` for all timing fields
8. **MANDATORY**: Include soft delete fields
9. **PLANNED**: Use `Guid` for primary keys

### Prompt Integration
Include this pattern in prompts when working with domain entities:
