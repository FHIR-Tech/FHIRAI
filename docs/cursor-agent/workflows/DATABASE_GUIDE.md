# FHIRAI - Database Guide

## 🗄️ Database Design Principles

### Normalization
FHIRAI tuân thủ 3NF (Third Normal Form) để đảm bảo data integrity:

```sql
-- ✅ Normalized design with snake_case naming
CREATE TABLE "todo_lists" (
    "id" SERIAL PRIMARY KEY,
    "title" VARCHAR(200) NOT NULL,
    "colour_code" VARCHAR(7) NOT NULL,
    "created" TIMESTAMP NOT NULL,
    "created_by" VARCHAR(100)
);

CREATE TABLE "todo_items" (
    "id" SERIAL PRIMARY KEY,
    "list_id" INTEGER NOT NULL,
    "title" VARCHAR(200) NOT NULL,
    "done" BOOLEAN NOT NULL DEFAULT FALSE,
    "priority" INTEGER NOT NULL DEFAULT 1,
    "note" VARCHAR(1000),
    "created" TIMESTAMP NOT NULL,
    "created_by" VARCHAR(100),
    "last_modified" TIMESTAMP,
    "last_modified_by" VARCHAR(100),
    FOREIGN KEY ("list_id") REFERENCES "todo_lists"("id") ON DELETE CASCADE
);
```

### Indexing Strategy
```sql
-- Primary indexes (automatic)
CREATE INDEX "ix_todo_items_list_id" ON "todo_items"("list_id");

-- Composite indexes for common queries
CREATE INDEX "ix_todo_items_list_id_done" ON "todo_items"("list_id", "done");
CREATE INDEX "ix_todo_items_list_id_priority" ON "todo_items"("list_id", "priority");

-- Full-text search indexes
CREATE INDEX "ix_todo_items_title_search" ON "todo_items" USING GIN (to_tsvector('english', "title"));
```

### Relationship Design
```sql
-- One-to-Many: TodoList -> TodoItems
ALTER TABLE "todo_items" 
ADD CONSTRAINT "fk_todo_items_todo_lists_list_id" 
FOREIGN KEY ("list_id") REFERENCES "todo_lists"("id") ON DELETE CASCADE;

-- Many-to-Many: Users -> Roles (if needed)
CREATE TABLE "user_roles" (
    "user_id" VARCHAR(450) NOT NULL,
    "role_id" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("user_id", "role_id"),
    FOREIGN KEY ("user_id") REFERENCES "users"("id") ON DELETE CASCADE,
    FOREIGN KEY ("role_id") REFERENCES "roles"("id") ON DELETE CASCADE
);
```

### Data Types
```sql
-- PostgreSQL-specific data types
CREATE TABLE "example_table" (
    "id" SERIAL PRIMARY KEY,                    -- Auto-incrementing integer
    "title" VARCHAR(200) NOT NULL,              -- Variable-length string with limit
    "description" TEXT,                         -- Unlimited text
    "price" DECIMAL(10,2),                     -- Precise decimal
    "created_date" TIMESTAMP NOT NULL,          -- Date and time
    "is_active" BOOLEAN NOT NULL DEFAULT TRUE,  -- Boolean
    "tags" TEXT[],                             -- Array of text
    "metadata" JSONB,                          -- JSON data
    "binary_data" BYTEA                        -- Binary data
);
```

## 🔧 Entity Framework Configuration

### Entity Configuration Pattern
```csharp
public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Table configuration
        builder.ToTable("todo_items");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties with explicit column mapping
        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(1000);

        builder.Property(x => x.Done)
            .HasColumnName("done")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasDefaultValue(PriorityLevel.Medium);

        builder.Property(x => x.ListId)
            .HasColumnName("list_id")
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.List)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ListId);
        builder.HasIndex(x => new { x.ListId, x.Done });
        builder.HasIndex(x => new { x.ListId, x.Priority });

        // Audit properties with explicit column mapping
        builder.Property(x => x.Created)
            .HasColumnName("created")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(x => x.LastModified)
            .HasColumnName("last_modified");

        builder.Property(x => x.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100);
    }
}
```

### Value Object Configuration
```csharp
public class ColourConfiguration : IEntityTypeConfiguration<Colour>
{
    public void Configure(EntityTypeBuilder<Colour> builder)
    {
        builder.ToTable("colours");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(7)
            .IsRequired();

        // Seed data
        builder.HasData(
            Colour.White,
            Colour.Red,
            Colour.Orange,
            Colour.Yellow,
            Colour.Green,
            Colour.Blue,
            Colour.Purple,
            Colour.Grey
        );
    }
}
```

### Complex Type Configuration
```csharp
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.OwnsOne(x => x.Location, location =>
        {
            location.Property(l => l.Street)
                .HasColumnName("street")
                .HasMaxLength(200);
            location.Property(l => l.City)
                .HasColumnName("city")
                .HasMaxLength(100);
            location.Property(l => l.State)
                .HasColumnName("state")
                .HasMaxLength(50);
            location.Property(l => l.ZipCode)
                .HasColumnName("zip_code")
                .HasMaxLength(10);
            location.Property(l => l.Country)
                .HasColumnName("country")
                .HasMaxLength(100);
        });
    }
}
```

### DbContext Configuration
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure global query filters
        modelBuilder.Entity<TodoItem>()
            .HasQueryFilter(x => !x.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Enable sensitive data logging in development
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Apply audit information before saving
        ApplyAuditInformation();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = DateTime.UtcNow;
                entry.Entity.CreatedBy = GetCurrentUserId();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModified = DateTime.UtcNow;
                entry.Entity.LastModifiedBy = GetCurrentUserId();
            }
        }
    }

    private string? GetCurrentUserId()
    {
        // Implementation to get current user ID
        return "system"; // Placeholder
    }
}
```

## 📊 Migration Management

### Migration Naming Convention
```bash
# Format: {Description}_{Date}
dotnet ef migrations add CreateTodoItems_20241201
dotnet ef migrations add AddPriorityToTodoItems_20241202
dotnet ef migrations add AddAuditFields_20241203
```

### Migration Strategy
```csharp
// Example migration
public partial class CreateTodoItems_20241201 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "todo_lists",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                colour_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_todo_lists", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "todo_items",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                list_id = table.Column<int>(type: "integer", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                done = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_todo_items", x => x.id);
                table.ForeignKey(
                    name: "fk_todo_items_todo_lists_list_id",
                    column: x => x.list_id,
                    principalTable: "todo_lists",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_todo_items_list_id",
            table: "todo_items",
            column: "list_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "todo_items");
        migrationBuilder.DropTable(name: "todo_lists");
    }
}
```

### Data Seeding
```csharp
public partial class SeedInitialData_20241201 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed todo_lists
        migrationBuilder.InsertData(
            table: "todo_lists",
            columns: new[] { "title", "colour_code", "created", "created_by" },
            values: new object[,]
            {
                { "Shopping List", "#FF0000", DateTime.UtcNow, "system" },
                { "Work Tasks", "#0000FF", DateTime.UtcNow, "system" },
                { "Personal Goals", "#008000", DateTime.UtcNow, "system" }
            });

        // Seed todo_items
        migrationBuilder.InsertData(
            table: "todo_items",
            columns: new[] { "list_id", "title", "done", "priority", "created", "created_by" },
            values: new object[,]
            {
                { 1, "Buy groceries", false, 1, DateTime.UtcNow, "system" },
                { 1, "Get gas", false, 2, DateTime.UtcNow, "system" },
                { 2, "Review code", false, 1, DateTime.UtcNow, "system" },
                { 2, "Write documentation", false, 2, DateTime.UtcNow, "system" },
                { 3, "Exercise", false, 1, DateTime.UtcNow, "system" },
                { 3, "Read book", false, 2, DateTime.UtcNow, "system" }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(table: "todo_items", keyColumn: "id", keyValues: new object[] { 1, 2, 3, 4, 5, 6 });
        migrationBuilder.DeleteData(table: "todo_lists", keyColumn: "id", keyValues: new object[] { 1, 2, 3 });
    }
}
```

### Rollback Strategy
```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration
dotnet ef migrations remove

# Generate SQL script for review
dotnet ef migrations script --from PreviousMigrationName --to CurrentMigrationName
```

## 📊 Performance Optimization

### Query Optimization
```csharp
// ✅ Efficient query with proper includes
public async Task<TodoItem?> GetTodoItemWithListAsync(int id, CancellationToken cancellationToken = default)
{
    return await _context.TodoItems
        .Include(x => x.List)
        .AsNoTracking() // For read-only queries
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}

// ✅ Efficient pagination
public async Task<PaginatedList<TodoItemBriefDto>> GetTodoItemsWithPaginationAsync(
    int listId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
{
    var query = _context.TodoItems
        .Where(x => x.ListId == listId)
        .OrderBy(x => x.Title);

    var totalCount = await query.CountAsync(cancellationToken);
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ProjectTo<TodoItemBriefDto>(_mapper.ConfigurationProvider)
        .ToListAsync(cancellationToken);

    return new PaginatedList<TodoItemBriefDto>(items, totalCount, pageNumber, pageSize);
}

// ❌ Avoid N+1 queries
public async Task<List<TodoItemDto>> GetTodoItemsWithListInfoAsync(CancellationToken cancellationToken = default)
{
    // ❌ Bad: N+1 queries
    var items = await _context.TodoItems.ToListAsync(cancellationToken);
    foreach (var item in items)
    {
        await _context.Entry(item).Reference(x => x.List).LoadAsync(cancellationToken);
    }

    // ✅ Good: Single query with include
    return await _context.TodoItems
        .Include(x => x.List)
        .ProjectTo<TodoItemDto>(_mapper.ConfigurationProvider)
        .ToListAsync(cancellationToken);
}
```

### Indexing Strategy
```sql
-- Composite indexes for common query patterns
CREATE INDEX CONCURRENTLY "ix_todo_items_list_id_done_priority" 
ON "todo_items"("list_id", "done", "priority");

-- Partial indexes for filtered queries
CREATE INDEX CONCURRENTLY "ix_todo_items_active_items" 
ON "todo_items"("list_id", "priority") 
WHERE "done" = FALSE;

-- Full-text search indexes
CREATE INDEX CONCURRENTLY "ix_todo_items_title_search" 
ON "todo_items" USING GIN (to_tsvector('english', "title"));

-- Covering indexes for frequently accessed data
CREATE INDEX CONCURRENTLY "ix_todo_items_covering" 
ON "todo_items"("list_id") 
INCLUDE ("title", "done", "priority");
```

### Connection Pooling
```csharp
// Connection string with pooling configuration
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=fhirai;Username=postgres;Password=password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Lifetime=300;"
}
```

### Caching Strategy
```csharp
// Memory cache for frequently accessed data
public class CachedTodoItemRepository : ITodoItemRepository
{
    private readonly ITodoItemRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"TodoItem_{id}";
        
        if (_cache.TryGetValue(cacheKey, out TodoItem? cachedItem))
        {
            return cachedItem;
        }

        var item = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (item != null)
        {
            _cache.Set(cacheKey, item, _cacheExpiration);
        }

        return item;
    }
}
```

## 🔒 Database Security

### Row Level Security (RLS)
```sql
-- Enable RLS on tables
ALTER TABLE "todo_items" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "todo_lists" ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "users_can_view_own_todo_items" ON "todo_items"
    FOR SELECT USING (
        "created_by" = current_user OR 
        EXISTS (
            SELECT 1 FROM "todo_lists" 
            WHERE "id" = "todo_items"."list_id" 
            AND "created_by" = current_user
        )
    );

CREATE POLICY "users_can_modify_own_todo_items" ON "todo_items"
    FOR ALL USING ("created_by" = current_user);
```

### Data Encryption
```sql
-- Column-level encryption (if needed)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Encrypt sensitive data
UPDATE "users" 
SET "ssn" = pgp_sym_encrypt("ssn", 'encryption_key_here')
WHERE "ssn" IS NOT NULL;
```

### Audit Logging
```sql
-- Create audit table
CREATE TABLE "audit_logs" (
    "id" SERIAL PRIMARY KEY,
    "table_name" VARCHAR(100) NOT NULL,
    "action" VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    "record_id" INTEGER,
    "old_values" JSONB,
    "new_values" JSONB,
    "changed_by" VARCHAR(100),
    "changed_at" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
    BEGIN
        IF TG_OP = 'INSERT' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'INSERT', NEW."id", to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'UPDATE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'UPDATE', NEW."id", to_jsonb(OLD), to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'DELETE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'DELETE', OLD."id", to_jsonb(OLD), current_user);
            RETURN OLD;
        END IF;
        RETURN NULL;
    END;
$$ LANGUAGE plpgsql;

-- Create audit trigger
CREATE TRIGGER audit_todoitems_trigger
    AFTER INSERT OR UPDATE OR DELETE ON "todo_items"
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();
```

### Backup Strategy
```bash
# Automated backup script
#!/bin/bash
BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="fhirai"

# Create backup
pg_dump -h localhost -U postgres -d $DB_NAME > $BACKUP_DIR/fhirai_$DATE.sql

# Compress backup
gzip $BACKUP_DIR/fhirai_$DATE.sql

# Keep only last 7 days of backups
find $BACKUP_DIR -name "fhirai_*.sql.gz" -mtime +7 -delete
```

## 📋 **POSTGRESQL TABLE NAMING CONVENTIONS (CRITICAL FOR CURSOR AI)**

**CRITICAL**: Cursor AI MUST follow these PostgreSQL table naming conventions when creating new entities.

### **Table Naming Rules**

#### **1. Entity Names → Table Names**
```
Domain Entity:     TodoItem
Table Name:        "todo_items"        (snake_case, plural, quoted)

Domain Entity:     FhirResource  
Table Name:        "fhir_resources"    (snake_case, plural, quoted)

Domain Entity:     PatientAccess
Table Name:        "patient_accesses"  (snake_case, plural, quoted)
```

#### **2. Naming Pattern**
```csharp
// ✅ CORRECT: Use snake_case, Plural, Quoted
builder.ToTable("todo_items");
builder.ToTable("fhir_resources");
builder.ToTable("patient_accesses");

// ❌ WRONG: Don't use these patterns
builder.ToTable("TodoItems");        // PascalCase
builder.ToTable("todoitems");        // No underscores
builder.ToTable("todo_item");        // Singular
builder.ToTable("Todo_Items");       // Mixed case
```

#### **3. Entity Configuration Template**
```csharp
public class [EntityName]Configuration : IEntityTypeConfiguration<[EntityName]>
{
    public void Configure(EntityTypeBuilder<[EntityName]> builder)
    {
        // Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("[entity_name]s");  // or appropriate plural form
        
        // Primary key
        builder.HasKey(x => x.Id);
        
        // Properties configuration
        builder.Property(x => x.PropertyName)
            .HasMaxLength(200)
            .IsRequired();
            
        // Relationships
        builder.HasOne(x => x.RelatedEntity)
            .WithMany(x => x.Collection)
            .HasForeignKey(x => x.ForeignKeyId);
            
        // Indexes
        builder.HasIndex(x => x.PropertyName);
    }
}
```

#### **4. Common Pluralization Rules**
```
Entity → Table
TodoItem → "todo_items"
TodoList → "todo_lists"
FhirResource → "fhir_resources"
PatientAccess → "patient_accesses"
UserRole → "user_roles"
UserClaim → "user_claims"
UserLogin → "user_logins"
UserToken → "user_tokens"
RoleClaim → "role_claims"
```

#### **5. Identity Tables (ASP.NET Core Identity)**
```csharp
// These are standard ASP.NET Core Identity tables
builder.ToTable("users");      // Users
builder.ToTable("roles");      // Roles  
builder.ToTable("user_roles"); // UserRoles
builder.ToTable("user_claims"); // UserClaims
builder.ToTable("user_logins"); // UserLogins
builder.ToTable("user_tokens"); // UserTokens
builder.ToTable("role_claims"); // RoleClaims
```

### **Column Naming Rules**

#### **1. Column Names**
```csharp
// ✅ CORRECT: Use snake_case, Quoted
builder.Property(x => x.Title).HasColumnName("title");
builder.Property(x => x.CreatedAt).HasColumnName("created_at");
builder.Property(x => x.LastModifiedAt).HasColumnName("last_modified_at");

// ❌ WRONG: Don't use PascalCase
builder.Property(x => x.Title).HasColumnName("Title");
builder.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
```

#### **2. Foreign Key Naming**
```csharp
// ✅ CORRECT: Use snake_case
builder.Property(x => x.ListId).HasColumnName("list_id");
builder.Property(x => x.PatientReference).HasColumnName("patient_reference");

// ❌ WRONG: Don't use PascalCase
builder.Property(x => x.ListId).HasColumnName("ListId");
builder.Property(x => x.PatientReference).HasColumnName("PatientReference");
```

### **Index Naming Rules**

#### **1. Index Names**
```csharp
// ✅ CORRECT: Use snake_case, Descriptive
builder.HasIndex(x => x.ListId).HasDatabaseName("ix_todo_items_list_id");
builder.HasIndex(x => new { x.ListId, x.Done }).HasDatabaseName("ix_todo_items_list_id_done");

// ❌ WRONG: Don't use PascalCase
builder.HasIndex(x => x.ListId).HasDatabaseName("IX_TodoItems_ListId");
```

#### **2. Index Naming Pattern**
```
ix_[table_name]_[column_name]
ix_todo_items_list_id
ix_todo_items_list_id_done
ix_fhir_resources_resource_type
ix_fhir_resources_patient_reference
```

### **Constraint Naming Rules**

#### **1. Foreign Key Constraints**
```csharp
// ✅ CORRECT: Use snake_case, Descriptive
builder.HasOne(x => x.List)
    .WithMany(x => x.Items)
    .HasForeignKey(x => x.ListId)
    .HasConstraintName("fk_todo_items_todo_lists_list_id");

// ❌ WRONG: Don't use PascalCase
.HasConstraintName("FK_TodoItems_TodoLists_ListId");
```

#### **2. Constraint Naming Pattern**
```
fk_[child_table]_[parent_table]_[column_name]
fk_todo_items_todo_lists_list_id
fk_user_roles_users_user_id
fk_user_roles_roles_role_id
```

### **PostgreSQL-Specific Features**

#### **1. JSONB Columns**
```csharp
// ✅ CORRECT: Use jsonb for JSON data
builder.Property(x => x.ResourceJson).HasColumnType("jsonb");
builder.Property(x => x.SearchParameters).HasColumnType("jsonb");
builder.Property(x => x.SecurityLabels).HasColumnType("jsonb");

// GIN indexes for JSONB (PostgreSQL specific)
builder.HasIndex(x => x.SearchParameters).HasMethod("gin");
builder.HasIndex(x => x.SecurityLabels).HasMethod("gin");
```

#### **2. UUID Primary Keys**
```csharp
// ✅ CORRECT: Use PostgreSQL UUID with default
builder.Property(x => x.Id)
    .HasDefaultValueSql("gen_random_uuid()");

// ❌ WRONG: Don't use SQL Server patterns
builder.Property(x => x.Id)
    .HasDefaultValueSql("NEWID()");  // SQL Server specific
```

#### **3. Array Types**
```csharp
// ✅ CORRECT: Use PostgreSQL arrays
builder.Property(x => x.Tags).HasColumnType("text[]");
builder.Property(x => x.Categories).HasColumnType("varchar(50)[]");
```

### **Complete Example for New Entity**

```csharp
public class NewEntityConfiguration : IEntityTypeConfiguration<NewEntity>
{
    public void Configure(EntityTypeBuilder<NewEntity> builder)
    {
        // ✅ Table name: snake_case, Plural, Quoted
        builder.ToTable("new_entities");

        // Primary key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties: snake_case, Quoted
        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(x => x.ParentEntity)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("fk_new_entities_parent_entities_parent_id");

        // Indexes: snake_case naming
        builder.HasIndex(x => x.Name).HasDatabaseName("ix_new_entities_name");
        builder.HasIndex(x => x.ParentId).HasDatabaseName("ix_new_entities_parent_id");
        builder.HasIndex(x => new { x.ParentId, x.Name }).HasDatabaseName("ix_new_entities_parent_id_name");

        // JSONB GIN index
        builder.HasIndex(x => x.Metadata).HasMethod("gin");

        // Audit properties
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.LastModifiedAt);
        builder.Property(x => x.LastModifiedBy).HasMaxLength(100);
    }
}
```

### **Cursor AI Checklist for Table Creation**

#### **Before Creating New Entity**:
- [ ] **Table Name**: Use snake_case, Plural, Quoted (e.g., "new_entities")
- [ ] **Column Names**: Use snake_case, Quoted (e.g., "name", "created_at")
- [ ] **Index Names**: Use snake_case pattern (e.g., "ix_new_entities_name")
- [ ] **Constraint Names**: Use snake_case pattern (e.g., "fk_new_entities_parent_parent_id")
- [ ] **PostgreSQL Features**: Use jsonb, UUID, arrays appropriately
- [ ] **Naming Consistency**: Follow existing patterns in project

#### **Common Mistakes to Avoid**:
- ❌ **PascalCase** for table/column names
- ❌ **Unquoted** table/column names
- ❌ **Singular** table names
- ❌ **SQL Server patterns** (NEWID(), nvarchar)
- ❌ **Inconsistent** naming across entities

---

**🎯 REMEMBER**: **ALWAYS use snake_case, Plural, Quoted names** for PostgreSQL tables in FHIRAI. This is **MANDATORY** for consistency and PostgreSQL compatibility.

## 🚨 Database Checklist

### Design
- [ ] Normalized to 3NF
- [ ] Proper relationships defined
- [ ] Appropriate data types used
- [ ] Indexes created for performance
- [ ] Constraints defined for data integrity

### Configuration
- [ ] Entity Framework configurations created
- [ ] Migrations generated and tested
- [ ] Seed data created for development
- [ ] Connection pooling configured
- [ ] Query optimization implemented

### Security
- [ ] Row Level Security (RLS) enabled
- [ ] Audit logging implemented
- [ ] Sensitive data encrypted
- [ ] Backup strategy in place
- [ ] Access controls configured

### Performance
- [ ] Indexes created for common queries
- [ ] Query optimization implemented
- [ ] Connection pooling configured
- [ ] Caching strategy implemented
- [ ] Performance monitoring in place

---

**🎯 Remember**: Database design is critical for application performance and data integrity. Always follow normalization principles, implement proper indexing, and ensure data security. Regular monitoring and optimization are essential for maintaining good performance.
