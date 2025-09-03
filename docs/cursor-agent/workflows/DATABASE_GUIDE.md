# FHIRAI - Database Guide for .NET 9 + PostgreSQL

> **Mục tiêu**: Hướng dẫn thiết kế CSDL, cấu hình Entity Framework Core, quy trình migration, cache, audit logging, bảo mật và tối ưu hiệu năng cho dự án FHIRAI sử dụng .NET 9 + PostgreSQL.  
> **Phong cách**: Rõ ràng, có ví dụ thực chiến, tuân thủ quy ước **snake_case, plural, quoted** cho tên bảng/cột.

---

## 🗄️ Database Design Principles

### Normalization
FHIRAI tuân thủ 3NF (Third Normal Form) để đảm bảo data integrity:

```sql
-- ✅ Normalized design with snake_case naming
CREATE TABLE "todo_lists" (
    "id" SERIAL PRIMARY KEY,
    "title" VARCHAR(200) NOT NULL,
    "colour_code" VARCHAR(7) NOT NULL,
    "created" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "created_by" VARCHAR(100)
);

CREATE TABLE "todo_items" (
    "id" SERIAL PRIMARY KEY,
    "list_id" INTEGER NOT NULL,
    "title" VARCHAR(200) NOT NULL,
    "done" BOOLEAN NOT NULL DEFAULT FALSE,
    "priority" INTEGER NOT NULL DEFAULT 1,
    "note" VARCHAR(1000),
    "created" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "created_by" VARCHAR(100),
    "last_modified" TIMESTAMPTZ,
    "last_modified_by" VARCHAR(100),
    FOREIGN KEY ("list_id") REFERENCES "todo_lists"("id") ON DELETE CASCADE
);
```

### JSONB Usage Guidelines
- **Khi nào dùng**: Dữ liệu **mở rộng**, **metadata**, **payload FHIR**, **thuộc tính tuỳ biến**
- **Không dùng**: Dữ liệu truy vấn thường xuyên theo điều kiện lọc (nếu có thể chuẩn hoá)
- **Index**: GIN cho tìm kiếm chứa/đường dẫn

```sql
-- JSONB & chỉ mục
ALTER TABLE "fhir_resources" ADD COLUMN "resource_json" JSONB NOT NULL;
CREATE INDEX CONCURRENTLY "ix_fhir_resources_resource_json_gin"
  ON "fhir_resources" USING GIN ("resource_json");

-- Ví dụ truy vấn theo path
-- Tìm tài nguyên có resource.type = 'Patient'
SELECT id FROM "fhir_resources"
WHERE resource_json->>'resourceType' = 'Patient';
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
    "created_date" TIMESTAMPTZ NOT NULL,        -- Date and time with timezone (UTC)
    "is_active" BOOLEAN NOT NULL DEFAULT TRUE,  -- Boolean
    "tags" TEXT[],                             -- Array of text
    "metadata" JSONB,                          -- JSON data with GIN index
    "binary_data" BYTEA                        -- Binary data
);
```

---

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

### FHIR Resource Configuration Example
```csharp
public class FhirResourceConfiguration : IEntityTypeConfiguration<FhirResource>
{
    public void Configure(EntityTypeBuilder<FhirResource> builder)
    {
        // Table configuration
        builder.ToTable("fhir_resources");

        // Primary key (UUID on PostgreSQL)
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasColumnName("id")
               .HasDefaultValueSql("gen_random_uuid()");

        // Properties
        builder.Property(x => x.ResourceType)
               .HasColumnName("resource_type")
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.PatientReference)
               .HasColumnName("patient_reference")
               .HasMaxLength(100);

        builder.Property(x => x.ResourceJson)
               .HasColumnName("resource_json")
               .HasColumnType("jsonb")
               .IsRequired();

        // Audit properties
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

        // Indexes
        builder.HasIndex(x => x.ResourceType)
               .HasDatabaseName("ix_fhir_resources_resource_type");
        builder.HasIndex(x => x.PatientReference)
               .HasDatabaseName("ix_fhir_resources_patient_reference");
        builder.HasIndex(x => x.ResourceJson).HasMethod("gin")
               .HasDatabaseName("ix_fhir_resources_resource_json_gin");
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
        builder.ToTable("addresses");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Location, owned =>
        {
            owned.Property(p => p.Street).HasColumnName("street").HasMaxLength(200);
            owned.Property(p => p.City).HasColumnName("city").HasMaxLength(100);
            owned.Property(p => p.State).HasColumnName("state").HasMaxLength(50);
            owned.Property(p => p.ZipCode).HasColumnName("zip_code").HasMaxLength(10);
            owned.Property(p => p.Country).HasColumnName("country").HasMaxLength(100);
        });
    }
}
```

### Global Query Filters & Soft Delete
```csharp
// Configure global query filters
modelBuilder.Entity<TodoItem>()
    .HasQueryFilter(x => !x.IsDeleted);

modelBuilder.Entity<FhirResource>()
    .HasQueryFilter(x => !x.IsDeleted);
```

### Concurrency Handling
```csharp
// Use PostgreSQL xmin as concurrency token
builder.Property<uint>("xmin")
       .IsRowVersion()
       .HasColumnName("xmin");

// Or use custom row_version field
builder.Property(x => x.RowVersion)
       .IsRowVersion()
       .HasColumnName("row_version");
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
    public DbSet<FhirResource> FhirResources => Set<FhirResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure global query filters
        modelBuilder.Entity<TodoItem>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<FhirResource>()
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

---

## 📊 Migration Management

### Migration Naming Convention
```bash
# Format: {Action/Entity}_{yyyyMMddHHmm}
dotnet ef migrations add CreateFhirResources_202412011030
dotnet ef migrations add AddIndexOnResourceType_202412021200
dotnet ef migrations add AddAuditFields_202412031500
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

### Migration Commands
```bash
# Create migration
dotnet ef migrations add CreateFhirResources_202412011030

# Apply to database
dotnet ef database update

# Remove last migration (not applied)
dotnet ef migrations remove

# Generate idempotent script (for CI/CD)
dotnet ef migrations script --idempotent -o ./artifacts/migrate.sql

# Rollback to specific migration
dotnet ef database update PreviousMigrationName
```

### Migration Safety Principles
- **Avoid breaking changes** directly (rename column/table) → use `ADD NEW → copy data → switch → DROP OLD`
- **Create indexes** using `CREATE INDEX CONCURRENTLY` in manual scripts to avoid long locks
- **Use idempotent scripts** for multiple environments, control version with `__EFMigrationsHistory` table

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

---

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

### Advanced Indexing Strategy
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

### Connection Pooling & Npgsql
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=fhirai;Username=postgres;Password=password;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Lifetime=300;"
}
```

**Note**: Consider configuring **Auto Prepare**/`Max Auto Prepare` if workload repeats statements.

---

## 🗄️ Caching Strategy

### When to Cache
- **Cache**: Read-heavy data, configuration catalogs, lookup identifiers, normalized search results, read-only DTOs
- **Don't cache**: Sensitive data or frequently changing data without reliable invalidation mechanism

### Cache Models
- **Cache-aside** (recommended): Read from cache; miss → DB → set cache → return result
- **Write-through**: Write to DB & cache simultaneously (more complex)
- **Write-behind**: Not recommended for transactional data

### Key Naming, TTL, Invalidation
- **Key pattern**: `"Entity:Name:{id}"`, `"Query:TodoItems:list={listId}:page={n}:size={s}"`
- **TTL recommendation**: `1-15 minutes` depending on data type; allow **sliding** (refresh TTL on hit)
- **Invalidation**: After `SaveChanges`, delete/mark related keys; can **publish message** for other instances to clear (Redis Pub/Sub)

### Memory Cache Implementation
```csharp
public class CachedTodoItemRepository : ITodoItemRepository
{
    private readonly ITodoItemRepository _inner;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Exp = TimeSpan.FromMinutes(5);

    public CachedTodoItemRepository(ITodoItemRepository inner, IMemoryCache cache)
    {
        _inner = inner; 
        _cache = cache;
    }

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var key = $"TodoItem:{id}";
        if (_cache.TryGetValue(key, out TodoItem? cached))
            return cached;

        var entity = await _inner.GetByIdAsync(id, ct);
        if (entity != null) 
            _cache.Set(key, entity, Exp);
        return entity;
    }
}
```

### Distributed Cache (Redis) Helper
```csharp
public static class CacheHelper
{
    public static async Task<T?> GetOrSetAsync<T>(
        IDistributedCache cache, string key, TimeSpan ttl, Func<Task<T?>> factory, CancellationToken ct = default)
        where T : class
    {
        var bytes = await cache.GetAsync(key, ct);
        if (bytes != null)
            return System.Text.Json.JsonSerializer.Deserialize<T>(bytes);

        var value = await factory();
        if (value != null)
        {
            var payload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
            await cache.SetAsync(key, payload, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, ct);
        }
        return value;
    }
}
```

### Cache Invalidation After SaveChanges
```csharp
public interface ICacheInvalidator 
{ 
    void Invalidate(params string[] keys); 
}

public class CacheInvalidatingDbContext : ApplicationDbContext
{
    private readonly ICacheInvalidator _invalidator;
    
    public CacheInvalidatingDbContext(DbContextOptions<ApplicationDbContext> o, ICacheInvalidator inv) : base(o) 
    { 
        _invalidator = inv; 
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var affectedKeys = ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .SelectMany(e => BuildKeysFromEntry(e)) // customize
            .Distinct()
            .ToArray();

        var result = await base.SaveChangesAsync(ct);
        _invalidator.Invalidate(affectedKeys);
        return result;
    }
}
```

---

## 🔒 Database Security

### Row Level Security (RLS)
```sql
-- Enable RLS on tables
ALTER TABLE "todo_items" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "todo_lists" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "fhir_resources" ENABLE ROW LEVEL SECURITY;

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

CREATE POLICY "view_own_fhir_resource" ON "fhir_resources"
    FOR SELECT USING ("created_by" = current_user);

CREATE POLICY "modify_own_fhir_resource" ON "fhir_resources"
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

---

## 📝 Audit Logging

### Application-Level Audit (EF Interceptor/SaveChanges hook)
**Advantages**: Log at application layer, easy to add user/request context  
**Disadvantages**: Missing if operations outside application

```csharp
public class AuditLog
{
    public long Id { get; set; }
    public string TableName { get; set; } = default!;
    public string Action { get; set; } = default!; // INSERT/UPDATE/DELETE
    public string? RecordId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var auditEntries = new List<AuditLog>();
    foreach (var e in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
    {
        var (oldVals, newVals) = GetAuditPayload(e); // serialize to JSON
        auditEntries.Add(new AuditLog {
            TableName = e.Metadata.GetTableName()!,
            Action = e.State.ToString().ToUpperInvariant(),
            RecordId = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
            OldValues = oldVals, 
            NewValues = newVals,
            ChangedBy = GetCurrentUserId(), 
            ChangedAt = DateTime.UtcNow
        });
    }

    var result = await base.SaveChangesAsync(ct);

    if (auditEntries.Count > 0)
    {
        Set<AuditLog>().AddRange(auditEntries);
        await base.SaveChangesAsync(ct);
    }
    return result;
}
```

### Database-Level Audit (PostgreSQL Triggers)
**Advantages**: **No missing** even if operations from outside app  
**Disadvantages**: Hard to attach additional user context if not passing `current_user`/`app.user_id`

```sql
-- Create audit table
CREATE TABLE "audit_logs" (
    "id" BIGSERIAL PRIMARY KEY,
    "table_name" VARCHAR(100) NOT NULL,
    "action" VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    "record_id" TEXT,
    "old_values" JSONB,
    "new_values" JSONB,
    "changed_by" VARCHAR(100),
    "changed_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
    BEGIN
        IF TG_OP = 'INSERT' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'INSERT', NEW."id"::text, to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'UPDATE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'UPDATE', NEW."id"::text, to_jsonb(OLD), to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'DELETE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'DELETE', OLD."id"::text, to_jsonb(OLD), current_user);
            RETURN OLD;
        END IF;
        RETURN NULL;
    END;
$$ LANGUAGE plpgsql;

-- Create audit trigger
CREATE TRIGGER audit_todoitems_trigger
    AFTER INSERT OR UPDATE OR DELETE ON "todo_items"
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_fhir_resources_trg
    AFTER INSERT OR UPDATE OR DELETE ON "fhir_resources"
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();
```

**Note**: Can store **diff** instead of full object using PL/pgSQL or process diff at application layer.

---

## 💾 Backup Strategy
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
```

---

## 🔒 Database Security

### Row Level Security (RLS)
```sql
-- Enable RLS on tables
ALTER TABLE "todo_items" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "todo_lists" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "fhir_resources" ENABLE ROW LEVEL SECURITY;

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

CREATE POLICY "view_own_fhir_resource" ON "fhir_resources"
    FOR SELECT USING ("created_by" = current_user);

CREATE POLICY "modify_own_fhir_resource" ON "fhir_resources"
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

---

## �� Audit Logging

### Application-Level Audit (EF Interceptor/SaveChanges hook)
**Advantages**: Log at application layer, easy to add user/request context  
**Disadvantages**: Missing if operations outside application

```csharp
public class AuditLog
{
    public long Id { get; set; }
    public string TableName { get; set; } = default!;
    public string Action { get; set; } = default!; // INSERT/UPDATE/DELETE
    public string? RecordId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}

public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var auditEntries = new List<AuditLog>();
    foreach (var e in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
    {
        var (oldVals, newVals) = GetAuditPayload(e); // serialize to JSON
        auditEntries.Add(new AuditLog {
            TableName = e.Metadata.GetTableName()!,
            Action = e.State.ToString().ToUpperInvariant(),
            RecordId = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString(),
            OldValues = oldVals, 
            NewValues = newVals,
            ChangedBy = GetCurrentUserId(), 
            ChangedAt = DateTime.UtcNow
        });
    }

    var result = await base.SaveChangesAsync(ct);

    if (auditEntries.Count > 0)
    {
        Set<AuditLog>().AddRange(auditEntries);
        await base.SaveChangesAsync(ct);
    }
    return result;
}
```

### Database-Level Audit (PostgreSQL Triggers)
**Advantages**: **No missing** even if operations from outside app  
**Disadvantages**: Hard to attach additional user context if not passing `current_user`/`app.user_id`

```sql
-- Create audit table
CREATE TABLE "audit_logs" (
    "id" BIGSERIAL PRIMARY KEY,
    "table_name" VARCHAR(100) NOT NULL,
    "action" VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    "record_id" TEXT,
    "old_values" JSONB,
    "new_values" JSONB,
    "changed_by" VARCHAR(100),
    "changed_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
    BEGIN
        IF TG_OP = 'INSERT' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'INSERT', NEW."id"::text, to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'UPDATE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "new_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'UPDATE', NEW."id"::text, to_jsonb(OLD), to_jsonb(NEW), current_user);
            RETURN NEW;
        ELSIF TG_OP = 'DELETE' THEN
            INSERT INTO "audit_logs" ("table_name", "action", "record_id", "old_values", "changed_by")
            VALUES (TG_TABLE_NAME, 'DELETE', OLD."id"::text, to_jsonb(OLD), current_user);
            RETURN OLD;
        END IF;
        RETURN NULL;
    END;
$$ LANGUAGE plpgsql;

-- Create audit trigger
CREATE TRIGGER audit_todoitems_trigger
    AFTER INSERT OR UPDATE OR DELETE ON "todo_items"
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();

CREATE TRIGGER audit_fhir_resources_trg
    AFTER INSERT OR UPDATE OR DELETE ON "fhir_resources"
    FOR EACH ROW EXECUTE FUNCTION audit_trigger_function();
```

**Note**: Can store **diff** instead of full object using PL/pgSQL or process diff at application layer.

---

## 💾 Backup Strategy
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

---

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

## 🚨 Database Checklist

### Design
- [ ] Normalized to 3NF
- [ ] Proper relationships defined
- [ ] Appropriate data types used (including JSONB when needed)
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

### Caching
- [ ] Choose cache model (cache-aside recommended)
- [ ] Key naming + TTL + sliding
- [ ] Invalidation after SaveChanges
- [ ] Redis Pub/Sub for multiple instances

### Audit
- [ ] Choose app-level or DB trigger (or both)
- [ ] Store JSON/JSONB; consider diff
- [ ] Protect audit table (insert only)

---

**🎯 Remember**: Database design is critical for application performance and data integrity. Always follow normalization principles, implement proper indexing, and ensure data security. Regular monitoring and optimization are essential for maintaining good performance.

**Key Principles**:
1. **Naming**: Always use snake_case, Plural, Quoted names for PostgreSQL tables
2. **JSONB**: Use for flexible properties, metadata, FHIR payloads with GIN indexes
3. **Performance**: Implement proper indexing, caching, and query optimization
4. **Security**: Enable RLS, implement audit logging, and encrypt sensitive data
5. **Migration**: Use safe migration strategies with idempotent scripts for CI/CD
