using CRM.Dtos;
using CRM.V3.Shared.Interfaces;
using Microsoft.AspNetCore.Components;

namespace CRM.V3.Shared.Pages
{
    public partial class DetalleBarco : ComponentBase
    {
        [Parameter]
        public string CodigoEmpresa { get; set; } = string.Empty;

        [Parameter]
        public string CodigoBarco { get; set; } = string.Empty;

        // Estado de carga
        private bool isLoading = true;
        private string tabActiva = "tramites";

        // Datos del barco y empresa
        private BarcosDto? barco;
        private EmpresasDto? empresa;

        // Estadísticas de trámites
        private int totalTramites = 0;
        private int tramitesVigentes = 0;
        private int tramitesPorVencer = 0;
        private int tramitesVencidos = 0;

        // Colecciones
        private List<BarcosTramitesDto> tramites = new();
        private List<UsuarioDto> usuariosBarco = new();

        // Formularios
        private bool mostrarFormTramite = false;
        private bool mostrarFormUsuario = false;
        private BarcosTramitesDto nuevoTramite = new();
        private UsuarioDto nuevoUsuario = new();
        private BarcosTramitesDto? tramiteEditando = null;
        private UsuarioDto? usuarioEditando = null;

        override protected async Task OnInitializedAsync()
        {
            try
            {
                Console.WriteLine("DetalleBarco OnInitializedAsync: Iniciando carga...");

                await CargarDatosBarco();

                Console.WriteLine($"DetalleBarco OnInitializedAsync: Carga completada. Barco: {barco?.NombreB}, Trámites: {tramites.Count}");

                isLoading = false;
                StateHasChanged(); // Forzar actualización de UI
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("DetalleBarco OnInitializedAsync: Operación cancelada");
                isLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DetalleBarco OnInitializedAsync: Error - {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                isLoading = false;
                StateHasChanged();
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(CodigoBarco))
            {
                await CargarDatosBarco();
            }
        }

        private async Task CargarDatosBarco()
        {
            try
            {
                Console.WriteLine($"CargarDatosBarco: Iniciando para CodigoBarco={CodigoBarco}, CodigoEmpresa={CodigoEmpresa}");
                isLoading = true;

                // Cargar barco con trámites
                string[] includesBarcos = new string[] { "BarcosTramites" };
                Console.WriteLine("CargarDatosBarco: Llamando a API Barcos...");

                Dictionary<string, string> filtros = new Dictionary<string, string>
                {
                    { "CodigoBarco", CodigoBarco }
                };

                var barcosResult = await servicioBarcos.GetAllAsync("api/Barcos", filtros, includesBarcos);
                Console.WriteLine($"CargarDatosBarco: Resultado API Barcos - {barcosResult?.Count() ?? 0} barcos recibidos");
                
                // Si el filtro funciona correctamente en el backend, solo debería haber 1 resultado
                barco = barcosResult?.FirstOrDefault();
                Console.WriteLine($"CargarDatosBarco: Barco encontrado = {barco?.NombreB ?? "NULL"} (CodigoBarco: {barco?.CodigoBarco})");

                if (barco == null)
                {
                    Console.WriteLine("CargarDatosBarco: Barco no encontrado - finalizando");
                    isLoading = false;
                    StateHasChanged();
                    return;
                }

                // Cargar empresa
                string[] includesEmpresas = new string[] { "Barco" };
                Console.WriteLine("CargarDatosBarco: Llamando a API Empresa...");
                Dictionary<string, string> filtrosEmpresa = new Dictionary<string, string>
                {
                    { "CodigoEmpresa", CodigoEmpresa }
                };
                var empresasResult = await servicioEmpresas.GetAllAsync("api/Empresa", filtrosEmpresa, includesEmpresas);
                Console.WriteLine($"CargarDatosBarco: Resultado API Empresa - {empresasResult?.Count() ?? 0} empresas recibidas");
                
                empresa = empresasResult?.FirstOrDefault(e => e.CodigoEmpresa == CodigoEmpresa);
                Console.WriteLine($"CargarDatosBarco: Empresa encontrada = {empresa?.Empresa ?? "NULL"}");

                var tramitesLst = await servicioBarcosTramites.GetAllAsync("api/BarcosTramites", "ProximoEnvioAviso is not null");

                Console.WriteLine($"Tramites: {tramitesLst.Select(x=>x.CodigoBarco).ToList()}");

                // Cargar trámites del barco
                if (barco.BarcosTramites != null)
                {
                    tramites = barco.BarcosTramites.ToList();
                    totalTramites = tramites.Count;
                    Console.WriteLine($"CargarDatosBarco: Total de trámites = {totalTramites}");

                    // Clasificar trámites por fechas
                    var hoy = DateOnly.FromDateTime(DateTime.Now);
                    var en30Dias = DateOnly.FromDateTime(DateTime.Now.AddDays(30));

                    tramitesVigentes = tramites.Count(t => t.FechaFin > hoy);
                    tramitesPorVencer = tramites.Count(t => t.FechaFin > hoy && t.FechaFin <= en30Dias);
                    tramitesVencidos = tramites.Count(t => t.FechaFin <= hoy);
                    
                    Console.WriteLine($"CargarDatosBarco: Vigentes={tramitesVigentes}, Por Vencer={tramitesPorVencer}, Vencidos={tramitesVencidos}");
                }
                else
                {
                    Console.WriteLine("CargarDatosBarco: barco.BarcosTramites es NULL");

                    
                }

                // Cargar usuarios de la empresa/barco
                Console.WriteLine("CargarDatosBarco: Llamando a API Usuarios...");
                var usuariosResult = await servicioUsuarios.GetAllAsync("api/Usuarios", null, null);
                usuariosBarco = usuariosResult?
                    .Where(u => u.CodigoEmpresa == CodigoEmpresa)
                    .OrderBy(u => u.Nombre)
                    .ToList() ?? new List<UsuarioDto>();
                Console.WriteLine($"CargarDatosBarco: Total usuarios = {usuariosBarco.Count}");

                Console.WriteLine("CargarDatosBarco: Carga completada exitosamente");
                isLoading = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CargarDatosBarco: ERROR - {ex.Message}");
                Console.WriteLine($"CargarDatosBarco: StackTrace - {ex.StackTrace}");
                isLoading = false;
                StateHasChanged();
            }
        }

        #region Gestión de Trámites

        private void MostrarFormularioTramite()
        {
            tramiteEditando = null;
            nuevoTramite = new BarcosTramitesDto
            {
                CodigoBarco = barco!.CodigoBarco,
                CodigoEmpresa = empresa?.CodigoEmpresa,
                CensoBarco = barco?.Censo ?? 0,
                FechaCreacion = DateOnly.FromDateTime(DateTime.Now),
                ListaEmailsEnvioAviso = string.Empty,
                DiasAvisoTramite = 30
            };
            mostrarFormTramite = true;
        }

        private void EditarTramite(BarcosTramitesDto tramite)
        {
            tramiteEditando = tramite;
            nuevoTramite = new BarcosTramitesDto
            {
                Id = tramite.Id,
                CodigoBarco = tramite.CodigoBarco,
                CodigoEmpresa = tramite.CodigoEmpresa,
                CensoBarco = tramite.CensoBarco,
                Certificado = tramite.Certificado,
                FechaInicio = tramite.FechaInicio,
                FechaFin = tramite.FechaFin,
                FechaAviso = tramite.FechaAviso,
                DiasAvisoTramite = tramite.DiasAvisoTramite,
                ListaEmailsEnvioAviso = tramite.ListaEmailsEnvioAviso,
                Observaciones = tramite.Observaciones,
                FechaCreacion = tramite.FechaCreacion
            };
            mostrarFormTramite = true;
        }

        private void CerrarFormularioTramite()
        {
            mostrarFormTramite = false;
            tramiteEditando = null;
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
                    nuevoTramite.FechaInicio = DateOnly.FromDateTime(DateTime.Now);
                }
                if (nuevoTramite.FechaFin == default)
                {
                    nuevoTramite.FechaFin = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
                }
                if (nuevoTramite.FechaAviso == default)
                {
                    nuevoTramite.FechaAviso = nuevoTramite.FechaFin.AddDays(-nuevoTramite.DiasAvisoTramite);
                }

                if (tramiteEditando != null)
                {
                    // Actualizar trámite existente
                    var resultado = await servicioBarcosTramites.UpdateAsync($"api/BarcosTramite/{nuevoTramite.Id}", nuevoTramite);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosBarco();
                        CerrarFormularioTramite();
                    }
                }
                else
                {
                    // Crear nuevo trámite
                    var resultado = await servicioBarcosTramites.CreateAsync("api/BarcosTramite", nuevoTramite);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosBarco();
                        CerrarFormularioTramite();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar trámite: {ex.Message}");
            }
        }

        private async Task EliminarTramite(Guid tramiteId)
        {
            try
            {
                await servicioBarcosTramites.DeleteAsync($"api/BarcosTramite/",tramiteId);
                await CargarDatosBarco();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar trámite: {ex.Message}");
            }
        }

        #endregion

        #region Gestión de Usuarios

        private void MostrarFormularioUsuario()
        {
            usuarioEditando = null;
            nuevoUsuario = new UsuarioDto
            {
                CodigoEmpresa = empresa?.CodigoEmpresa,
                // Note: Using Barco.Id for EmpresaId to match pattern in DetalleEmpresa.razor.cs
                // This may need review if there's a separate empresa.Id field available
                //EmpresaId = empresa?.Barco?.CodigoEmpresa,
                Activo = true,
                Rol = "Cliente",
                FechaRegistro = DateTime.UtcNow
            };
            mostrarFormUsuario = true;
        }

        private void EditarUsuario(UsuarioDto usuario)
        {
            usuarioEditando = usuario;
            nuevoUsuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                EMail = usuario.EMail,
                Telefono = usuario.Telefono,
                NIFAcceso = usuario.NIFAcceso,
                Rol = usuario.Rol,
                EMailAvisos = usuario.EMailAvisos,
                Activo = usuario.Activo,
                CodigoEmpresa = usuario.CodigoEmpresa,
                EmpresaId = usuario.EmpresaId,
                FechaRegistro = usuario.FechaRegistro,
                NombreUsuario = usuario.NombreUsuario
            };
            mostrarFormUsuario = true;
        }

        private void CerrarFormularioUsuario()
        {
            mostrarFormUsuario = false;
            usuarioEditando = null;
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

                if (usuarioEditando != null)
                {
                    // Actualizar usuario existente
                    var resultado = await servicioUsuarios.UpdateAsync($"api/Usuario/{nuevoUsuario.Id}", nuevoUsuario);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosBarco();
                        CerrarFormularioUsuario();
                    }
                }
                else
                {
                    // Validar que el NIF no esté duplicado solo al crear
                    var usuariosExistentes = await servicioUsuarios.GetAllAsync("api/Usuarios", null, null);
                    if (usuariosExistentes?.Any(u => u.NIFAcceso == nuevoUsuario.NIFAcceso) == true)
                    {
                        Console.WriteLine("El NIF ya está registrado");
                        return;
                    }

                    // Crear nuevo usuario
                    var resultado = await servicioUsuarios.CreateAsync("api/Usuarios", nuevoUsuario);
                    if (resultado != null && resultado.Success)
                    {
                        await CargarDatosBarco();
                        CerrarFormularioUsuario();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar usuario: {ex.Message}");
            }
        }

        private async Task EliminarUsuario(Guid usuarioId)
        {
            try
            {
                await servicioUsuarios.DeleteAsync($"api/Usuario/",usuarioId);
                await CargarDatosBarco();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar usuario: {ex.Message}");
            }
        }

        #endregion

        #region Métodos Auxiliares

        private string GetInicialesUsuario(string? nombre, string? apellidos)
        {
            var inicial1 = !string.IsNullOrWhiteSpace(nombre) ? nombre[0].ToString() : "?";
            var inicial2 = !string.IsNullOrWhiteSpace(apellidos) ? apellidos[0].ToString() : "?";
            return (inicial1 + inicial2).ToUpper();
        }

        #endregion
    }
}
