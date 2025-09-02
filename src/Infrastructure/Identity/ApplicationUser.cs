using Microsoft.AspNetCore.Identity;

namespace FHIRAI.Infrastructure.Identity;

/// <summary>
/// Custom Application User entity
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Bạn có thể thêm custom properties ở đây
    // Ví dụ:
    // public string FirstName { get; set; }
    // public string LastName { get; set; }
    // public DateTime CreatedAt { get; set; }
}
