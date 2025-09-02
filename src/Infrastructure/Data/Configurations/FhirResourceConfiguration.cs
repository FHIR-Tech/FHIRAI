using FHIRAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Data.Configurations;

public class FhirResourceConfiguration : IEntityTypeConfiguration<FhirResource>
{
    public void Configure(EntityTypeBuilder<FhirResource> builder)
    {
        // Table configuration
        builder.ToTable("FhirResources");

        // Primary key
        builder.HasKey(f => f.Id);

        // Id configuration for Guid
        builder.Property(f => f.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Properties configuration
        builder.Property(f => f.ResourceType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.FhirId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.VersionId)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(f => f.ResourceJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(f => f.Status)
            .HasMaxLength(50)
            .HasDefaultValue("active");

        builder.Property(f => f.SearchParameters)
            .HasColumnType("jsonb");

        builder.Property(f => f.SecurityLabels)
            .HasColumnType("jsonb");

        builder.Property(f => f.Tags)
            .HasColumnType("jsonb");

        builder.Property(f => f.PatientReference)
            .HasMaxLength(255);

        builder.Property(f => f.OrganizationReference)
            .HasMaxLength(255);

        builder.Property(f => f.PractitionerReference)
            .HasMaxLength(255);

        // Indexes for performance
        builder.HasIndex(f => f.ResourceType);
        builder.HasIndex(f => f.FhirId);
        builder.HasIndex(f => new { f.ResourceType, f.FhirId }); // Composite index for FHIR lookup
        builder.HasIndex(f => f.PatientReference);
        builder.HasIndex(f => f.OrganizationReference);
        builder.HasIndex(f => f.PractitionerReference);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.LastUpdated);
        builder.HasIndex(f => f.FhirCreated);

        // Composite indexes for common queries
        builder.HasIndex(f => new { f.ResourceType, f.Status, f.LastUpdated });
        builder.HasIndex(f => new { f.PatientReference, f.ResourceType, f.Status });
        builder.HasIndex(f => new { f.OrganizationReference, f.ResourceType, f.Status });

        // GIN index for JSONB fields (PostgreSQL specific)
        builder.HasIndex(f => f.SearchParameters)
            .HasMethod("gin");
        builder.HasIndex(f => f.SecurityLabels)
            .HasMethod("gin");
        builder.HasIndex(f => f.Tags)
            .HasMethod("gin");

        // Audit properties (inherited from BaseAuditableEntity)
        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.CreatedBy)
            .HasMaxLength(100);

        builder.Property(f => f.LastModifiedAt);

        builder.Property(f => f.LastModifiedBy)
            .HasMaxLength(100);
    }
}
