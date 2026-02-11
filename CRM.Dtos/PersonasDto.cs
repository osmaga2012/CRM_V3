namespace CRM.Dtos;

public class PersonasDto
{
    public long CodigoPersona { get; set; }
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? NifNie { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public bool? Activo { get; set; }
}
