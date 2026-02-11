namespace CRM.Dtos;

public class ConfiguracionDto
{
    public long IdConfiguracion { get; set; }
    public string? Clave { get; set; }
    public string? Valor { get; set; }
    public string? Descripcion { get; set; }
    public string? TipoDato { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool? Activo { get; set; }
}
