using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Spry.Identity.Infrastructure.AuthRequestEventHandlers
{
    public class ScopesAsArrayHandler : IOpenIddictServerHandler<OpenIddictServerEvents.GenerateTokenContext>
    {
        public ValueTask HandleAsync(OpenIddictServerEvents.GenerateTokenContext context)
        {
            // backup OpenIdDict's custom scope claims
            var scopes = context.Principal.GetClaims("oi_scp");

            // remove OpenIdDict's custom scope claims
            context.Principal.RemoveClaims("oi_scp");
            context.Principal.RemoveClaims("scope");

            // add backed up scopes as "scope" claims
            context.Principal.AddClaims("scope", scopes);

            return ValueTask.CompletedTask;
        }
    }
}
