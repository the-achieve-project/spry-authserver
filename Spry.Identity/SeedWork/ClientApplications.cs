using OpenIddict.Abstractions;
using Spry.Identity.Utility;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Spry.Identity.SeedWork
{
    public static class ClientApplications
    {
        static readonly ClientDto[] _clientConfiguration;
        static ClientApplications()
        {
            var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), $"SeedWork/files/clients{"." + AppVariables.CurrentEnvironment}.json"));
            _clientConfiguration = JsonSerializer.Deserialize<ClientDto[]>(config, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public static OpenIddictApplicationDescriptor M2m
        {
            get
            {
                return new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.M2M,
                    ClientSecret = _clientConfiguration.GetClient(ClientIds.M2M).ClientSecret,
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
                };
            }
        }

        public static OpenIddictApplicationDescriptor AcheiveApp
        {
            get
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.AchieveApp,
                    ClientType = ClientTypes.Public,
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Logout,

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
                };

                foreach (var uri in _clientConfiguration.GetRedirectUris(ClientIds.AchieveApp))
                {
                    descriptor.RedirectUris.Add(uri);
                }

                foreach (var uri in _clientConfiguration.GetPostLogoutRedirectUris(ClientIds.AchieveApp))
                {
                    descriptor.PostLogoutRedirectUris.Add(uri);
                }

                return descriptor;
            }
        }


        public static OpenIddictApplicationDescriptor AdminApp
        {
            get
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryAdmin,
                    ClientType = ClientTypes.Public,
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Logout,

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
                };

                foreach (var uri in _clientConfiguration.GetRedirectUris(ClientIds.SpryAdmin))
                {
                    descriptor.RedirectUris.Add(uri);
                }


                foreach (var uri in _clientConfiguration.GetPostLogoutRedirectUris(ClientIds.SpryAdmin))
                {
                    descriptor.PostLogoutRedirectUris.Add(uri);
                }
                return descriptor;
            }
        }

        public static OpenIddictApplicationDescriptor EssApp
        {
            get
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryEss,
                    ClientType = ClientTypes.Public,
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Logout,

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
                };

                foreach (var uri in _clientConfiguration.GetRedirectUris(ClientIds.SpryEss))
                {
                    descriptor.RedirectUris.Add(uri);
                }

                foreach (var uri in _clientConfiguration.GetPostLogoutRedirectUris(ClientIds.SpryEss))
                {
                    descriptor.PostLogoutRedirectUris.Add(uri);
                }

                return descriptor;
            }
        }


        public static OpenIddictApplicationDescriptor Idsr4App
        {
            get
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = ClientIds.SpryIdsrv4,
                    ClientType = ClientTypes.Public,
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Logout,

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
                };

                foreach (var uri in _clientConfiguration.GetRedirectUris(ClientIds.SpryIdsrv4))
                {
                    descriptor.RedirectUris.Add(uri);
                }

                foreach (var uri in _clientConfiguration.GetPostLogoutRedirectUris(ClientIds.SpryIdsrv4))
                {
                    descriptor.PostLogoutRedirectUris.Add(uri);
                }

                return descriptor;
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

#nullable disable
    public class ClientDto
    {
        public string Id { get; set; }
        public string[] RedirectUris { get; set; }
        public string[] PostLogoutRedirectUris { get; set; }
        public string ClientSecret { get; set; }

        public HashSet<Uri> GetRedirectUris()
        {
            return RedirectUris.Select(r => new Uri(r)).ToHashSet();
        }

        public HashSet<Uri> GetPostLogoutRedirectUris()
        {
            return PostLogoutRedirectUris.Select(r => new Uri(r)).ToHashSet();
        }
    }

    public static class ClientStoreExtensions
    {
        public static ClientDto GetClient(this ClientDto[] clients, string clientId)
        {
            return clients.First(c => c.Id == clientId);
        }

        public static HashSet<Uri> GetRedirectUris(this ClientDto[] clients, string clientId)
        {
            return clients.First(c => c.Id == clientId).GetRedirectUris();
        }

        public static HashSet<Uri> GetPostLogoutRedirectUris(this ClientDto[] clients, string clientId)
        {
            return clients.First(c => c.Id == clientId).GetPostLogoutRedirectUris();
        }
    }
}
