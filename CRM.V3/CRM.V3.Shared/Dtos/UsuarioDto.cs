using System.ComponentModel.DataAnnotations;

namespace CRM.V3.Shared.Dtos
{
    public class UsuarioDto
    {
        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        [EmailAddress]
        public string EMail { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string Rol { get; set; } = "Cliente";
    }
}
