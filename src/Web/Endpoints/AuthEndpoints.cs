using FHIRAI.Infrastructure.Identity;

namespace FHIRAI.Web.Endpoints;

public class AuthEndpoints : EndpointGroupBase
{
    public override string? GroupName => "auth";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapIdentityApi<ApplicationUser>();
    }
}
