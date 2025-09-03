using FHIRAI.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Identity.Configurations;

public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        // Basic configuration - can be extended as needed
        builder.ToTable("ApplicationUserLogins");
        
        // Add any custom configurations here if needed
    }
}
