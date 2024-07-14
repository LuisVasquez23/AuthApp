using System.ComponentModel.DataAnnotations;

namespace AuthApp.Api.DTOs
{
    public class ResetPasswordDTO
    {
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match")]
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
