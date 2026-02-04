using System.ComponentModel.DataAnnotations;

namespace CRM.V3.Shared.Dtos
{
    public class Usuario2Dto
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string EMail { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
        public string Rol { get; set; } = "Cliente";
        public Guid Id { get; set; } // Este ID debe coincidir con el de auth.users
        public int? EmpresaId { get; set; }
        public int? PersonaId { get; set; }
        public bool Activo { get; set; } = true;
        public int TipoUsuario { get; set; } // Enum en el backend
    }
}
