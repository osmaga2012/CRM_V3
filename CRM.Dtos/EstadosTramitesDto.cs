namespace CRM.Dtos;

public class EstadosTramitesDto
{
    public long IdEstado { get; set; }
    public string? CodigoEstado { get; set; }
    public string? NombreEstado { get; set; }
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public int? Orden { get; set; }
    public bool? Activo { get; set; }
}
