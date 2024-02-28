using Microsoft.AspNetCore.Identity;
using Spry.Identity.Dtos.Account;
using Spry.Identity.Models;
using Spry.Identity.SeedWork;

namespace Spry.Identity.Services
{
    public class AccountService
    {
        readonly UserManager<User> _userManager;
        public AccountService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
        {
            User user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            IdentityResult createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                return new ServiceResult(createResult.Errors.Select(err => err.Description).ToList());
            }

            return new ServiceResult();
        }

        public async Task<ServiceResult> ResetPasswordAsync(PasswordChange request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new ServiceResult("user doesnt exist");
            }

            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, request.Password);

            if (!result.Succeeded)
            {
                return new ServiceResult("password reset succeeded");
            }

            return new ServiceResult();
        }
    }
}
