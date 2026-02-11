namespace CRM.Dtos;

public class BarcosDto
{
    public long CodigoBarco { get; set; }
    public string? Censo { get; set; }
    public string? NombreB { get; set; }
    public string? NombreA { get; set; }
    public string? CapitanNombre { get; set; }
    public string? TipoBarco { get; set; }
    public string? Puerto { get; set; }
    public decimal? Eslora { get; set; }
    public decimal? Manga { get; set; }
    public decimal? Puntal { get; set; }
    public int? TRB { get; set; }
    public int? GT { get; set; }
    public string? Matricula { get; set; }
    public string? LicenciaPesca { get; set; }
    public DateTime? FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public bool? Activo { get; set; }
    
    // Navigation properties
    public ICollection<BarcosTramitesDto>? BarcosTramites { get; set; }
}
