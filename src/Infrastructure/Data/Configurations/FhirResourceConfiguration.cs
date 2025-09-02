using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class FhirResourceConfiguration : IEntityTypeConfiguration<FhirResource>
{
    public void Configure(EntityTypeBuilder<FhirResource> builder)
    {
        // ✅ Table configuration - ALWAYS use snake_case, Plural, Quoted
        builder.ToTable("fhir_resources");

        // Primary key
        builder.HasKey(f => f.Id);

        // Id configuration for Guid
        builder.Property(f => f.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration with explicit column mapping
        builder.Property(f => f.ResourceType)
            .HasColumnName("resource_type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FhirId)
            .HasColumnName("fhir_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.VersionId)
            .HasColumnName("version_id")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(f => f.ResourceJson)
            .HasColumnName("resource_json")
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasDefaultValue("active");

        builder.Property(f => f.LastUpdated)
            .HasColumnName("last_updated");

        builder.Property(f => f.FhirCreated)
            .HasColumnName("fhir_created");

        builder.Property(f => f.SearchParameters)
            .HasColumnName("search_parameters")
            .HasColumnType("jsonb");

        builder.Property(f => f.SecurityLabels)
            .HasColumnName("security_labels")
            .HasColumnType("jsonb");

        builder.Property(f => f.Tags)
            .HasColumnName("tags")
            .HasColumnType("jsonb");

        builder.Property(f => f.PatientReference)
            .HasColumnName("patient_reference")
            .HasMaxLength(255);

        builder.Property(f => f.OrganizationReference)
            .HasColumnName("organization_reference")
            .HasMaxLength(255);

        builder.Property(f => f.PractitionerReference)
            .HasColumnName("practitioner_reference")
            .HasMaxLength(255);

        // Indexes for performance with proper naming (snake_case)
        builder.HasIndex(f => f.ResourceType)
            .HasDatabaseName("ix_fhir_resources_resource_type");
            
        builder.HasIndex(f => f.FhirId)
            .HasDatabaseName("ix_fhir_resources_fhir_id");
            
        builder.HasIndex(f => new { f.ResourceType, f.FhirId }) // Composite index for FHIR lookup
            .HasDatabaseName("ix_fhir_resources_resource_type_fhir_id");
            
        builder.HasIndex(f => f.PatientReference)
            .HasDatabaseName("ix_fhir_resources_patient_reference");
            
        builder.HasIndex(f => f.OrganizationReference)
            .HasDatabaseName("ix_fhir_resources_organization_reference");
            
        builder.HasIndex(f => f.PractitionerReference)
            .HasDatabaseName("ix_fhir_resources_practitioner_reference");
            
        builder.HasIndex(f => f.Status)
            .HasDatabaseName("ix_fhir_resources_status");
            
        builder.HasIndex(f => f.LastUpdated)
            .HasDatabaseName("ix_fhir_resources_last_updated");
            
        builder.HasIndex(f => f.FhirCreated)
            .HasDatabaseName("ix_fhir_resources_fhir_created");

        // Composite indexes for common queries
        builder.HasIndex(f => new { f.ResourceType, f.Status, f.LastUpdated })
            .HasDatabaseName("ix_fhir_resources_resource_type_status_last_updated");
            
        builder.HasIndex(f => new { f.PatientReference, f.ResourceType, f.Status })
            .HasDatabaseName("ix_fhir_resources_patient_reference_resource_type_status");
            
        builder.HasIndex(f => new { f.OrganizationReference, f.ResourceType, f.Status })
            .HasDatabaseName("ix_fhir_resources_organization_reference_resource_type_status");

        // GIN index for JSONB fields (PostgreSQL specific)
        builder.HasIndex(f => f.SearchParameters)
            .HasDatabaseName("ix_fhir_resources_search_parameters")
            .HasMethod("gin");
            
        builder.HasIndex(f => f.SecurityLabels)
            .HasDatabaseName("ix_fhir_resources_security_labels")
            .HasMethod("gin");
            
        builder.HasIndex(f => f.Tags)
            .HasDatabaseName("ix_fhir_resources_tags")
            .HasMethod("gin");

        // Audit properties (inherited from BaseAuditableEntity) with explicit column mapping
        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);

        builder.Property(f => f.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(f => f.LastModifiedBy)
            .HasColumnName("last_modified_by")
            .HasMaxLength(100);

        builder.Property(f => f.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(f => f.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(f => f.DeletedBy)
            .HasColumnName("deleted_by");
    }
}
