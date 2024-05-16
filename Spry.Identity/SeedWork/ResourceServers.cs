using OpenIddict.Abstractions;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Spry.Identity.SeedWork
{
#nullable disable
    public static class ResourceServers
    {
        public static async Task GenerateResourceServers(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "SeedWork/files/resources.json"));

            var resources = JsonSerializer.Deserialize<ApiResource[]>(config);

            foreach (var item in resources)
            {
                if (await manager.FindByClientIdAsync(item.Name, cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = item.Name,
                        ClientSecret = item.ClientSecret,
                        Permissions =
                        {
                            Permissions.Endpoints.Introspection
                        }
                    }, cancellationToken);
                }
            }
        }
    }

    public class ApiResource
    {
        public string Name { get; set; }
        public string ClientSecret { get; set; }
    }
}


