using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Spry.Identity.Models;
using Spry.Identity.SeedWork;
using System.Security.Claims;

namespace Spry.Identity.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AuthorizationController(IOptions<IdentityServerSettings> options, SignInManager<User> signInManager) : Controller
    {
        private readonly IdentityServerSettings _idServerOptions = options.Value;

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal!;
            }
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal!;
            }

            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            if (request.ClientId == ClientIds.AchieveApp)
            {
                claimsPrincipal.SetAudiences(_idServerOptions.AchieveAudiences);
            }

            if (_idServerOptions.PayrollClients.Contains(request.ClientId))
            {
                claimsPrincipal.SetAudiences(_idServerOptions.Audiences);
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            var user = HttpContext.User;

            // If the user principal can't be extracted, redirect the user to the login page.
            if (!user.Identity!.IsAuthenticated)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? [.. Request.Form] : [.. Request.Query])
                    });
            }

            var dbUser = await signInManager.UserManager.FindByIdAsync(user.GetClaim(ClaimTypes.NameIdentifier)!);

            var claims = new List<Claim>
                    {
                        // 'subject' claim which is required
                        //new(OpenIddictConstants.Claims.Subject, user.GetClaim(ClaimTypes.NameIdentifier)!),
                        new(OpenIddictConstants.Claims.Subject, dbUser!.AchieveId!),
                        new Claim("src", "oiddict").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                        new Claim("is_migrated", "true").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                    };

            if (request.ClientId != ClientIds.AchieveApp)
            {
                if (!string.IsNullOrEmpty(request.AcrValues))
                {
                    claims.Add(new Claim("tenant", request.AcrValues.Split(':')[1]).SetDestinations(OpenIddictConstants.Destinations.AccessToken));
                }
                claims.Add(new Claim("user", user.GetClaim(ClaimTypes.Name)!).SetDestinations(OpenIddictConstants.Destinations.AccessToken));
            }

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Set requested scopes (this is not done automatically)
            claimsPrincipal.SetScopes(request.GetScopes());

            // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            User user = (await signInManager.UserManager.Users.FirstAsync(u => u.AchieveId == claimsPrincipal!.GetClaim("sub")!))!;

            return Ok(new UserInfo
            {
                Sub = user.AchieveId,
                UserName = user.UserName,
                FirstName = user.FirstName,
                OtherNames = user.OtherNames,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            });
        }

        [HttpGet("~/connect/endsession")]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
