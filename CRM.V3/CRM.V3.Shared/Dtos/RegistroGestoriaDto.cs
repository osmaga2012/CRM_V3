namespace CRM.V3.Shared.Dtos
{
    public class RegistroGestoriaDto
    {
        // Datos empresa
        public string NombreComercial { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string NIF { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string CP { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string EmailEmpresa { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;

        // Usuario administrador
        public string NombreUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
