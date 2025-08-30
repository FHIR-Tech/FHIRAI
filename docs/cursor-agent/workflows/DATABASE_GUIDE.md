# FHIRAI - Database Guide

## 🗄️ Database Design Principles

### Normalization
FHIRAI tuân thủ 3NF (Third Normal Form) để đảm bảo data integrity:

```sql
-- ✅ Normalized design
CREATE TABLE "TodoLists" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(200) NOT NULL,
    "Colour_Code" VARCHAR(7) NOT NULL,
    "Created" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100)
);

CREATE TABLE "TodoItems" (
    "Id" SERIAL PRIMARY KEY,
    "ListId" INTEGER NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Done" BOOLEAN NOT NULL DEFAULT FALSE,
    "Priority" INTEGER NOT NULL DEFAULT 1,
    "Note" VARCHAR(1000),
    "Created" TIMESTAMP NOT NULL,
    "CreatedBy" VARCHAR(100),
    "LastModified" TIMESTAMP,
    "LastModifiedBy" VARCHAR(100),
    FOREIGN KEY ("ListId") REFERENCES "TodoLists"("Id") ON DELETE CASCADE
);
```

### Indexing Strategy
```sql
-- Primary indexes (automatic)
CREATE INDEX "IX_TodoItems_ListId" ON "TodoItems"("ListId");

-- Composite indexes for common queries
CREATE INDEX "IX_TodoItems_ListId_Done" ON "TodoItems"("ListId", "Done");
CREATE INDEX "IX_TodoItems_ListId_Priority" ON "TodoItems"("ListId", "Priority");

-- Full-text search indexes
CREATE INDEX "IX_TodoItems_Title_Search" ON "TodoItems" USING GIN (to_tsvector('english', "Title"));
```

### Relationship Design
```sql
-- One-to-Many: TodoList -> TodoItems
ALTER TABLE "TodoItems" 
ADD CONSTRAINT "FK_TodoItems_TodoLists_ListId" 
FOREIGN KEY ("ListId") REFERENCES "TodoLists"("Id") ON DELETE CASCADE;

-- Many-to-Many: Users -> Roles (if needed)
CREATE TABLE "UserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles"("Id") ON DELETE CASCADE
);
```

### Data Types
```sql
-- PostgreSQL-specific data types
CREATE TABLE "ExampleTable" (
    "Id" SERIAL PRIMARY KEY,                    -- Auto-incrementing integer
    "Title" VARCHAR(200) NOT NULL,              -- Variable-length string with limit
    "Description" TEXT,                         -- Unlimited text
    "Price" DECIMAL(10,2),                     -- Precise decimal
    "CreatedDate" TIMESTAMP NOT NULL,           -- Date and time
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,   -- Boolean
    "Tags" TEXT[],                             -- Array of text
    "Metadata" JSONB,                          -- JSON data
    "BinaryData" BYTEA                         -- Binary data
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
        builder.ToTable("TodoItems");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000);

        builder.Property(x => x.Done)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(PriorityLevel.Medium);

        // Relationships
        builder.HasOne(x => x.List)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.ListId);
        builder.HasIndex(x => new { x.ListId, x.Done });
        builder.HasIndex(x => new { x.ListId, x.Priority });

        // Audit properties
        builder.Property(x => x.Created)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModified);

        builder.Property(x => x.LastModifiedBy)
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
        builder.ToTable("Colours");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
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
            location.Property(l => l.Street).HasMaxLength(200);
            location.Property(l => l.City).HasMaxLength(100);
            location.Property(l => l.State).HasMaxLength(50);
            location.Property(l => l.ZipCode).HasMaxLength(10);
            location.Property(l => l.Country).HasMaxLength(100);
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
            name: "TodoLists",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Colour_Code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoLists", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TodoItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ListId = table.Column<int>(type: "integer", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Done = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TodoItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_TodoItems_TodoLists_ListId",
                    column: x => x.ListId,
                    principalTable: "TodoLists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TodoItems_ListId",
            table: "TodoItems",
            column: "ListId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TodoItems");
        migrationBuilder.DropTable(name: "TodoLists");
    }
}
```

### Data Seeding
```csharp
public partial class SeedInitialData_20241201 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seed TodoLists
        migrationBuilder.InsertData(
            table: "TodoLists",
            columns: new[] { "Title", "Colour_Code", "Created", "CreatedBy" },
            values: new object[,]
            {
                { "Shopping List", "#FF0000", DateTime.UtcNow, "system" },
                { "Work Tasks", "#0000FF", DateTime.UtcNow, "system" },
                { "Personal Goals", "#008000", DateTime.UtcNow, "system" }
            });

        // Seed TodoItems
        migrationBuilder.InsertData(
            table: "TodoItems",
            columns: new[] { "ListId", "Title", "Done", "Priority", "Created", "CreatedBy" },
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
        migrationBuilder.DeleteData(table: "TodoItems", keyColumn: "Id", keyValues: new object[] { 1, 2, 3, 4, 5, 6 });
        migrationBuilder.DeleteData(table: "TodoLists", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });
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
CREATE INDEX CONCURRENTLY "IX_TodoItems_ListId_Done_Priority" 
ON "TodoItems"("ListId", "Done", "Priority");

-- Partial indexes for filtered queries
CREATE INDEX CONCURRENTLY "IX_TodoItems_Active_Items" 
ON "TodoItems"("ListId", "Priority") 
WHERE "Done" = FALSE;

-- Full-text search indexes
CREATE INDEX CONCURRENTLY "IX_TodoItems_Title_Search" 
ON "TodoItems" USING GIN (to_tsvector('english', "Title"));

-- Covering indexes for frequently accessed data
CREATE INDEX CONCURRENTLY "IX_TodoItems_Covering" 
ON "TodoItems"("ListId") 
INCLUDE ("Title", "Done", "Priority");
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
ALTER TABLE "TodoItems" ENABLE ROW LEVEL SECURITY;
ALTER TABLE "TodoLists" ENABLE ROW LEVEL SECURITY;

-- Create policies
CREATE POLICY "Users_Can_View_Own_TodoItems" ON "TodoItems"
    FOR SELECT USING (
        "CreatedBy" = current_user OR 
        EXISTS (
            SELECT 1 FROM "TodoLists" 
            WHERE "Id" = "TodoItems"."ListId" 
            AND "CreatedBy" = current_user
        )
    );

CREATE POLICY "Users_Can_Modify_Own_TodoItems" ON "TodoItems"
    FOR ALL USING ("CreatedBy" = current_user);
```

### Data Encryption
```sql
-- Column-level encryption (if needed)
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Encrypt sensitive data
UPDATE "Users" 
SET "SSN" = pgp_sym_encrypt("SSN", 'encryption_key_here')
WHERE "SSN" IS NOT NULL;
```

### Audit Logging
```sql
-- Create audit table
CREATE TABLE "AuditLogs" (
    "Id" SERIAL PRIMARY KEY,
    "TableName" VARCHAR(100) NOT NULL,
    "Action" VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    "RecordId" INTEGER,
    "OldValues" JSONB,
    "NewValues" JSONB,
    "ChangedBy" VARCHAR(100),
    "ChangedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create audit trigger function
CREATE OR REPLACE FUNCTION audit_trigger_function()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO "AuditLogs" ("TableName", "Action", "RecordId", "NewValues", "ChangedBy")
        VALUES (TG_TABLE_NAME, 'INSERT', NEW."Id", to_jsonb(NEW), current_user);
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO "AuditLogs" ("TableName", "Action", "RecordId", "OldValues", "NewValues", "ChangedBy")
        VALUES (TG_TABLE_NAME, 'UPDATE', NEW."Id", to_jsonb(OLD), to_jsonb(NEW), current_user);
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO "AuditLogs" ("TableName", "Action", "RecordId", "OldValues", "ChangedBy")
        VALUES (TG_TABLE_NAME, 'DELETE', OLD."Id", to_jsonb(OLD), current_user);
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create audit trigger
CREATE TRIGGER audit_todoitems_trigger
    AFTER INSERT OR UPDATE OR DELETE ON "TodoItems"
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
