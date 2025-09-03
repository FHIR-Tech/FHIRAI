using FHIRAI.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FHIRAI.Infrastructure.Identity.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // // Table configuration
        // builder.ToTable("users");

        // // Properties with explicit column mapping
        // builder.Property(e => e.Id).HasColumnName("id");
        // builder.Property(e => e.UserName).HasColumnName("user_name");
        // builder.Property(e => e.NormalizedUserName).HasColumnName("normalized_user_name");
        // builder.Property(e => e.Email).HasColumnName("email");
        // builder.Property(e => e.NormalizedEmail).HasColumnName("normalized_email");
        // builder.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");
        // builder.Property(e => e.PasswordHash).HasColumnName("password_hash");
        // builder.Property(e => e.SecurityStamp).HasColumnName("security_stamp");
        // builder.Property(e => e.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        // builder.Property(e => e.PhoneNumber).HasColumnName("phone_number");
        // builder.Property(e => e.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
        // builder.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
        // builder.Property(e => e.LockoutEnd).HasColumnName("lockout_end");
        // builder.Property(e => e.LockoutEnabled).HasColumnName("lockout_enabled");
        // builder.Property(e => e.AccessFailedCount).HasColumnName("access_failed_count");
    }
}
