using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System;
using Spry.Identity.Data;

namespace Spry.Identity.Workers
{
    public class ClientStore : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientStore(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("achieve_app", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "achieve_app",
                    ClientSecret = "achieve_app_secret",
                    DisplayName = "achieve-app",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,

                        Permissions.Prefixes.Scope + "api"
                    }
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync("spry_admin", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "spry_admin", 
                    //ClientSecret = "postman-secret-spa",
                    ClientType = ClientTypes.Public,
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") ,
                                    new Uri("https://jwt.ms")},
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,

                        //Permissions.GrantTypes.RefreshToken,

                        Permissions.ResponseTypes.Code,

                        //Permissions.Scopes.Email,
                        //Permissions.Scopes.Profile,
                        //Permissions.Scopes.Roles,

                        Permissions.Prefixes.Scope + "api",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
