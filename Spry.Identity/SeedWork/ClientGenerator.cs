using OpenIddict.Abstractions;

namespace Spry.Identity.SeedWork
{
    public static class ClientGenerator
    {
        public static async Task GenerateClients(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var manager2 = serviceProvider.GetRequiredService<IOpenIddictTokenManager>();

            if (await manager.FindByClientIdAsync(ClientIds.M2M, cancellationToken) is null)
            {
                await manager.CreateAsync(ClientApplications.M2m, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.AchieveApp, cancellationToken) is null)
            {
                await manager.CreateAsync(ClientApplications.AcheiveApp, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryAdmin, cancellationToken) is null)
            {
                await manager.CreateAsync(ClientApplications.AdminApp, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryEss, cancellationToken) is null)
            {
                await manager.CreateAsync(ClientApplications.EssApp, cancellationToken);
            }

            if (await manager.FindByClientIdAsync(ClientIds.SpryIdsrv4, cancellationToken) is null)
            {
                await manager.CreateAsync(ClientApplications.Idsr4App, cancellationToken);
            }
        }
    }
}
