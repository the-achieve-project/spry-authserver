using Spry.Identity.Infrastructure.AuthRequestEventHandlers;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;
using static OpenIddict.Server.OpenIddictServerHandlers.Session;

namespace Spry.Identity.Infrastructure
{
    public static class AuthEventHandlerConfiguration
    {
        public static OpenIddictServerBuilder ServerEventHandlers(this OpenIddictServerBuilder builder)
        {
            builder.RemoveEventHandler(ValidateClientRedirectUri.Descriptor);
            //builder.RemoveEventHandler(ValidateClientPostLogoutRedirectUri.Descriptor);

            builder.AddEventHandler<GenerateTokenContext>(b =>
            {
                b.UseSingletonHandler<ScopesAsArrayHandler>();
                // make sure this is executed before weird stuff is done with the scopes
                b.SetOrder(int.MinValue);
            });

            builder.AddEventHandler(CustomValidateClientRedirectUri.Descriptor);

            return builder;
        }
    }
}
