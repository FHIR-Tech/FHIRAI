using FHIRAI.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Identity.Configurations;

public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
{
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        // Basic configuration - can be extended as needed
        builder.ToTable("ApplicationUserTokens");
        
        // Add any custom configurations here if needed
    }
}
