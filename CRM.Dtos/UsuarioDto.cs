namespace CRM.Dtos;

public class UsuarioDto
{
    public long IdUsuario { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Rol { get; set; }
    public string? CodigoEmpresa { get; set; }
    public long? CodigoPersona { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public bool? Activo { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    
    // Navigation properties
    public EmpresasDto? Empresa { get; set; }
    public PersonasDto? Persona { get; set; }
}
