namespace CRM.Dtos;

public class TipoDocumentoDto
{
    public long IdTipoDocumento { get; set; }
    public string? CodigoTipoDocumento { get; set; }
    public string? NombreTipoDocumento { get; set; }
    public string? Descripcion { get; set; }
    public string? ExtensionesPermitidas { get; set; }
    public long? TamanoMaximoBytes { get; set; }
    public bool? Obligatorio { get; set; }
    public bool? Activo { get; set; }
}
