namespace CRM.V3.Shared.Dtos
{
    public class ActividadRecienteDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Trámite", "Documento", etc.
        public DateTime Fecha { get; set; }
    }
}
