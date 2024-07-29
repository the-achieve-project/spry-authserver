using System.ComponentModel.DataAnnotations;

namespace Spry.AuthServer.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
