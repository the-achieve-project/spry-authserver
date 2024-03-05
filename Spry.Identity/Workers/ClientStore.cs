using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
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

            await CreateApplicationsAsync();
            await CreateScopesAsync();

            async Task CreateApplicationsAsync()
            {
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
                        ClientType = ClientTypes.Public,
                        RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") ,
                                    //new Uri("https://jwt.ms")
                    },
                        Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,

                        //Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        //Permissions.Scopes.Roles,

                        Permissions.Prefixes.Scope + "api",
                    },
                        Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                    }, cancellationToken);
                }

                if (await manager.FindByClientIdAsync("resource_server_1", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "resource_server_1",
                        ClientSecret = "846B62D0-DEF9-4215-A99D-86E6B8DAB342",
                        Permissions =
                {
                    Permissions.Endpoints.Introspection
                }
                    }, cancellationToken);
                }
            }


            async Task CreateScopesAsync()
            {
                var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("api1", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "api1",
                        Resources =
                        {
                            "resource_server_1"
                        }
                    }, cancellationToken);
                }

                if (await manager.FindByNameAsync("api2", cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = "api2",
                        Resources =
                        {
                            "resource_server_2"
                        }
                    }, cancellationToken);
                }
            }

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
