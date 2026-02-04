namespace CRM.V3.Shared.Dtos
{
    public class DashboardResumenDto
    {
        public int TotalEmpresas { get; set; }
        public int TotalPersonas { get; set; }
        public int TotalTramitesAbiertos { get; set; }
        public int TotalDocumentosPendientes { get; set; }
        public double PorcentajeTramitesFinalizados { get; set; }
        public List<TramiteResumenDto> UltimosTramites { get; set; } = new();
        public List<DocumentoResumenDto> UltimosDocumentos { get; set; } = new();
    }
    public class TramiteResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class DocumentoResumenDto
    {
        public int Id { get; set; }
        public string NombreDocumento { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; }
    }
}
