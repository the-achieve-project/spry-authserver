using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Spry.Identity.SeedWork
{
    public static class ClientStore
    {
        public static async Task GenerateClients(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("m2m", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "m2m",
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

            if (await manager.FindByClientIdAsync("achieve_app", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "achieve_app",
                    ClientType = ClientTypes.Public,
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") ,
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
            
            if (await manager.FindByClientIdAsync("spry.admin", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "spry.admin",
                    ClientType = ClientTypes.Public,
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") ,
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
            
            if (await manager.FindByClientIdAsync("spry.ess", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "spry.ess",
                    ClientType = ClientTypes.Public,
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") ,
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
        public const string SpryWeb = "spry.admin";
        public const string AchieveApp = "achieve_app";
        public const string M2M = "m2m";
    }
}
