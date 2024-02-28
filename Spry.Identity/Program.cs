using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spry.Identity.Data;
using Spry.Identity.Infrastructure;
using Spry.Identity.Models;
using Spry.Identity.Services;
using Spry.Identity.Workers;
using static OpenIddict.Server.OpenIddictServerEvents;
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

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                            {
                                options.LoginPath = "/account/login";
                            });

            builder.Services.AddDbContext<IdentityDataContext>(options => {
                options.UseNpgsql(builder.Configuration.GetConnectionString("Spry_SSO_Identity"),
                        npgsqlOptionsAction: sqlOptions => {
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), []);
                        });

                options.UseOpenIddict<Guid>();
            });

            builder.Services.AddIdentity<User, UserRole>()
                           .AddEntityFrameworkStores<IdentityDataContext>()
                           .AddDefaultTokenProviders();

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
                       options.RemoveEventHandler(ValidateClientRedirectUri.Descriptor);

                       options.ServerEventHandlers();

                       options.AllowClientCredentialsFlow()
                              .AllowAuthorizationCodeFlow()
                              .RequireProofKeyForCodeExchange()
                              .AllowRefreshTokenFlow();

                       options.SetAuthorizationEndpointUris("/connect/authorize")
                               .SetTokenEndpointUris("/connect/token")
                               .SetUserinfoEndpointUris("/connect/userinfo")
                               .AddEphemeralEncryptionKey()
                               .AddEphemeralSigningKey()
                               .DisableAccessTokenEncryption()
                               .RegisterScopes("api");

                       options.UseAspNetCore()
                               .EnableTokenEndpointPassthrough()
                               .EnableAuthorizationEndpointPassthrough();
                   });

            builder.Services.AddScoped<AccountService>();
            builder.Services.AddHostedService<ClientStore>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
