using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class PatientAccessConfiguration : IEntityTypeConfiguration<PatientAccess>
{
    public void Configure(EntityTypeBuilder<PatientAccess> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("patient_accesses");

        // Primary key
        builder.HasKey(p => p.Id);

        // Id configuration for Guid
        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration with explicit column mapping
        builder.Property(p => p.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.AccessLevel)
            .HasColumnName("access_level")
            .IsRequired();

        builder.Property(p => p.GrantedAt)
            .HasColumnName("granted_at")
            .IsRequired();

        builder.Property(p => p.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(p => p.GrantedBy)
            .HasColumnName("granted_by")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Reason)
            .HasColumnName("reason");

        builder.Property(p => p.IsEmergencyAccess)
            .HasColumnName("is_emergency_access")
            .HasDefaultValue(false);

        builder.Property(p => p.EmergencyJustification)
            .HasColumnName("emergency_justification");

        builder.Property(p => p.IsEnabled)
            .HasColumnName("is_enabled")
            .HasDefaultValue(true);

        // Indexes for performance with proper naming (snake_case)
        builder.HasIndex(p => p.PatientId)
            .HasDatabaseName("ix_patient_accesses_patient_id");
            
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("ix_patient_accesses_user_id");
            
        builder.HasIndex(p => p.AccessLevel)
            .HasDatabaseName("ix_patient_accesses_access_level");
            
        builder.HasIndex(p => p.ExpiresAt)
            .HasDatabaseName("ix_patient_accesses_expires_at");
            
        builder.HasIndex(p => p.IsEnabled)
            .HasDatabaseName("ix_patient_accesses_is_enabled");
            
        builder.HasIndex(p => p.GrantedAt)
            .HasDatabaseName("ix_patient_accesses_granted_at");

        // Composite indexes for common queries
        builder.HasIndex(p => new { p.PatientId, p.UserId, p.AccessLevel })
            .HasDatabaseName("ix_patient_accesses_patient_id_user_id_access_level");
            
        builder.HasIndex(p => new { p.UserId, p.AccessLevel, p.IsEnabled })
            .HasDatabaseName("ix_patient_accesses_user_id_access_level_is_enabled");
            
        builder.HasIndex(p => new { p.PatientId, p.IsEnabled, p.ExpiresAt })
            .HasDatabaseName("ix_patient_accesses_patient_id_is_enabled_expires_at");
            
        builder.HasIndex(p => new { p.PatientId, p.IsEmergencyAccess, p.IsEnabled })
            .HasDatabaseName("ix_patient_accesses_patient_id_is_emergency_access_is_enabled");

        // Audit properties (inherited from BaseAuditableEntity) with explicit column mapping
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(p => p.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(p => p.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100);

        builder.Property(p => p.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(p => p.DeletedBy)
            .HasColumnName("deleted_by");
    }
}
