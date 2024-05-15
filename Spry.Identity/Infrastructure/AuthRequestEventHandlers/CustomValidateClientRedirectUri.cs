using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;

namespace Spry.Identity.Infrastructure.AuthRequestEventHandlers
{
    public sealed class CustomValidateClientRedirectUri : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        //
        // Summary:
        //     Gets the default descriptor definition assigned to this handler.
        public static OpenIddictServerHandlerDescriptor Descriptor { get; } =
            OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ValidateAuthorizationRequestContext>()
            .UseScopedHandler<CustomValidateClientRedirectUri>()
            .SetOrder(ValidateResponseType.Descriptor.Order + 1000)
            .SetType(OpenIddictServerHandlerType.Custom)
            .Build();

        public CustomValidateClientRedirectUri()
        {
            throw new InvalidOperationException(OpenIddictResources.GetResourceString("ID0016"));
        }

        public CustomValidateClientRedirectUri(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException("applicationManager");
        }

        public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateAuthorizationRequestContext context)
        {
            if (context == null)
            {
                //throw new ArgumentNullException("context");
            }
        }
    }

}
