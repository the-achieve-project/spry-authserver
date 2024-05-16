using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using Spry.Identity.Utility;
using System.Text.Json;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Spry.Identity.SeedWork
{
    public static class ClientGenerator
    {
        public static async Task GenerateClients(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

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
