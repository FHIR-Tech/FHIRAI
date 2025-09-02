using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("todo_items");

        // Primary key
        builder.HasKey(t => t.Id);

        // Id configuration for Guid
        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration with explicit column mapping
        builder.Property(t => t.ListId)
            .HasColumnName("list_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200);

        builder.Property(t => t.Note)
            .HasColumnName("note")
            .HasMaxLength(1000);

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .IsRequired()
            .HasDefaultValue(Domain.Enums.PriorityLevel.Medium)
            .HasSentinel(Domain.Enums.PriorityLevel.None);

        builder.Property(t => t.Done)
            .HasColumnName("done")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.Reminder)
            .HasColumnName("reminder");

        // Relationships with proper constraint naming
        builder.HasOne(t => t.List)
            .WithMany(l => l.Items)
            .HasForeignKey(t => t.ListId)
            .HasConstraintName("fk_todo_items_todo_lists_list_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes with proper naming (snake_case)
        builder.HasIndex(t => t.ListId)
            .HasDatabaseName("ix_todo_items_list_id");
            
        builder.HasIndex(t => new { t.ListId, t.Done })
            .HasDatabaseName("ix_todo_items_list_id_done");
            
        builder.HasIndex(t => new { t.ListId, t.Priority })
            .HasDatabaseName("ix_todo_items_list_id_priority");
            
        builder.HasIndex(t => t.Reminder)
            .HasDatabaseName("ix_todo_items_reminder");

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
