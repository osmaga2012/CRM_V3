namespace CRM.Dtos;

public class RequisitosTramiteDto
{
    public long IdRequisito { get; set; }
    public long? IdTipoTramite { get; set; }
    public string? NombreRequisito { get; set; }
    public string? DescripcionRequisito { get; set; }
    public bool? Obligatorio { get; set; }
    public int? Orden { get; set; }
    public bool? Activo { get; set; }
    
    public TipoTramiteDto? TipoTramite { get; set; }
}
