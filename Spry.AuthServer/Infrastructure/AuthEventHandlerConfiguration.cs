using Spry.AuthServer.Infrastructure.AuthRequestEventHandlers;
using static OpenIddict.Server.OpenIddictServerEvents;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;
using static OpenIddict.Server.OpenIddictServerHandlers.Session;

namespace Spry.AuthServer.Infrastructure
{
    public static class AuthEventHandlerConfiguration
    {
        public static OpenIddictServerBuilder ServerEventHandlers(this OpenIddictServerBuilder builder, IConfiguration configuration)
        {
            builder.RemoveEventHandler(ValidateClientRedirectUri.Descriptor);

            if (configuration.GetValue<bool>("DisablePostLogoutRedirectUriValidation"))
            {
                builder.RemoveEventHandler(ValidateClientPostLogoutRedirectUri.Descriptor);
            }

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
