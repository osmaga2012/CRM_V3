namespace CRM.Dtos;

public class CofradiasDto
{
    public long CodigoCofradia { get; set; }
    public string? NombreCofradia { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Web { get; set; }
    public DateTime? FechaAlta { get; set; }
    public bool? Activo { get; set; }
}
