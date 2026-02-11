namespace CRM.Dtos;

public class TipoTramiteDto
{
    public long IdTipoTramite { get; set; }
    public string? CodigoTipoTramite { get; set; }
    public string? NombreTipoTramite { get; set; }
    public string? Descripcion { get; set; }
    public int? DiasValidez { get; set; }
    public int? DiasAvisoDefecto { get; set; }
    public bool? RequiereDocumento { get; set; }
    public bool? Activo { get; set; }
}
