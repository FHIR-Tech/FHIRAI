using System.Reflection;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FHIRAI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<FhirResource> FhirResources => Set<FhirResource>();

    public DbSet<PatientAccess> PatientAccesses => Set<PatientAccess>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply our configurations first
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Call base class to get Identity configurations
        base.OnModelCreating(builder);
        
        // Override Identity table names to use snake_case
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<IdentityRole<string>>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
    }
}
