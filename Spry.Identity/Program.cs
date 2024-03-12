using Microsoft.EntityFrameworkCore;
using Spry.Identity.Data;
using Spry.Identity.Infrastructure;
using Spry.Identity.Models;
using Spry.Identity.SeedWork;
using Spry.Identity.Services;
using System;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Server.OpenIddictServerHandlers.Authentication;


namespace Spry.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            var serverSettings = builder.Configuration.GetSection(IdentityServerSettings.Settings).Get<IdentityServerSettings>()!;
            builder.Services.Configure<IdentityServerSettings>(builder.Configuration.GetSection("IdentityServer"));

            //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.AddDbContext<IdentityDataContext>(options =>
            {
                Console.WriteLine("connection: {0}", builder.Configuration.GetConnectionString("SprySSOIdentity"));
                options.UseNpgsql(builder.Configuration.GetConnectionString("SprySSOIdentity"),
                        npgsqlOptionsAction: sqlOptions =>
                        {
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), []);
                        });

                options.UseOpenIddict<Guid>();
            });

            builder.Services.AddIdentity<User, UserRole>()
                           .AddEntityFrameworkStores<IdentityDataContext>()
                           .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(90); //subject to change
                options.SlidingExpiration = true;
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            });

            builder.Services.AddOpenIddict()
                   .AddValidation(options => options.AddAudiences(serverSettings.Audiences))
                   .AddCore(options =>
                   {
                       options.UseEntityFrameworkCore()
                               .UseDbContext<IdentityDataContext>()
                               .ReplaceDefaultEntities<Guid>();

                       //options.UseQuartz();                      
                   })
                   .AddServer(options =>
                   {
                       //options.EnableDegradedMode();
                       //options.DisableTokenStorage(); //for dev

                       options.RemoveEventHandler(ValidateClientRedirectUri.Descriptor);

                       //options.AddEventHandler(CustomValidateClientRedirectUri.Descriptor);

                       options.ServerEventHandlers();

                       options.AllowClientCredentialsFlow()
                              .AllowAuthorizationCodeFlow()
                              .RequireProofKeyForCodeExchange()
                              .AllowRefreshTokenFlow();

                       options.SetAuthorizationEndpointUris("/connect/authorize")
                               .SetTokenEndpointUris("/connect/token")
                               .SetUserinfoEndpointUris("/connect/userinfo")
                               .SetIntrospectionEndpointUris("/connect/introspect");

                       if (builder.Environment.IsDevelopment())
                       {
                           options.AddDevelopmentEncryptionCertificate()
                                   .AddDevelopmentSigningCertificate();
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
                               .EnableAuthorizationEndpointPassthrough();

                       if (builder.Environment.IsDevelopment())
                           options.UseAspNetCore()
                                  .DisableTransportSecurityRequirement();
                   });

            builder.Services.AddScoped<AccountService>();
            builder.Services.AddHostedService<Workers.Seeder>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

                logger!.LogInformation("Migrating IdentityDataContext start");
                var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();

                db.Database.Migrate();
                logger!.LogInformation("Migrating IdentityDataContext end");
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}
