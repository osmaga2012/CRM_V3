using System.ComponentModel.DataAnnotations;

namespace CRM.V3.Shared.Dtos
{
    public class EmpresaDto
    {
        [Required]
        public string NIF { get; set; }

        [Required]
        public string NombreComercial { get; set; }

        [Required]
        public string RazonSocial { get; set; }

        public string? Calle { get; set; }
        public string? Numero { get; set; }
        public string? CP { get; set; }
        public string? Municipio { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Telefono { get; set; }
        public string? NombrePersonaContacto { get; set; }

        public string? IBAN { get; set; }
        public bool? CotizaIltObrera { get; set; }
    }
}
