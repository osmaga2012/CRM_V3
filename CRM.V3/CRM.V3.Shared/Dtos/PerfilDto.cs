namespace CRM.V3.Shared.Dtos
{
    public class PerfilDto
    {
        public bool PerfilCreado { get; set; }
        public string TipoUsuario { get; set; } = "Persona"; // Valores esperados: "Persona" o "Empresa"
    }
}
