using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("patients");

        // Primary key
        builder.HasKey(p => p.Id);

        // Id configuration for Guid
        builder.Property(p => p.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // FHIR Integration fields with explicit column mapping
        builder.Property(p => p.FhirPatientId)
            .HasColumnName("fhir_patient_id")
            .IsRequired()
            .HasMaxLength(255);

        // Core Identity fields with explicit column mapping
        builder.Property(p => p.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(255);

        builder.Property(p => p.DateOfBirth)
            .HasColumnName("date_of_birth");

        builder.Property(p => p.Gender)
            .HasColumnName("gender")
            .HasMaxLength(10);

        builder.Property(p => p.MaritalStatus)
            .HasColumnName("marital_status")
            .HasMaxLength(50);

        // Contact Information fields with explicit column mapping
        builder.Property(p => p.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(p => p.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(p => p.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(500);

        builder.Property(p => p.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(500);

        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(100);

        builder.Property(p => p.State)
            .HasColumnName("state")
            .HasMaxLength(100);

        builder.Property(p => p.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(20);

        builder.Property(p => p.Country)
            .HasColumnName("country")
            .HasMaxLength(100);

        // Status & Configuration fields with explicit column mapping
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.PatientType)
            .HasColumnName("patient_type")
            .HasMaxLength(50);

        // Emergency Contact fields with explicit column mapping
        builder.Property(p => p.EmergencyContactName)
            .HasColumnName("emergency_contact_name")
            .HasMaxLength(255);

        builder.Property(p => p.EmergencyContactPhone)
            .HasColumnName("emergency_contact_phone")
            .HasMaxLength(50);

        builder.Property(p => p.EmergencyContactEmail)
            .HasColumnName("emergency_contact_email")
            .HasMaxLength(255);

        builder.Property(p => p.EmergencyContactRelationship)
            .HasColumnName("emergency_contact_relationship")
            .HasMaxLength(100);

        // Organization & Provider fields with explicit column mapping
        builder.Property(p => p.PrimaryCareProviderId)
            .HasColumnName("primary_care_provider_id")
            .HasMaxLength(255);

        builder.Property(p => p.ManagingOrganizationId)
            .HasColumnName("managing_organization_id")
            .HasMaxLength(255);

        // Indexes for performance with proper naming (snake_case)
        builder.HasIndex(p => p.FhirPatientId)
            .HasDatabaseName("ix_patients_fhir_patient_id")
            .IsUnique();

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("ix_patients_status");

        builder.HasIndex(p => p.PatientType)
            .HasDatabaseName("ix_patients_patient_type");

        builder.HasIndex(p => p.DateOfBirth)
            .HasDatabaseName("ix_patients_date_of_birth");

        builder.HasIndex(p => p.Gender)
            .HasDatabaseName("ix_patients_gender");

        builder.HasIndex(p => p.Email)
            .HasDatabaseName("ix_patients_email");

        builder.HasIndex(p => p.Phone)
            .HasDatabaseName("ix_patients_phone");

        // Composite indexes for common queries
        builder.HasIndex(p => new { p.Status, p.PatientType })
            .HasDatabaseName("ix_patients_status_patient_type");

        builder.HasIndex(p => new { p.LastName, p.FirstName })
            .HasDatabaseName("ix_patients_last_name_first_name");

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
