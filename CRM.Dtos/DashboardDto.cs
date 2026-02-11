namespace CRM.Dtos;

public class DashboardDto
{
    public int? TotalBarcos { get; set; }
    public int? TotalEmpresas { get; set; }
    public int? TotalTramites { get; set; }
    public int? TotalUsuarios { get; set; }
    public int? TramitesVigentes { get; set; }
    public int? TramitesPorVencer { get; set; }
    public int? TramitesVencidos { get; set; }
    public int? DocumentosPendientes { get; set; }
    public List<object>? TramitesRecientes { get; set; }
    public List<object>? AvisosRecientes { get; set; }
}
