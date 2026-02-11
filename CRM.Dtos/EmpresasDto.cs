namespace CRM.Dtos;

public class EmpresasDto
{
    public string? CodigoEmpresa { get; set; }
    public long CodigoBarco { get; set; }
    public string? NombreArmador { get; set; }
    public string? CifNif { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? CodigoBanco { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public bool? Activo { get; set; }
    
    // Navigation properties
    public BarcosDto? Barco { get; set; }
}
