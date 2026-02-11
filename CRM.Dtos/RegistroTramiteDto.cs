namespace CRM.Dtos;

public class RegistroTramiteDto
{
    public long IdRegistro { get; set; }
    public long? IdTramite { get; set; }
    public string? AccionRealizada { get; set; }
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public DateTime? FechaAccion { get; set; }
    public long? IdUsuario { get; set; }
    public string? Observaciones { get; set; }
    
    public TramiteDto? Tramite { get; set; }
    public UsuarioDto? Usuario { get; set; }
}
