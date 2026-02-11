namespace CRM.Dtos;

public class UsuarioDto
{
    public long IdUsuario { get; set; }
    public long Id { get; set; } // Alias for IdUsuario
    public string? Email { get; set; }
    public string? EMail { get; set; } // Alias for Email
    public string? Password { get; set; }
    public string? PasswordHash { get; set; } // Alias for Password
    public string? Nombre { get; set; }
    public string? NombreUsuario { get; set; } // Username
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Rol { get; set; }
    public string? CodigoEmpresa { get; set; }
    public long? CodigoPersona { get; set; }
    public string? NIFAcceso { get; set; } // NIF for access
    public string? EMailAvisos { get; set; } // Email for notifications
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public DateTime? FechaRegistro { get; set; } // Registration date
    public bool? Activo { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    
    // Navigation properties
    public EmpresasDto? Empresa { get; set; }
    public PersonasDto? Persona { get; set; }
}
