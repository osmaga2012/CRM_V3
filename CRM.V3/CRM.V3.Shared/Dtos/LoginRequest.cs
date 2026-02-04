using System.ComponentModel.DataAnnotations;

namespace CRM.V3.Shared.Dtos
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

