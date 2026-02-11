namespace CRM.Dtos;

public class AvisosDto
{
    public long IdAviso { get; set; }
    public string? TituloAviso { get; set; }
    public string? MensajeAviso { get; set; }
    public string? TipoAviso { get; set; }
    public DateTime? FechaAviso { get; set; }
    public DateTime? FechaLectura { get; set; }
    public long? IdUsuario { get; set; }
    public string? EmailDestinatario { get; set; }
    public bool? Leido { get; set; }
    public bool? Activo { get; set; }
    
    public UsuarioDto? Usuario { get; set; }
}
