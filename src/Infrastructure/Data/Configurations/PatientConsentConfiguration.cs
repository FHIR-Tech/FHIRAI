using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class PatientConsentConfiguration : IEntityTypeConfiguration<PatientConsent>
{
    public void Configure(EntityTypeBuilder<PatientConsent> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("patient_consents");

        // Primary key
        builder.HasKey(pc => pc.Id);

        // Id configuration for Guid
        builder.Property(pc => pc.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Foreign Key fields with explicit column mapping
        builder.Property(pc => pc.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        // Core Consent fields with explicit column mapping
        builder.Property(pc => pc.ConsentType)
            .HasColumnName("consent_type")
            .IsRequired();

        builder.Property(pc => pc.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Timing fields with explicit column mapping
        builder.Property(pc => pc.GrantedAt)
            .HasColumnName("granted_at")
            .IsRequired();

        builder.Property(pc => pc.ExpiresAt)
            .HasColumnName("expires_at");

        // Authorization fields with explicit column mapping
        builder.Property(pc => pc.GrantedBy)
            .HasColumnName("granted_by")
            .IsRequired()
            .HasMaxLength(255);

        // Revocation fields with explicit column mapping
        builder.Property(pc => pc.RevokedBy)
            .HasColumnName("revoked_by")
            .HasMaxLength(255);

        builder.Property(pc => pc.RevokedAt)
            .HasColumnName("revoked_at");

        // Electronic Consent fields with explicit column mapping
        builder.Property(pc => pc.IsElectronicConsent)
            .HasColumnName("is_electronic_consent");

        builder.Property(pc => pc.ConsentIpAddress)
            .HasColumnName("consent_ip_address")
            .HasMaxLength(45);

        builder.Property(pc => pc.UserAgent)
            .HasColumnName("user_agent");

        // Details fields with explicit column mapping
        builder.Property(pc => pc.Purpose)
            .HasColumnName("purpose");

        builder.Property(pc => pc.Details)
            .HasColumnName("details");

        // Relationships with proper constraint naming
        builder.HasOne(pc => pc.Patient)
            .WithMany(p => p.PatientConsents)
            .HasForeignKey(pc => pc.PatientId)
            .HasConstraintName("fk_patient_consents_patients_patient_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance with proper naming (snake_case)
        builder.HasIndex(pc => pc.PatientId)
            .HasDatabaseName("ix_patient_consents_patient_id");

        builder.HasIndex(pc => pc.ConsentType)
            .HasDatabaseName("ix_patient_consents_consent_type");

        builder.HasIndex(pc => pc.IsActive)
            .HasDatabaseName("ix_patient_consents_is_active");

        builder.HasIndex(pc => pc.GrantedAt)
            .HasDatabaseName("ix_patient_consents_granted_at");

        builder.HasIndex(pc => pc.ExpiresAt)
            .HasDatabaseName("ix_patient_consents_expires_at");

        builder.HasIndex(pc => pc.RevokedAt)
            .HasDatabaseName("ix_patient_consents_revoked_at");

        builder.HasIndex(pc => pc.IsElectronicConsent)
            .HasDatabaseName("ix_patient_consents_is_electronic_consent");

        // Composite indexes for common queries
        builder.HasIndex(pc => new { pc.PatientId, pc.ConsentType, pc.IsActive })
            .HasDatabaseName("ix_patient_consents_patient_id_consent_type_is_active");

        builder.HasIndex(pc => new { pc.PatientId, pc.IsActive, pc.ExpiresAt })
            .HasDatabaseName("ix_patient_consents_patient_id_is_active_expires_at");

        builder.HasIndex(pc => new { pc.ConsentType, pc.IsActive, pc.GrantedAt })
            .HasDatabaseName("ix_patient_consents_consent_type_is_active_granted_at");

        // Audit properties (inherited from BaseAuditableEntity) with explicit column mapping
        builder.Property(pc => pc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pc => pc.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(pc => pc.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(pc => pc.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100);

        builder.Property(pc => pc.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(pc => pc.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(pc => pc.DeletedBy)
            .HasColumnName("deleted_by");
    }
}
