using System.Text.Json.Serialization;
using CRM.Dtos.Converters;

namespace CRM.Dtos;

public class BarcosTramitesDto
{
    public long Id { get; set; }
    public long CodigoBarco { get; set; }
    public string? CodigoEmpresa { get; set; }
    public string? Certificado { get; set; }
    public string? TipoTramite { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime? FechaAviso { get; set; }
    public int? DiasAvisoTramite { get; set; }
    public string? ListaEmailsEnvioAviso { get; set; }
    
    [JsonConverter(typeof(NumberToStringConverter))]
    public string? CensoBarco { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? Estado { get; set; }
    public string? Observaciones { get; set; }
    public string? DocumentoPath { get; set; }
    public bool? Activo { get; set; }
    
    // Parser properties for date handling
    public DateOnly? FechaInicioParser => FechaInicio.HasValue ? DateOnly.FromDateTime(FechaInicio.Value) : null;
    public DateOnly? FechaFinParser => FechaFin.HasValue ? DateOnly.FromDateTime(FechaFin.Value) : null;
    public DateOnly? FechaAvisoParser => FechaAviso.HasValue ? DateOnly.FromDateTime(FechaAviso.Value) : null;
    
    // Navigation properties
    public BarcosDto? Barco { get; set; }
    public EmpresasDto? Empresa { get; set; }
}
