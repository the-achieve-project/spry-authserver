using Spry.Identity.Data;
using Spry.Identity.Models;
using Spry.Identity.SeedWork;
using System.Configuration;

namespace Spry.Identity.Infrastructure.Identity
{
    public static class AuthConfiguration
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(options =>
            {
                var googleAuthOptions = builder.Configuration.GetSection("GoogleAuthentication").Get<GoogleAuthenticationOptions>()!;

                options.ClientId = googleAuthOptions.Client_Id;
                options.ClientSecret = googleAuthOptions.Client_Secret;
            })
            .AddMicrosoftAccount(options =>
            {
                var microsoftAuth = builder.Configuration.GetSection("MicrosoftAuthentication").Get<MicrosoftAuthOptions>()!;

                options.ClientId = microsoftAuth.ClientId;
                options.ClientSecret = microsoftAuth.ClientSecret;
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
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            });

            return services;
        }
    }
}
