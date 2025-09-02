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
        base.OnModelCreating(builder);
        
        // Apply our configurations after the base class
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
