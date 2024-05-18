using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Spry.Identity.Data;
using Spry.Identity.Infrastructure;
using Spry.Identity.Infrastructure.Identity;
using Spry.Identity.SeedWork;

namespace Spry.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            //builder.WebHost.UseStaticWebAssets();
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<IdentityDataContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("SprySSOIdentity"),
                        npgsqlOptionsAction: sqlOptions =>
                        {
                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                            sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), []);
                        });

                options.UseOpenIddict<Guid>();
            });

            builder.Services.AddAuthenticationConfiguration(builder);
            builder.Services.AddOpenIddictConfiguration(builder);
            builder.Services.AddEventBusConfiguration(builder.Configuration);
            builder.Services.AddApplicationServiceConfiguration(builder);

            var app = builder.Build();

            app.UseMigrations();

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

            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //    Secure = CookieSecurePolicy.Always
            //});

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}
