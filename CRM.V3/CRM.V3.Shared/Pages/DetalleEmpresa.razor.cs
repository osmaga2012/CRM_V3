using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CRM.V3.Shared.Pages
{
    public partial class DetalleEmpresa : ComponentBase
    {
        [Parameter]
        public string CodigoEmpresa { get; set; } = string.Empty;

        [Parameter]
        public string CodigoBarco { get; set; } = string.Empty;

        // Estado de carga
        private bool isLoading = true;
        private string tabActiva = "barcos";

        // Datos de la empresa
        private EmpresasDto? empresa;

        // Estadísticas
        private int totalBarcos = 0;
        private int totalTramites = 0;
        private int tramitesVigentes = 0;
        private int tramitesPorVencer = 0;
        private int tramitesVencidos = 0;

        // Colecciones
        private List<BarcosDto> barcosEmpresa = new();
        private List<UsuarioDto> usuariosEmpresa = new();

        // Formularios
        private bool mostrarFormBarco = false;
        private bool mostrarFormUsuario = false;
        private BarcosDto nuevoBarco = new();
        private UsuarioDto nuevoUsuario = new();
        private BarcosDto? barcoEditando = null;

        protected override async Task OnInitializedAsync()
        {
            await CargarDatosEmpresa();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(CodigoEmpresa))
            {
                await CargarDatosEmpresa();
            }
        }

        private async Task CargarDatosEmpresa()
        {
            try
            {
                isLoading = true;

                // Cargar empresa
                Dictionary<string, string> filtrosEmpresa = new Dictionary<string, string>
                {
                    { "CodigoEmpresa", CodigoEmpresa }
                };
                var empresasResult = await servicioEmpresas.GetAllAsync("api/Empresa", filtrosEmpresa, null);
                empresa = empresasResult?.FirstOrDefault(e => e.CodigoEmpresa == CodigoEmpresa);

                if (empresa == null)
                {
                    isLoading = false;
                    StateHasChanged();
                    return;
                }

                // Si hay un Código de Barco en la URL, cargar los datos de ese barco en particular
                if (!string.IsNullOrEmpty(CodigoBarco) && long.TryParse(CodigoBarco, out long codigoBarcoLong))
                {
                    // Cargar barco por CodigoBarco
                    var barcoResult = await servicioBarcos.GetByIdAsync("api/Barcos", codigoBarcoLong);
                    if (barcoResult != null)
                    {
                        barcoResult.BarcosTramites = barcoResult.BarcosTramites?.OrderByDescending(t => t.FechaInicio).ToList();

                        // Asignar el barco encontrado a la lista de barcos de la empresa (para mostrar detalles)
                        barcosEmpresa = new List<BarcosDto> { barcoResult };
                        empresa.CodigoBarco = barcoResult.CodigoBarco;
                    }
                }
                else
                {
                    // Cargar todos los barcos con sus trámites y filtrar por CodigoBarco de la empresa
                    // Nota: El modelo actual es 1:1 (Empresa → Barco), pero mostramos como lista
                    string[] includesBarcos = new string[] { "BarcosTramites" };
                    
                    // Si la empresa tiene un CodigoBarco, buscar ese barco específico
                    if (empresa.CodigoBarco > 0)
                    {
                        var barcosResult = await servicioBarcos.GetAllAsync("api/Barcos", null, includesBarcos);
                        
                        // Filtrar en el cliente por CodigoBarco de la empresa
                        barcosEmpresa = barcosResult?
                            .Where(b => b.CodigoBarco == empresa.CodigoBarco)
                            .ToList() ?? new List<BarcosDto>();
                    }
                    else
                    {
                        barcosEmpresa = new List<BarcosDto>();
                    }
                }
                
                totalBarcos = barcosEmpresa.Count;

                // Calcular estadísticas de trámites de todos los barcos
                var hoy = DateTime.Now;
                var en30Dias = DateTime.Now.AddDays(30);

                totalTramites = barcosEmpresa.Sum(b => b.BarcosTramites?.Count ?? 0);
                
                var todosTramites = barcosEmpresa
                    .SelectMany(b => b.BarcosTramites ?? new List<BarcosTramitesDto>())
                    .ToList();

                tramitesVigentes = todosTramites.Count(t => t.FechaFin.HasValue && t.FechaFin.Value > hoy);
                tramitesPorVencer = todosTramites.Count(t => t.FechaFin.HasValue && t.FechaFin.Value > hoy && t.FechaFin.Value <= en30Dias);
                tramitesVencidos = todosTramites.Count(t => t.FechaFin.HasValue && t.FechaFin.Value <= hoy);

                // Cargar usuarios de la empresa
                var usuariosResult = await servicioUsuarios.GetAllAsync("api/Usuarios", new Dictionary<string, string>(), Array.Empty<string>());
                usuariosEmpresa = usuariosResult?
                    .Where(u => u.CodigoEmpresa == CodigoEmpresa)
                    .OrderBy(u => u.Nombre)
                    .ToList() ?? new List<UsuarioDto>();

                isLoading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos de la empresa");
                isLoading = false;
                StateHasChanged();
            }
        }

        #region Gestión de Barcos

        private void MostrarFormularioBarco()
        {
            barcoEditando = null;
            nuevoBarco = new BarcosDto
            {
                Censo = string.Empty,
                FechaAlta = DateTime.Now,
                Activo = true
            };
            mostrarFormBarco = true;
        }

        private void EditarBarco(BarcosDto barco)
        {
            barcoEditando = barco;
            nuevoBarco = new BarcosDto
            {
                CodigoBarco = barco.CodigoBarco,
                NombreB = barco.NombreB,
                Matricula = barco.Matricula,
                Censo = barco.Censo,
                CapitanNombre = barco.CapitanNombre,
                Puerto = barco.Puerto,
                FechaAlta = barco.FechaAlta,
                Activo = barco.Activo
            };
            mostrarFormBarco = true;
        }

        private void CerrarFormularioBarco()
        {
            mostrarFormBarco = false;
            barcoEditando = null;
            nuevoBarco = new();
        }

        private async Task GuardarBarco()
        {
            if (string.IsNullOrWhiteSpace(nuevoBarco.NombreB) || 
                nuevoBarco.CodigoBarco == 0)
            {
                return;
            }

            try
            {
                if (barcoEditando != null)
                {
                    // Actualizar barco existente
                    var resultado = await servicioBarcos.UpdateAsync($"api/Barcos/{nuevoBarco.CodigoBarco}", nuevoBarco);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosEmpresa();
                        CerrarFormularioBarco();
                    }
                }
                else
                {
                    // Crear nuevo barco
                    var resultado = await servicioBarcos.CreateAsync("api/Barcos", nuevoBarco);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosEmpresa();
                        CerrarFormularioBarco();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar barco");
            }
        }

        private async Task EliminarBarco(long codigoBarco)
        {
            try
            {
                await servicioBarcos.DeleteAsync($"api/Barcos/", codigoBarco);
                await CargarDatosEmpresa();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar barco con CodigoBarco: {CodigoBarco}", codigoBarco);
            }
        }

        private void NavegarADetalleBarco(long codigoBarco)
        {
            NavigationManager.NavigateTo($"/barco/empresa/{CodigoEmpresa}/tramites/{codigoBarco}");
        }

        #endregion

        #region Gestión de Usuarios

        private void MostrarFormularioUsuario()
        {
            nuevoUsuario = new UsuarioDto
            {
                CodigoEmpresa = empresa?.CodigoEmpresa,
                //EmpresaId = empresa?.Barco?.Id,
                Activo = true,
                Rol = "Cliente",
                FechaRegistro = DateTime.UtcNow
            };
            mostrarFormUsuario = true;
        }

        private void CerrarFormularioUsuario()
        {
            mostrarFormUsuario = false;
            nuevoUsuario = new();
        }

        private async Task GuardarUsuario()
        {
            if (string.IsNullOrWhiteSpace(nuevoUsuario.Nombre) || 
                string.IsNullOrWhiteSpace(nuevoUsuario.NIFAcceso))
            {
                return;
            }

            try
            {
                // Validar que el NIF no esté duplicado
                var usuariosExistentes = await servicioUsuarios.GetAllAsync("api/Usuarios", new Dictionary<string, string>(), Array.Empty<string>());
                if (usuariosExistentes?.Any(u => u.NIFAcceso == nuevoUsuario.NIFAcceso) == true)
                {
                    _logger.LogWarning("El NIF {NIF} ya está registrado", nuevoUsuario.NIFAcceso);
                    return;
                }

                // Si no hay email de avisos, usar el email principal
                if (string.IsNullOrWhiteSpace(nuevoUsuario.EMailAvisos))
                {
                    nuevoUsuario.EMailAvisos = nuevoUsuario.EMail;
                }

                // Si no hay nombre de usuario, generar uno basado en el email
                if (string.IsNullOrWhiteSpace(nuevoUsuario.NombreUsuario) && !string.IsNullOrWhiteSpace(nuevoUsuario.EMail))
                {
                    nuevoUsuario.NombreUsuario = nuevoUsuario.EMail.Split('@')[0];
                }

                // Guardar el usuario usando el servicio
                var resultado = await servicioUsuarios.CreateAsync("api/Usuarios", nuevoUsuario);

                if (resultado != null)
                {
                    // Recargar datos
                    await CargarDatosEmpresa();
                    CerrarFormularioUsuario();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar usuario");
            }
        }

        #endregion

        #region Métodos Auxiliares

        private string GetInicialesEmpresa(string nombreEmpresa)
        {
            if (string.IsNullOrWhiteSpace(nombreEmpresa))
                return "??";

            var palabras = nombreEmpresa.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (palabras.Length == 1)
                return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();

            return (palabras[0][0].ToString() + palabras[1][0].ToString()).ToUpper();
        }

        private string GetInicialesUsuario(string? nombre, string? apellidos)
        {
            var inicial1 = !string.IsNullOrWhiteSpace(nombre) ? nombre[0].ToString() : "?";
            var inicial2 = !string.IsNullOrWhiteSpace(apellidos) ? apellidos[0].ToString() : "?";
            return (inicial1 + inicial2).ToUpper();
        }

        private string GetInicialesBarco(string? nombreBarco)
        {
            if (string.IsNullOrWhiteSpace(nombreBarco))
                return "??";

            var palabras = nombreBarco.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (palabras.Length == 1)
                return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();
            
            return (palabras[0][0].ToString() + palabras[1][0].ToString()).ToUpper();
        }

        #endregion
    }
}
