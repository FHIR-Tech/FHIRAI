using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Table configuration
        builder.ToTable("TodoItems");

        // Primary key
        builder.HasKey(t => t.Id);

        // Id configuration for Guid
        builder.Property(t => t.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration
        builder.Property(t => t.ListId)
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(200);

        builder.Property(t => t.Note)
            .HasMaxLength(1000);

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasDefaultValue(Domain.Enums.PriorityLevel.Medium)
            .HasSentinel(Domain.Enums.PriorityLevel.None);

        builder.Property(t => t.Done)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.Reminder);

        // Relationships
        builder.HasOne(t => t.List)
            .WithMany(l => l.Items)
            .HasForeignKey(t => t.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.ListId);
        builder.HasIndex(t => new { t.ListId, t.Done });
        builder.HasIndex(t => new { t.ListId, t.Priority });
        builder.HasIndex(t => t.Reminder);

        // Audit properties (inherited from BaseAuditableEntity)
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.LastModifiedAt);

        builder.Property(t => t.LastModifiedBy)
            .HasMaxLength(100);
    }
}
