namespace CRM.Dtos;

public class DocumentoDto
{
    public long IdDocumento { get; set; }
    public string? NombreDocumento { get; set; }
    public string? TipoDocumento { get; set; }
    public string? RutaDocumento { get; set; }
    public string? Extension { get; set; }
    public long? TamanoBytes { get; set; }
    public DateTime? FechaSubida { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public long? IdUsuarioSubida { get; set; }
    public long? IdTramite { get; set; }
    public string? Observaciones { get; set; }
    public bool? Activo { get; set; }
    
    public UsuarioDto? Usuario { get; set; }
    public TramiteDto? Tramite { get; set; }
}
