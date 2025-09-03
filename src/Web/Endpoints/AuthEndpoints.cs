using FHIRAI.Infrastructure.Identity.Models;

namespace FHIRAI.Web.Endpoints;

public class AuthEndpoints : EndpointGroupBase
{
    public override string? GroupName => "auth";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
