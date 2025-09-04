using System.Reflection;
using FHIRAI.Application.Common.Interfaces;
using FHIRAI.Domain.Constants;
using FHIRAI.Domain.Repositories;
using FHIRAI.Infrastructure.Data;
using FHIRAI.Infrastructure.Data.Interceptors;
using FHIRAI.Infrastructure.Data.Repositories;
using FHIRAI.Infrastructure.Identity;
using FHIRAI.Infrastructure.Identity.Models;
using FHIRAI.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("FHIRAIDb");
        Guard.Against.Null(connectionString, message: "Connection string 'FHIRAIDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });


        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddScoped<IFhirResourceRepository, FhirResourceRepository>();
        builder.Services.AddScoped<IPatientAccessService, PatientAccessService>();

        builder.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        #region Repository
        builder.Services.AddRepositories(Assembly.GetExecutingAssembly());
        #endregion
    }

    /// <summary>
    /// Đăng ký tự động các class có đuôi tên kết thúc Repositorty và có kế thừa từ IRepository
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    private static void AddRepositories(this IServiceCollection services, Assembly assembly)
    {
        // Lấy tất cả các class trong assembly có tên kết thúc bằng "Repository"
        // và loại bỏ các class generic như RepositoryBase<>
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && !t.IsGenericType
                        && t.Name.EndsWith("Repository"));

        foreach (var impl in repositoryTypes)
        {
            // Tìm interface có tên "I" + tên class, ví dụ: ILanguageRepository cho LanguageRepository
            var interfaceType = impl.GetInterface("I" + impl.Name);
            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, impl);
            }
        }
    }
}
