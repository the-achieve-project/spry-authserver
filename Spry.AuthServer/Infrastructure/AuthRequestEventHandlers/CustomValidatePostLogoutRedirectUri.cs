using OpenIddict.Abstractions;
using OpenIddict.Server;
using System.Diagnostics.CodeAnalysis;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;

namespace Spry.AuthServer.Infrastructure.AuthRequestEventHandlers
{
    public sealed class CustomValidatePostLogoutRedirectUri : IOpenIddictServerHandler<OpenIddictServerEvents.ValidateLogoutRequestContext>
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        //
        // Summary:
        //     Gets the default descriptor definition assigned to this handler.
        public static OpenIddictServerHandlerDescriptor Descriptor { get; } = OpenIddictServerHandlerDescriptor.CreateBuilder<OpenIddictServerEvents.ValidateLogoutRequestContext>().AddFilter<OpenIddictServerHandlerFilters.RequireDegradedModeDisabled>().AddFilter<OpenIddictServerHandlerFilters.RequirePostLogoutRedirectUriParameter>()
            .UseScopedHandler<CustomValidatePostLogoutRedirectUri>()
            .SetOrder(ValidateAuthentication.Descriptor.Order + 1000)
            .SetType(OpenIddictServerHandlerType.BuiltIn)
            .Build();


        public CustomValidatePostLogoutRedirectUri()
        {
            throw new InvalidOperationException(OpenIddictResources.GetResourceString("ID0016"));
        }

        public CustomValidatePostLogoutRedirectUri(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException("applicationManager");
        }

        public async ValueTask HandleAsync(OpenIddictServerEvents.ValidateLogoutRequestContext context)
        {
            OpenIddictServerEvents.ValidateLogoutRequestContext context2 = context;
            if (context2 == null)
            {
                throw new ArgumentNullException("context");
            }

            if (!string.IsNullOrEmpty(context2.ClientId))
            {
                object application = (await _applicationManager.FindByClientIdAsync(context2.ClientId)) ?? throw new InvalidOperationException(OpenIddictResources.GetResourceString("ID0032"));
                if (!(await _applicationManager.ValidatePostLogoutRedirectUriAsync(application, context2.PostLogoutRedirectUri)))
                {
                    context2.Logger.LogInformation(OpenIddictResources.GetResourceString("ID6128"), context2.PostLogoutRedirectUri);
                    context2.Reject("invalid_request", OpenIddictResources.FormatID2052("post_logout_redirect_uri"), OpenIddictResources.FormatID8000("ID2052"));
                }
            }
            else if (!(await ValidatePostLogoutRedirectUriAsync(context2.PostLogoutRedirectUri)))
            {
                context2.Logger.LogInformation(OpenIddictResources.GetResourceString("ID6128"), context2.PostLogoutRedirectUri);
                context2.Reject("invalid_request", OpenIddictResources.FormatID2052("post_logout_redirect_uri"), OpenIddictResources.FormatID8000("ID2052"));
            }

            async ValueTask<bool> ValidatePostLogoutRedirectUriAsync([StringSyntax("Uri")] string uri)
            {
                await foreach (object application2 in _applicationManager.FindByPostLogoutRedirectUriAsync(uri))
                {
                    bool flag = !context2.Options.IgnoreEndpointPermissions;
                    bool flag2 = flag;
                    if (flag2)
                    {
                        flag2 = !(await _applicationManager.HasPermissionAsync(application2, "ept:logout"));
                    }

                    if (!flag2 && await _applicationManager.ValidatePostLogoutRedirectUriAsync(application2, uri))
                    {
                        return true;
                    }
                }

                if (Uri.TryCreate(uri, UriKind.Absolute, out Uri result) && !result.IsDefaultPort && result.IsLoopback && (string.Equals(result.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || string.Equals(result.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
                {
                    await foreach (object application2 in _applicationManager.FindByPostLogoutRedirectUriAsync(new UriBuilder(result)
                    {
                        Port = -1
                    }.Uri.AbsoluteUri))
                    {
                        bool flag3 = !context2.Options.IgnoreEndpointPermissions;
                        bool flag4 = flag3;
                        if (flag4)
                        {
                            flag4 = !(await _applicationManager.HasPermissionAsync(application2, "ept:logout"));
                        }

                        if (!flag4)
                        {
                            bool flag5 = await _applicationManager.HasApplicationTypeAsync(application2, "native");
                            if (flag5)
                            {
                                flag5 = await _applicationManager.ValidatePostLogoutRedirectUriAsync(application2, uri);
                            }

                            if (flag5)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
