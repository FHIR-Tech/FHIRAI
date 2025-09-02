using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class PatientAccessConfiguration : IEntityTypeConfiguration<PatientAccess>
{
    public void Configure(EntityTypeBuilder<PatientAccess> builder)
    {
        // Table configuration
        builder.ToTable("PatientAccesses");

        // Primary key
        builder.HasKey(p => p.Id);

        // Id configuration for Guid
        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration
        builder.Property(p => p.PatientId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.AccessLevel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.GrantedAt)
            .IsRequired();

        builder.Property(p => p.ExpiresAt);

        builder.Property(p => p.GrantedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Reason);

        builder.Property(p => p.EmergencyJustification);

        builder.Property(p => p.IsEmergencyAccess)
            .HasDefaultValue(false);

        builder.Property(p => p.IsEnabled)
            .HasDefaultValue(true);

        // Indexes for performance
        builder.HasIndex(p => p.PatientId);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.AccessLevel);
        builder.HasIndex(p => p.ExpiresAt);
        builder.HasIndex(p => p.IsEnabled);
        builder.HasIndex(p => p.GrantedAt);

        // Composite indexes for common queries
        builder.HasIndex(p => new { p.PatientId, p.UserId, p.AccessLevel });
        builder.HasIndex(p => new { p.UserId, p.AccessLevel, p.IsEnabled });
        builder.HasIndex(p => new { p.PatientId, p.IsEnabled, p.ExpiresAt });
        builder.HasIndex(p => new { p.PatientId, p.IsEmergencyAccess, p.IsEnabled });

        // Audit properties (inherited from BaseAuditableEntity)
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.LastModifiedAt);

        builder.Property(p => p.LastModifiedBy)
            .HasMaxLength(100);
    }
}
