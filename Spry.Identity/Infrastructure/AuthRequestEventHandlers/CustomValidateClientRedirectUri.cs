using OpenIddict.Abstractions;
using OpenIddict.Server;
using Spry.Identity.SeedWork;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;

namespace Spry.Identity.Infrastructure.AuthRequestEventHandlers
{
    public sealed class CustomValidateClientRedirectUri : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateAuthorizationRequestContext>
    {
        readonly IOpenIddictApplicationManager _applicationManager;
        readonly IConfiguration _configuration;
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

        public CustomValidateClientRedirectUri(IOpenIddictApplicationManager applicationManager, IConfiguration configuration)
        {
            _configuration = configuration;
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
        }

        public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateAuthorizationRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var requestRedirectUrl = new Uri(context.Request.RedirectUri!);

            if (requestRedirectUrl.Host.Contains("host.docker.internal"))
            {
                return;
            }

            string[] achievePayrollClients = _configuration.GetSection("IdentityServer:PayrollClients").Get<string[]>()!;

            if (achievePayrollClients.Contains(context.Request.ClientId) && context.Request.AcrValues is not null)
            {
                var tenant = context.Request.AcrValues.Split(":")[1];

                if (!requestRedirectUrl.IsWellFormedOriginalString())
                {
                    context.Reject("malformed redirect uri");
                    return;
                }

                var validRedirectUrls = await _applicationManager.GetRedirectUrisAsync((await _applicationManager.FindByClientIdAsync(context.Request.ClientId!))!);

                bool requestUrlIsValid = validRedirectUrls.Contains($"{requestRedirectUrl.Scheme}://{requestRedirectUrl.OriginalString!.Split(".")[1]}");

                if (requestUrlIsValid)
                {
                    //check if tenant matches tenant in redirectUrl
                    if (!requestRedirectUrl!.Host.Contains(tenant))
                    {
                        context.Reject("invalid_request", OpenIddictResources.FormatID2043("redirect_uri"), OpenIddictResources.FormatID8000("ID2043"));
                    }

                    return;
                }

                context.Reject("invalid_request", OpenIddictResources.FormatID2043("redirect_uri"), OpenIddictResources.FormatID8000("ID2043"));
            }
            else if (achievePayrollClients.Contains(context.Request.ClientId) && context.Request.AcrValues is null)
            {
                context.Reject("acr_values parameter must contain tenant");
            }
            else
            {
                var validRedirectUrls = await _applicationManager.GetRedirectUrisAsync((await _applicationManager.FindByClientIdAsync(context.Request.ClientId!))!);
                bool requestUrlIsValid = validRedirectUrls.Contains(context.RedirectUri!);

                if (!requestUrlIsValid)
                {
                    context.Reject("invalid_request", OpenIddictResources.FormatID2043("redirect_uri"), OpenIddictResources.FormatID8000("ID2043"));
                }
            }
        }
    }
}
