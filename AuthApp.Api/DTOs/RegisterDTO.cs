using System.ComponentModel.DataAnnotations;

namespace AuthApp.Api.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
