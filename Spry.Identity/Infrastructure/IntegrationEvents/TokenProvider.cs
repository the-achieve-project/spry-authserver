using Spry.AuthServer.Models;

namespace Spry.AuthServer.Infrastructure.IntegrationEvents
{
    public class TokenProvider : IUserTwoFactorTokenProvider<User>
    {
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<User> manager, User user)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateAsync(string purpose, UserManager<User> manager, User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<User> manager, User user)
        {
            throw new NotImplementedException();
        }
    }
}
