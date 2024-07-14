using Microsoft.AspNetCore.Identity;

namespace AuthApp.Api.Services.JWT
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(IdentityUser user);
    }
}
