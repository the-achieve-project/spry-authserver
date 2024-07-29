using OpenIddict.Abstractions;
using System.Text.Json;

namespace Spry.AuthServer.SeedWork
{
#nullable disable
    public static class Scopes
    {
        public static async Task GenerateScopes(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var config = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "SeedWork/files/scopes.json"));

            var scopes = JsonSerializer.Deserialize<ApiResource[]>(config);

            foreach (var item in scopes)
            {
                if (await manager.FindByNameAsync(item.Name, cancellationToken) is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = item.Name,
                    
                    }, cancellationToken);
                }
            }
        }
    }
}
