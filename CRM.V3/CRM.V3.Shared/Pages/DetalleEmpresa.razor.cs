using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CRM.V3.Shared.Pages
{
    public partial class DetalleEmpresa : ComponentBase
    {
        [Parameter]
        public string CodigoEmpresa { get; set; } = string.Empty;

        // Estado de carga
        private bool isLoading = true;
        private string tabActiva = "tramites";

        // Datos de la empresa
        private EmpresasDto? empresa;
        private BarcosDto? barco;

        // Estadísticas de trámites
        private int totalTramites = 0;
        private int tramitesVigentes = 0;
        private int tramitesPorVencer = 0;
        private int tramitesVencidos = 0;

        // Colecciones
        private List<BarcosTramitesDto> tramites = new();
        private List<UsuarioDto> usuariosEmpresa = new();

        // Formularios
        private bool mostrarFormTramite = false;
        private bool mostrarFormUsuario = false;
        private BarcosTramitesDto nuevoTramite = new();
        private UsuarioDto nuevoUsuario = new();

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

                // Cargar empresa con include anidado: Barco y sus BarcosTramites
                string[] includesEmpresas = new string[] { "Barco.BarcosTramites" };
                Dictionary<string, string> filtrosEmpresa = new Dictionary<string, string>
                {
                    { "CodigoEmpresa", CodigoEmpresa }
                };
                var empresasResult = await servicioEmpresas.GetAllAsync("api/Empresa", filtrosEmpresa, includesEmpresas);
                empresa = empresasResult?.FirstOrDefault(e => e.CodigoEmpresa == CodigoEmpresa);

                if (empresa == null)
                {
                    isLoading = false;
                    StateHasChanged();
                    return;
                }

                // Obtener barco y trámites directamente desde la empresa (ya vienen con el include)
                barco = empresa.Barco;
                
                if (barco?.BarcosTramites != null)
                {
                    tramites = barco.BarcosTramites.ToList();
                    totalTramites = tramites.Count;

                    // Clasificar trámites por fechas
                    var hoy = DateOnly.FromDateTime(DateTime.Now);
                    var en30Dias = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

                    tramitesVigentes = tramites.Count(t => t.FechaFin.HasValue && DateOnly.FromDateTime(t.FechaFin.Value) > hoy);
                    tramitesPorVencer = tramites.Count(t => t.FechaFin.HasValue && DateOnly.FromDateTime(t.FechaFin.Value) > hoy && DateOnly.FromDateTime(t.FechaFin.Value) <= en30Dias);
                    tramitesVencidos = tramites.Count(t => t.FechaFin.HasValue && DateOnly.FromDateTime(t.FechaFin.Value) <= hoy);
                }

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
                Console.WriteLine($"Error al cargar datos de la empresa: {ex.Message}");
                isLoading = false;
                StateHasChanged();
            }
        }

        #region Gestión de Trámites

        private void MostrarFormularioTramite()
        {
            nuevoTramite = new BarcosTramitesDto
            {
                CodigoBarco = barco!.CodigoBarco,
                CodigoEmpresa = empresa?.CodigoEmpresa,
                CensoBarco = barco?.Censo ?? string.Empty,
                FechaCreacion = DateTime.Now,
                ListaEmailsEnvioAviso = string.Empty,
                DiasAvisoTramite = 30
            };
            mostrarFormTramite = true;
        }

        private void CerrarFormularioTramite()
        {
            mostrarFormTramite = false;
            nuevoTramite = new();
        }

        private async Task GuardarTramite()
        {
            if (string.IsNullOrWhiteSpace(nuevoTramite.Certificado))
            {
                return;
            }

            try
            {
                // Asegurarse de que las fechas estén configuradas
                if (nuevoTramite.FechaInicio == default)
                {
                    nuevoTramite.FechaInicio = DateTime.Now;
                }
                if (nuevoTramite.FechaFin == default)
                {
                    nuevoTramite.FechaFin = DateTime.Now.AddYears(1);
                }
                if (nuevoTramite.FechaAviso == default)
                {
                    if (nuevoTramite.FechaFin.HasValue && nuevoTramite.DiasAvisoTramite.HasValue)
                    {
                        nuevoTramite.FechaAviso = nuevoTramite.FechaFin.Value.AddDays(-nuevoTramite.DiasAvisoTramite.Value);
                    }
                }

                // Guardar el trámite usando el servicio
                var resultado = await servicioBarcosTramites.CreateAsync("api/BarcosTramite", nuevoTramite);

                if (resultado != null)
                {
                    // Recargar datos
                    await CargarDatosEmpresa();
                    CerrarFormularioTramite();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar trámite: {ex.Message}");
            }
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
                    Console.WriteLine("El NIF ya está registrado");
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
                Console.WriteLine($"Error al guardar usuario: {ex.Message}");
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

        #endregion
    }
}
