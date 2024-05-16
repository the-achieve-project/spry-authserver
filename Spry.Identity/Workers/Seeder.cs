using Spry.Identity.Data;
using Spry.Identity.SeedWork;

namespace Spry.Identity.Workers
{
    public class Seeder(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            await ClientGenerator.GenerateClients(scope.ServiceProvider, cancellationToken);
            await ResourceServers.GenerateResourceServers(scope.ServiceProvider, cancellationToken);  
            await Scopes.GenerateScopes(scope.ServiceProvider, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
