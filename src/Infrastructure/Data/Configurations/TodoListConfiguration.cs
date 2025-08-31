using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        // Table configuration
        builder.ToTable("TodoLists");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties configuration
        builder.Property(t => t.Title)
            .HasMaxLength(200);

        // Value object configuration
        builder
            .OwnsOne(b => b.Colour)
            .Property(c => c.Code)
            .HasMaxLength(7)
            .IsRequired();

        // Audit properties (inherited from BaseAuditableEntity)
        builder.Property(t => t.Created)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(100);

        builder.Property(t => t.LastModified);

        builder.Property(t => t.LastModifiedBy)
            .HasMaxLength(100);
    }
}
