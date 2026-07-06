using System.ComponentModel.DataAnnotations;

namespace KeyStone_Identity.Core.DTOs
{
    public class LoginDTO
    {
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
