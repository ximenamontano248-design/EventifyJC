using System.Security.Claims;
using EventifyJC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EventifyJC.Identity;

// Agrega el Rol y el Nombre del usuario como claims dentro de la cookie de sesión,
// para poder usar [Authorize(Roles = "admin")] más adelante y leer User.Identity.Name.
public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<Usuario> userManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.Role, user.Rol));
        identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Nombre));
        return identity;
    }
}
