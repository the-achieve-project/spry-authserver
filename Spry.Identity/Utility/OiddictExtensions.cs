using OpenIddict.Abstractions;
using Spry.Identity.Models;

namespace Spry.Identity.Utility
{
    public static class OiddictExtensions
    {
        public static async Task RevokeTokensAsync(this IOpenIddictTokenManager tokenManager, string sub)
        {
            await foreach (var token in tokenManager.FindBySubjectAsync(sub))
            {
                _ = await tokenManager.TryRevokeAsync(token);
            }
        }
    }
}
