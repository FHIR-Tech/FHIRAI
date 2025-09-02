using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("todo_lists");

        // Primary key
        builder.HasKey(t => t.Id);

        // Id configuration for Guid
        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration with explicit column mapping
        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200);

        // Value object configuration with explicit column mapping
        builder
            .OwnsOne(b => b.Colour, colour =>
            {
                colour.Property(c => c.Code)
                    .HasColumnName("colour_code")
                    .HasMaxLength(7)
                    .IsRequired();
            });

        // Audit properties (inherited from BaseAuditableEntity) with explicit column mapping
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(t => t.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(t => t.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100);

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(t => t.DeletedBy)
            .HasColumnName("deleted_by");
    }
}
