using System.Reflection;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Entities;
using FHIRAI.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FHIRAI.Domain.Common;

namespace FHIRAI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<FhirResource> FhirResources => Set<FhirResource>();

    public DbSet<PatientAccess> PatientAccesses => Set<PatientAccess>();

        /// <summary>
        /// Khởi tạo DbSet<TEntity> tự động cho các thực thể trong DbContext kế thừa từ EntityBase có kiểu ID khóa chính với long
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public new DbSet<TEntity> Set<TEntity>()
            where TEntity : BaseEntity
            => base.Set<TEntity>();

        #region Identity
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationRoleClaim> ApplicationRoleClaims { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserClaim> ApplicationUserClaims { get; set; }
        public DbSet<ApplicationUserLogin> ApplicationUserLogins { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<ApplicationUserToken> ApplicationUserTokens { get; set; }
        #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Gọi base.OnModelCreating TRƯỚC (theo Microsoft pattern)
        base.OnModelCreating(builder);
        
        // Apply our configurations AFTER Identity configuration
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
