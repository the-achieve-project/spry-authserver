using Spry.AuthServer.Data;
using Spry.AuthServer.Models;
using Spry.AuthServer.SeedWork;

namespace Spry.AuthServer.Infrastructure.Identity
{
    public static class AuthConfiguration
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(options =>
            {
                var googleAuthOptions = builder.Configuration.GetSection(GoogleAuthenticationOptions.GoogleAuthentication).Get<GoogleAuthenticationOptions>()!;

                options.ClientId = googleAuthOptions.Client_Id;
                options.ClientSecret = googleAuthOptions.Client_Secret;
            })
            .AddMicrosoftAccount(options =>
            {
                var microsoftAuth = builder.Configuration.GetSection(MicrosoftAuthOptions.MicrosoftAuthentication).Get<MicrosoftAuthOptions>()!;

                options.ClientId = microsoftAuth.ClientId;
                options.ClientSecret = microsoftAuth.ClientSecret;
            })
            .AddYahoo(options =>
            {
                var yahooAuth = builder.Configuration.GetSection(YahooAuthOptions.YahooAuthentication).Get<YahooAuthOptions>()!;

                options.ClientId = yahooAuth.ClientId;
                options.ClientSecret = yahooAuth.ClientSecret;
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
