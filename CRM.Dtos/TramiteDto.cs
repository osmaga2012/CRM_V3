namespace CRM.Dtos;

public class TramiteDto
{
    public long IdTramite { get; set; }
    public string? CodigoTramite { get; set; }
    public string? NombreTramite { get; set; }
    public string? DescripcionTramite { get; set; }
    public string? TipoTramite { get; set; }
    public string? Estado { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime? FechaAviso { get; set; }
    public int? DiasAviso { get; set; }
    public string? Observaciones { get; set; }
    public bool? Activo { get; set; }
}
