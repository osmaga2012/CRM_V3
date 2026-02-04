namespace CRM.V3.Shared.Dtos
{
    public class UltimaActividadDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
    }
}
