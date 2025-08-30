# FHIRAI - Security Guide

## 🔐 Authentication System

### JWT Bearer Tokens
FHIRAI sử dụng JWT (JSON Web Tokens) cho authentication:

```csharp
// JWT Configuration in appsettings.json
{
  "JwtSettings": {
    "Key": "your-secret-key-here",
    "Issuer": "FHIRAI",
    "Audience": "FHIRAI-Users",
    "DurationInMinutes": 60
  }
}
```

#### JWT Token Structure
```csharp
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int DurationInMinutes { get; set; }
}
```

#### Token Generation
```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}
```

### Azure Key Vault Integration
Sử dụng Azure Key Vault để lưu trữ secrets an toàn:

```csharp
// Key Vault Configuration
public class KeyVaultSettings
{
    public string VaultUrl { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

// Key Vault Service
public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;

    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
```

### ASP.NET Core Identity
Implementation của ASP.NET Core Identity:

```csharp
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
        return user.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.Users.FirstAsync(u => u.Id == userId);
        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        var result = await _authorizationService.AuthorizeAsync(principal, policyName);
        return result.Succeeded;
    }
}
```

## 🚪 Authorization Rules

### Role-based Access Control (RBAC)
```csharp
// Role Constants
public static class Roles
{
    public const string Administrator = nameof(Administrator);
    public const string Manager = nameof(Manager);
    public const string User = nameof(User);
}

// Policy Constants
public static class Policies
{
    public const string CanPurge = nameof(CanPurge);
    public const string CanViewAllUsers = nameof(CanViewAllUsers);
    public const string CanManageUsers = nameof(CanManageUsers);
}
```

### Policy-based Authorization
```csharp
// Policy Configuration
public static class AuthorizationPolicies
{
    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(Policies.CanPurge, policy =>
            policy.RequireRole(Roles.Administrator));

        options.AddPolicy(Policies.CanViewAllUsers, policy =>
            policy.RequireRole(Roles.Administrator, Roles.Manager));

        options.AddPolicy(Policies.CanManageUsers, policy =>
            policy.RequireRole(Roles.Administrator));
    }
}
```

### Resource-based Authorization
```csharp
// Custom Authorization Attribute
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _policy;

    public AuthorizeAttribute(string policy = "")
    {
        _policy = policy;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!string.IsNullOrEmpty(_policy))
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!identityService.AuthorizeAsync(userId, _policy).Result)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
```

### Custom Authorization Attributes
```csharp
// Usage in Controllers/Endpoints
[Authorize(Policies.CanPurge)]
public async Task<IActionResult> PurgeData()
{
    // Only administrators can access
}

[Authorize(Roles = Roles.Manager)]
public async Task<IActionResult> ViewReports()
{
    // Only managers and administrators can access
}
```

## 🛡️ Security Best Practices

### Input Validation
Sử dụng FluentValidation cho input validation:

```csharp
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters.");

        RuleFor(v => v.ListId)
            .GreaterThan(0)
            .WithMessage("ListId must be greater than 0.");
    }
}
```

### SQL Injection Prevention
Entity Framework Core tự động ngăn chặn SQL injection:

```csharp
// ✅ Safe - Parameterized queries
var items = await _context.TodoItems
    .Where(x => x.ListId == listId)
    .ToListAsync();

// ❌ Dangerous - String concatenation (avoid this)
var sql = $"SELECT * FROM TodoItems WHERE ListId = {listId}";
```

### XSS Protection
```csharp
// Output encoding in Razor views
@Html.Encode(Model.Title)

// JSON serialization with proper encoding
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
```

### CSRF Protection
```csharp
// Anti-forgery token configuration
services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
});
```

### HTTPS Enforcement
```csharp
// HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

## 🔒 Data Protection

### Sensitive Data Encryption
```csharp
// Data Protection API
public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtector _protector;

    public DataProtectionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("FHIRAI.SensitiveData");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string encryptedText)
    {
        return _protector.Unprotect(encryptedText);
    }
}
```

### Audit Logging
```csharp
// Base auditable entity
public abstract class BaseAuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }
    public DateTime Created { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
}

// Audit interceptor
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = DateTime.UtcNow;
                entry.Entity.CreatedBy = GetCurrentUserId();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModified = DateTime.UtcNow;
                entry.Entity.LastModifiedBy = GetCurrentUserId();
            }
        }
    }
}
```

### Data Masking
```csharp
// PII Data Masking
public static class DataMaskingExtensions
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;
        
        var parts = email.Split('@');
        if (parts.Length != 2) return email;
        
        var username = parts[0];
        var domain = parts[1];
        
        if (username.Length <= 2) return email;
        
        var maskedUsername = username[0] + new string('*', username.Length - 2) + username[^1];
        return $"{maskedUsername}@{domain}";
    }

    public static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4) return phoneNumber;
        
        return phoneNumber[..^4] + "****";
    }
}
```

### Backup Security
```csharp
// Encrypted backup configuration
public class BackupSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string BackupPath { get; set; } = string.Empty;
    public string EncryptionKey { get; set; } = string.Empty;
    public bool EnableCompression { get; set; } = true;
}
```

## 🚨 Security Checklist

### Authentication
- [ ] JWT configuration secure and properly configured
- [ ] Token expiration time appropriate (not too long)
- [ ] Refresh token mechanism implemented
- [ ] Azure Key Vault integration for secrets
- [ ] Password policies enforced
- [ ] Multi-factor authentication (MFA) enabled for sensitive operations

### Authorization
- [ ] Role-based access control (RBAC) implemented
- [ ] Policy-based authorization configured
- [ ] Resource-based authorization for sensitive data
- [ ] Principle of least privilege applied
- [ ] Regular access reviews conducted

### Data Protection
- [ ] HTTPS enabled in production
- [ ] Sensitive data encrypted at rest
- [ ] Data in transit encrypted (TLS 1.2+)
- [ ] Audit logging enabled for all sensitive operations
- [ ] Data masking implemented for PII
- [ ] Backup encryption enabled

### Input Validation
- [ ] All user inputs validated
- [ ] SQL injection prevention measures in place
- [ ] XSS protection implemented
- [ ] CSRF protection enabled
- [ ] File upload validation and scanning

### Error Handling
- [ ] Error messages don't leak sensitive information
- [ ] Proper HTTP status codes returned
- [ ] Security exceptions logged appropriately
- [ ] No stack traces exposed in production

### Monitoring & Logging
- [ ] Security events logged and monitored
- [ ] Failed authentication attempts tracked
- [ ] Unusual access patterns detected
- [ ] Regular security audits conducted

## 🔧 Security Configuration

### Program.cs Security Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Security Services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicies.AddPolicies(options);
});

// Security Headers
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Security Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
```

---

**🎯 Remember**: Security is not a feature, it's a fundamental requirement. Always follow the principle of defense in depth, implement security at every layer, and regularly review and update security measures. Never trust user input, always validate and sanitize data, and keep security configurations up to date.
