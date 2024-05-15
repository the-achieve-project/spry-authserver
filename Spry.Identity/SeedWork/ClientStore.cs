using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Spry.Identity.SeedWork
{
    public static class ClientStore
    {
        public static async Task GenerateClients(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync(ClientIds.M2M, cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.M2M,
                    ClientSecret = "946B62D0-DEF9-3215-A99D-46E6B8DAB342",
                    DisplayName = "m2m",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.Prefixes.Scope + "spry.pim",
                        Permissions.Prefixes.Scope + "spry.time",
                        Permissions.Prefixes.Scope + "spry.messaging",
                        Permissions.Prefixes.Scope + "spry.payroll",
                        Permissions.Prefixes.Scope + "spry.id",
                        Permissions.Prefixes.Scope + "spry.company",
                    }
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.AchieveApp, cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.AchieveApp,
                    ClientType = ClientTypes.Public,
                    RedirectUris = 
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback"),
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,

                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryAdmin, cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryAdmin,
                    ClientType = ClientTypes.Public,
                    RedirectUris =
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback"),
                        new Uri("http://localhost:5200/signin-oidc2"),
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,

                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,

                        Permissions.Prefixes.Scope + "spry.pim",
                        Permissions.Prefixes.Scope + "spry.time",
                        Permissions.Prefixes.Scope + "spry.messaging",
                        Permissions.Prefixes.Scope + "spry.payroll",
                        Permissions.Prefixes.Scope + "spry.id",
                        Permissions.Prefixes.Scope + "spry.company",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryEss, cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryEss,
                    ClientType = ClientTypes.Public,
                    RedirectUris =
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback"),
                        new Uri("http://localhost:5201/signin-oidc2"),
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,

                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,

                        Permissions.Prefixes.Scope + "spry.pim",
                        Permissions.Prefixes.Scope + "spry.time",
                        Permissions.Prefixes.Scope + "spry.messaging",
                        Permissions.Prefixes.Scope + "spry.payroll",
                        Permissions.Prefixes.Scope + "spry.id",
                        Permissions.Prefixes.Scope + "spry.company",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                }, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryIdsrv4, cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryIdsrv4,
                    ClientType = ClientTypes.Public,
                    RedirectUris =
                    {
                        new Uri("https://oauth.pstmn.io/v1/callback"),
                        new Uri("http://localhost:5101/signin-oidc2")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,

                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.ClientCredentials,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,

                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,

                        Permissions.Prefixes.Scope + "spry.pim",
                        Permissions.Prefixes.Scope + "spry.time",
                        Permissions.Prefixes.Scope + "spry.messaging",
                        Permissions.Prefixes.Scope + "spry.payroll",
                        Permissions.Prefixes.Scope + "spry.id",
                        Permissions.Prefixes.Scope + "spry.company",
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange,
                    },
                }, cancellationToken);
            }
        }
    }

    public static class ClientIds
    {
        public const string SpryEss = "spry.ess";
        public const string SpryAdmin = "spry.admin";
        public const string SpryIdsrv4 = "spry.idsrv4";
        public const string AchieveApp = "achieve_app";
        public const string M2M = "m2m";
    }
}
