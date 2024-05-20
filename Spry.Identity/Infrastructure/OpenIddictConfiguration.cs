using Spry.Identity.Data;
using System.Security.Cryptography.X509Certificates;
using Spry.Identity.SeedWork;

namespace Spry.Identity.Infrastructure
{
    public static class OpenIddictConfiguration
    {
        public static IServiceCollection AddOpenIddictConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var serverSettings = builder.Configuration.GetSection(IdentityServerSettings.Settings).Get<IdentityServerSettings>()!;

            services.AddOpenIddict()
                  .AddValidation(options =>
                  {
                      options.AddAudiences(serverSettings.Audiences);
                      //options.EnableTokenEntryValidation();
                  })
                  .AddCore(options =>
                  {
                      options.UseEntityFrameworkCore()
                              .UseDbContext<IdentityDataContext>()
                              .ReplaceDefaultEntities<Guid>();

                      options.UseQuartz();                      
                  })
                  .AddServer(options =>
                  {
                      options.SetIssuer(serverSettings.Issuer);
                      options.ServerEventHandlers(builder.Configuration);

                      options.AllowClientCredentialsFlow()
                             .AllowAuthorizationCodeFlow()
                             .RequireProofKeyForCodeExchange()
                             .AllowRefreshTokenFlow();

                      options.SetAuthorizationEndpointUris("/connect/authorize")
                             .SetTokenEndpointUris("/connect/token")
                             .SetUserinfoEndpointUris("/connect/userinfo")
                             .SetIntrospectionEndpointUris("/connect/introspect")
                             .SetLogoutEndpointUris("/connect/endsession")
                             .SetRevocationEndpointUris("/connect/revocation");

                      if (builder.Environment.IsDevelopment())
                      {
                          var cert = new X509Certificate2("everwage.key.dev.pfx", serverSettings.CertificatePasswordProd);
                          options.AddSigningCertificate(cert)
                                 .AddEncryptionCertificate(cert);
                      }
                      else if (builder.Environment.IsProduction())
                      {
                          var cert = new X509Certificate2("everwage.key.prod.pfx", serverSettings.CertificatePasswordProd
                                   //it is important to use X509KeyStorageFlags.EphemeralKeySet to avoid 
                                   //Internal.Cryptography.CryptoThrowHelper+WindowsCryptographicException: The system cannot find the file specified.
                                   //keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet
                                   );
                          options.AddSigningCertificate(cert)
                                 .AddEncryptionCertificate(cert);
                      }

                      options.DisableAccessTokenEncryption();
                      //.RegisterScopes("api", "profile");

                      options.UseAspNetCore()
                              .EnableTokenEndpointPassthrough()
                              .EnableAuthorizationEndpointPassthrough()
                              .EnableLogoutEndpointPassthrough()
                              .EnableUserinfoEndpointPassthrough();

                      if (builder.Environment.IsDevelopment())
                          options.UseAspNetCore()
                                 .DisableTransportSecurityRequirement();
                  });

            return services;
        }
    }
}
